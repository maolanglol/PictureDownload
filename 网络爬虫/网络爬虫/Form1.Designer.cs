namespace 网络爬虫
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.url = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.saveUrlPath = new System.Windows.Forms.TextBox();
            this.selectSaveUrl = new System.Windows.Forms.Button();
            this.showListUrl = new System.Windows.Forms.ListBox();
            this.beginDownload = new System.Windows.Forms.Button();
            this.stopDownload = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(65, 50);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "所爬网址：";
            // 
            // url
            // 
            this.url.Location = new System.Drawing.Point(147, 47);
            this.url.Name = "url";
            this.url.Size = new System.Drawing.Size(385, 21);
            this.url.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(65, 93);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "保存位置：";
            // 
            // saveUrlPath
            // 
            this.saveUrlPath.Location = new System.Drawing.Point(147, 93);
            this.saveUrlPath.Name = "saveUrlPath";
            this.saveUrlPath.Size = new System.Drawing.Size(385, 21);
            this.saveUrlPath.TabIndex = 3;
            // 
            // selectSaveUrl
            // 
            this.selectSaveUrl.Location = new System.Drawing.Point(538, 93);
            this.selectSaveUrl.Name = "selectSaveUrl";
            this.selectSaveUrl.Size = new System.Drawing.Size(31, 23);
            this.selectSaveUrl.TabIndex = 4;
            this.selectSaveUrl.Text = "...";
            this.selectSaveUrl.UseVisualStyleBackColor = true;
            this.selectSaveUrl.Click += new System.EventHandler(this.selectSaveUrl_Click);
            // 
            // showListUrl
            // 
            this.showListUrl.FormattingEnabled = true;
            this.showListUrl.ItemHeight = 12;
            this.showListUrl.Location = new System.Drawing.Point(67, 152);
            this.showListUrl.Name = "showListUrl";
            this.showListUrl.Size = new System.Drawing.Size(626, 256);
            this.showListUrl.TabIndex = 5;
            // 
            // beginDownload
            // 
            this.beginDownload.Location = new System.Drawing.Point(196, 417);
            this.beginDownload.Name = "beginDownload";
            this.beginDownload.Size = new System.Drawing.Size(75, 23);
            this.beginDownload.TabIndex = 6;
            this.beginDownload.Text = "开始爬图";
            this.beginDownload.UseVisualStyleBackColor = true;
            this.beginDownload.Click += new System.EventHandler(this.beginDownload_Click);
            // 
            // stopDownload
            // 
            this.stopDownload.Enabled = false;
            this.stopDownload.Location = new System.Drawing.Point(333, 417);
            this.stopDownload.Name = "stopDownload";
            this.stopDownload.Size = new System.Drawing.Size(75, 23);
            this.stopDownload.TabIndex = 7;
            this.stopDownload.Text = "停止爬图";
            this.stopDownload.UseVisualStyleBackColor = true;
            this.stopDownload.Click += new System.EventHandler(this.stopDownload_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(734, 452);
            this.Controls.Add(this.stopDownload);
            this.Controls.Add(this.beginDownload);
            this.Controls.Add(this.showListUrl);
            this.Controls.Add(this.selectSaveUrl);
            this.Controls.Add(this.saveUrlPath);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.url);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox url;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox saveUrlPath;
        private System.Windows.Forms.Button selectSaveUrl;
        private System.Windows.Forms.ListBox showListUrl;
        private System.Windows.Forms.Button beginDownload;
        private System.Windows.Forms.Button stopDownload;
    }
}

