using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;

namespace Accounting.App
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_LoadAsync;
        }

        private async void Form1_LoadAsync(object? sender, EventArgs e)
        {
            await EnsureWebViewAsync();

            // Nạp file wwwroot/index.html ở thư mục output (bin/…)
            var indexPath = Path.Combine(
                System.Windows.Forms.Application.StartupPath,
                "wwwroot", "index.html");

            if (!File.Exists(indexPath))
            {
                MessageBox.Show($"Không tìm thấy {indexPath}");
                return;
            }

            webView.Source = new Uri(indexPath);
        }

        private async Task EnsureWebViewAsync()
        {
            if (webView.CoreWebView2 == null)
            {
                var env = await CoreWebView2Environment.CreateAsync();
                await webView.EnsureCoreWebView2Async(env);
            }
        }
    }
}
