using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace 网络爬虫
{
   public class DownLoad
    {
       private string _rootUrl = null;//存放根URL，所输入的链接
       private string _baseUrl = null;//存放处理后的根URL，所输入的链接
       private string _path = null;//存放所选择保存图片的路径
       private bool _stop = true;//判断是否点击停止下载
       private System.Threading.Timer _checkTimer = null;//一个时间控制器，也是一个线程
       private readonly object _locker = new object();//排他锁，保证请求的完整性
       private int _maxTime = 2 * 60 * 1000;//请求的最大时间
       private static Encoding GB18030 = Encoding.GetEncoding("GB18030");//编码格式，GB18030兼容GBK和GB2312
       private Encoding _encoding = GB18030;
       private int _index = 0;//索引标志，用于判断所下载的图片数量
       private bool[] _reqsBusy = null;//判断是否是正在工作，true为正在工作
       private int _reqCount = 4;//初始值，默认是四个线程工作
       private WorkingUnitCollection _workingSignals;//类的对象，用于判断是否工作

       private Dictionary<string, int> _urlsLoaded = new Dictionary<string, int>();//数据字典，用于存放未下载的链接
       private Dictionary<string, int> _urlsUnload = new Dictionary<string, int>();//数据字典，用于存放已经下载的链接
       public delegate void ContentsSavedHandler(string path, string url);//定义一个委托
       public event ContentsSavedHandler ContentsSaved = null;//定义一个事件，用于保存内容时触发
       public delegate void DownloadFinishHandler(int count);
       public event DownloadFinishHandler DownloadFinish = null;//定义一个事件，用于完成下载时触发

       /// <summary>
       /// 定义一个类，初始化请求时所需要的属性
       /// </summary>
       private class RequestState
       {
           /// <summary>
           /// 接收数据包的空间大小
           /// </summary>
           private const int BUFFER_SIZE = 131072;
           /// <summary>
           /// 接收数据包的buffer
           /// </summary>
           private byte[] _data = new byte[BUFFER_SIZE];
           /// <summary>
           /// 存放所有接收到的字符
           /// </summary>
           private StringBuilder _sb = new StringBuilder();
           /// <summary>
           /// 请求
           /// </summary>
           public HttpWebRequest Req { get; private set; }
           /// <summary>
           /// 请求的URL
           /// </summary>
           public string Url { get; private set; }
           /// <summary>
           /// 此次请求的相对深度
           /// </summary>
           public int Depth { get; private set; }
           /// <summary>
           /// 工作实例的编号
           /// </summary>
           public int Index { get; private set; }
           /// <summary>
           /// 接收数据流
           /// </summary>
           public Stream ResStream { get; set; }

           public StringBuilder Html
           {
               get
               {
                   return _sb;
               }
           }

           public byte[] Data
           {
               get
               {
                   return _data;
               }
           }

           public int BufferSize
           {
               get
               {
                   return BUFFER_SIZE;
               }
           }

           public RequestState(HttpWebRequest req, string url, int depth,int index)
           {
               Req = req;
               Url = url;
               Depth = depth;
               Index = index;
           }
       }
       private class WorkingUnitCollection
       {
           private int _count;//工作数量
           //private AutoResetEvent[] _works;
           /// <summary>
           /// 是否工作
           /// </summary>
           private bool[] _busy;
           /// <summary>
           /// 正在工作的数量
           /// </summary>
           /// <param name="count"></param>
           public WorkingUnitCollection(int count)
           {
               _count = count;
               //_works = new AutoResetEvent[count];
               _busy = new bool[count];

               for (int i = 0; i < count; i++)
               {
                   //_works[i] = new AutoResetEvent(true);
                   _busy[i] = true;
               }
           }

           public void StartWorking(int index)
           {
               if (!_busy[index])
               {
                   _busy[index] = true;
                   //_works[index].Reset();
               }
           }

           public void FinishWorking(int index)
           {
               if (_busy[index])
               {
                   _busy[index] = false;
                   //_works[index].Set();
               }
           }

           public bool IsFinished()
           {
               bool notEnd = false;
               foreach (var b in _busy)
               {
                   notEnd |= b;
               }
               return !notEnd;
           }

           public void WaitAllFinished()
           {
               while (true)
               {
                   if (IsFinished())
                   {
                       break;
                   }
                   Thread.Sleep(1000);
               }
               //WaitHandle.WaitAll(_works);
           }

           public void AbortAllWork()
           {
               for (int i = 0; i < _count; i++)
               {
                   _busy[i] = false;
               }
           }
       }

        /// <summary>
        /// 下载根Url
        /// </summary>
        public string RootUrl
        {
            get
            {
                return _rootUrl;
            }
            set
            {
                if (value.Contains("http://"))
                {
                    _rootUrl = value;
                }
                else if ( value.Contains("https://"))
                {
                    _rootUrl = value;
                }
                else
                {
                    _rootUrl = "http://" + value;
                }
                _baseUrl = _rootUrl.Replace("www.", "");
                _baseUrl = _baseUrl.Replace("http://", "");
                _baseUrl = _baseUrl.Replace("https://", "");
                _baseUrl = _baseUrl.TrimEnd('/');
            }
        }
        /// <summary>
        /// 开始下载
        /// </summary>
        /// <param name="path">保存本地文件的目录</param>
        public void Download(string path)
        {
            if (string.IsNullOrEmpty(RootUrl))
            {
                return;
            }
            _path = path;
            Init();
            StartDownload();
        }

        private void Init()
        {
            _urlsLoaded.Clear();//已下载的数据字典集合清空
            _urlsUnload.Clear();//未下载的数据字典集合清空
            AddUrls(new string[1] { RootUrl }, 0);
            _index = 0;
            _reqsBusy = new bool[_reqCount];
            _workingSignals = new WorkingUnitCollection(_reqCount);
            _stop = false;
            ret = 0;
        }
        /// <summary>
        /// 终止下载
        /// </summary>
        public void Abort()
        {
            _stop = true;
            if (_workingSignals != null)
            {
                _workingSignals.AbortAllWork();
            }
        }
       /// <summary>
       /// 开始下载
       /// </summary>
        private void StartDownload()
        {
            _checkTimer = new System.Threading.Timer(new TimerCallback(CheckFinish), null, 0, 300);
            DispatchWork();
        }
       /// <summary>
       /// 线程回调函数，完成时调用
       /// </summary>
       /// <param name="param"></param>
        private void CheckFinish(object param)
        {
            if (_workingSignals.IsFinished())
            {
                _checkTimer.Dispose();
                _checkTimer = null;
                if (DownloadFinish != null)
                {
                    DownloadFinish(_index);
                }
            }
        }
        /// <summary>
        /// 给空闲的实例分配新任务了。
        /// </summary>
        private void DispatchWork()
        {
            if (_stop)
            {
                return;
            }

            for (int i = 0; i < _reqCount; i++)
            {
                if (!_reqsBusy[i])
                {
                    RequestResource(i);
                }
            }
            
        }
        /// <summary>
        /// 发送请求
        /// </summary>
        /// <param name="index"></param>
        private void RequestResource(int index)
        {
            int depth;
            string url = "";
            try
            {
                lock (_locker)
                {
                    if (_urlsUnload.Count <= 0)//判断是否还有未下载的URL
                    {
                        _workingSignals.FinishWorking(index);//设置工作实例的状态为Finished
                        return;
                    }
                    _reqsBusy[index] = true;
                    _workingSignals.StartWorking(index);
                    depth = _urlsUnload.First().Value;
                    url = _urlsUnload.First().Key;
                    _urlsLoaded.Add(url, depth);
                    _urlsUnload.Remove(url);
                }

                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "Get"; //请求方法
                req.Accept = "text/html"; //接受的内容
                req.UserAgent = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; Trident/4.0)"; //用户代理
                RequestState rs = new RequestState(req, url, depth,index);//回调方法的参数
                var result = req.BeginGetResponse(new AsyncCallback(ReceivedResource), rs);//异步请求
                ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle,
                        TimeoutCallback, rs, _maxTime, true);
            }
            catch (WebException we)
            {
                MessageBox.Show("RequestResource " + we.Message + url + we.Status);
            }
        }
       /// <summary>
       /// 请求超时的处理
       /// </summary>
       /// <param name="state"></param>
       /// <param name="timedOut"></param>
        private void TimeoutCallback(object state, bool timedOut)
        {
            if (timedOut)
            {
                RequestState rs = state as RequestState;
                if (rs != null)
                {
                    rs.Req.Abort();
                }
                _reqsBusy[rs.Index] = false;
                DispatchWork();
            }
        }
        /// <summary>
        /// 处理请求的响应
        /// </summary>
        /// <param name="ar"></param>
        private void ReceivedResource(IAsyncResult ar)
        {
            RequestState rs = (RequestState)ar.AsyncState;//得到请求时传入的参数
            HttpWebRequest req = rs.Req;
            string url = rs.Url;
            try
            {
                HttpWebResponse res = (HttpWebResponse)req.EndGetResponse(ar);
                if (_stop)
                {
                    res.Close();
                    req.Abort();
                    return;
                }
                if (res != null && res.StatusCode == HttpStatusCode.OK)//判断是否成功获取响应
                {
                    Stream resStream = res.GetResponseStream();//得到资源流
                    rs.ResStream = resStream;
                    var result = resStream.BeginRead(rs.Data, 0, rs.BufferSize,//异步请求读取数据
                        new AsyncCallback(ReceivedData), rs);
                }
                else
                {
                    res.Close();
                    rs.Req.Abort();
                    _reqsBusy[rs.Index] = false;
                    DispatchWork();
                }
            }
            catch (WebException we)
            {
                MessageBox.Show("ReceivedResource " + we.Message + url + we.Status);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        /// <summary>
        /// 接受数据并处理
        /// </summary>
        /// <param name="ar"></param>
        private void ReceivedData(IAsyncResult ar)
        {
            RequestState rs = (RequestState)ar.AsyncState;//获取参数
            HttpWebRequest req = rs.Req;
            Stream resStream = rs.ResStream;
            string url = rs.Url;
            int depth = rs.Depth;
            string html = null;
            int index = rs.Index;
            int read = 0;

            try
            {
                read = resStream.EndRead(ar);//获得数据读取结果
                if (_stop)
                {
                    rs.ResStream.Close();
                    req.Abort();
                    return;
                }
                if (read > 0)
                {
                    MemoryStream ms = new MemoryStream(rs.Data, 0, read);
                    StreamReader reader = new StreamReader(ms, _encoding);
                    string str = reader.ReadToEnd();//读取所有字符
                    rs.Html.Append(str);
                    var result = resStream.BeginRead(rs.Data, 0, rs.BufferSize,
                        new AsyncCallback(ReceivedData), rs);//再次异步请求读取数据
                    return;
                }
                html = rs.Html.ToString();
                SaveContents(html, url);
                string[] links = GetLinks(html);
                AddUrls(links, depth + 1);

                _reqsBusy[index] = false;
                DispatchWork();
            }
            catch (WebException we)
            {
                MessageBox.Show("ReceivedData Web " + we.Message + url + we.Status);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.GetType().ToString() + e.Message);
            }
        }
        /// <summary>
        /// 传入访问的文件匹配我们所需的链接
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private string[] GetLinks(string html)
        {
            const string pattern = @"http://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?";
            Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection m = r.Matches(html);
            string[] links = new string[m.Count];

            for (int i = 0; i < m.Count; i++)
            {
                links[i] = m[i].ToString();
            }
            return links;
        }
        /// <summary>
        /// 将读取的数据保存到本地
        /// </summary>
        /// <param name="html">字符串</param>
        /// <param name="url"></param>
        private void SaveContents(string html, string url)
        {
            if (string.IsNullOrEmpty(html))
            {
                return;
            }
            string path = "";
            lock (_locker)
            {
                if (_index == 0)
                {
                    _index++;
                }
                else
                {
                    string fileExtName = url.Substring(url.LastIndexOf(".") + 1).ToString();
                    string filepath = "";
                    if (fileExtName != "")
                    {
                        switch (fileExtName)
                        {
                            case "jpg":
                                filepath = _path + "\\" + "网络爬虫" + _index + ".jpg";
                                break;
                            case "gif":
                                filepath = _path + "\\" + "网络爬虫" + _index + ".gif";
                                break;
                            case "png":
                                filepath = _path + "\\" + "网络爬虫" + _index + ".png";
                                break;
                            default:
                                return;
                        }

                    }
                    _index++;
                    path = filepath;
                    WebClient mywebclient = new WebClient();
                    mywebclient.DownloadFile(url, filepath);
                }

            }
            if (ContentsSaved != null)
            {
                ContentsSaved(path, url);
            }
        }
        /// <summary>
        /// 判断链接url，并向集合_urlsUnload添加未下载的链接
        /// </summary>
        /// <param name="urls">链接</param>
        /// <param name="depth">深度</param>
        private void AddUrls(string[] urls, int depth)
        {
            if (depth >= 2)
            {
                return;
            }
            foreach (string url in urls)
            {
                string cleanUrl = url.Trim();
                int end = cleanUrl.IndexOf(' ');
                if (end > 0)
                {
                    cleanUrl = cleanUrl.Substring(0, end);
                }
                cleanUrl = cleanUrl.TrimEnd('/');
                if (UrlAvailable(cleanUrl))
                {
                    if (cleanUrl.Contains(_baseUrl))
                    {
                        _urlsUnload.Add(cleanUrl, depth);
                    }
                    else
                    {
                        _urlsUnload.Add(cleanUrl, depth);//自己添加2016-08-15
                        // 外链
                    }
                }
            }
        }
        int ret = 0;
        /// <summary>
        /// 判断是否是满足我们需要的url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private bool UrlAvailable(string url)
        {
            if (UrlExists(url))
            {
                return false;
            }
            bool re = false;

            if (ret == 0)
            {
                ret++;
                return true;
            }
           if (url.Contains(".jpg") || url.Contains(".gif") || url.Contains(".png") )
            {
                re = true;
            }
            return re;
        }
        /// <summary>
        /// 判断url是否存在
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private bool UrlExists(string url)
        {
            bool result = _urlsUnload.ContainsKey(url);
            result |= _urlsLoaded.ContainsKey(url);
            return result;
        }
        
    }
}
