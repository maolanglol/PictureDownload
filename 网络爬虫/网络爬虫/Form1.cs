using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows;

namespace 网络爬虫
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        DownLoad Dl = new DownLoad();
        private delegate void CSHandler(string arg0);
        private delegate void DFHandler(int arg1);

        private void selectSaveUrl_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog saveUrl = new FolderBrowserDialog();
            saveUrl.RootFolder = Environment.SpecialFolder.Desktop;
            saveUrl.Description = "";
            var result = saveUrl.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string savePath = saveUrl.SelectedPath;
                saveUrlPath.Text = savePath;
            }
        }

        private void beginDownload_Click(object sender, EventArgs e)
        {
            Dl.RootUrl = url.Text;
            Thread thread = new Thread(new ParameterizedThreadStart(Download));//开始下载的线程
            thread.Start(saveUrlPath.Text);//下载所保存的地址
            beginDownload.Enabled = false;
            beginDownload.Text = "爬图中...";
            stopDownload.Enabled = true;
        }

        private void Download(object obj)
        {
            Dl.Download(saveUrlPath.Text);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Dl.ContentsSaved += new DownLoad.ContentsSavedHandler(SaveDownLoad);
            Dl.DownloadFinish += new DownLoad.DownloadFinishHandler(Spider_DownloadFinish);
            url.Text = "http://news.sina.com.cn/";
        }
        void SaveDownLoad(string path, string url)
        {
           // showListUrl.Items.Add(path+url);
            CSHandler h = (p) =>
            {
                showListUrl.Items.Add(new { File = p });
            };
            this.Invoke(h,path);
           // Dispatcher.Invoke(h, path, url);
        }
        void Spider_DownloadFinish(int count)
        {
            DFHandler h = c =>
            {
                beginDownload.Enabled = true;
                beginDownload.Text = "开始爬图";
                stopDownload.Enabled = false;
                MessageBox.Show("完成爬图： " + c.ToString()+"个");
            };
            this.Invoke(h, count);
        }

        private void stopDownload_Click(object sender, EventArgs e)
        {
            Dl.Abort();
            beginDownload.Enabled = true;
            beginDownload.Text = "开始爬图";
            stopDownload.Enabled = false;
        }
    }
}
