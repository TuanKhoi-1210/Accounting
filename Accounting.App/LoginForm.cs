using Accounting.Application.Services;
using Accounting.Domain.Entities;
using Accounting.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
// alias để phân biệt Application của WinForms và Accounting.Application
using WinApp = System.Windows.Forms.Application;

namespace Accounting.App
{
    public class LoginForm : Form
    {
        private readonly DbContextOptions<AccountingDbContext> _options;
        private WebView2 webView = null!;

        public LoginForm(DbContextOptions<AccountingDbContext> options)
        {
            _options = options;

            // ==== kích thước & hiển thị ====
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1000, 700);              // ngăn quá nhỏ
            FormBorderStyle = FormBorderStyle.Sizable;       // cho resize

            webView = new WebView2 { Dock = DockStyle.Fill }; // chiếm hết form
            Controls.Add(webView);

            Shown += async (_, __) => await InitAsync();
        }

        private async Task InitAsync()
        {
            try
            {
                var userData = Path.Combine(WinApp.LocalUserAppDataPath, "wv2-login");
                webView.CreationProperties = new CoreWebView2CreationProperties
                {
                    UserDataFolder = userData
                };

                // Retry tối đa 3 lần nếu dính E_ABORT (form trước chưa xả xong)
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        var env = await CoreWebView2Environment.CreateAsync(userDataFolder: userData);
                        await webView.EnsureCoreWebView2Async(env);
                        break;
                    }
                    catch (COMException ex) when ((uint)ex.HResult == 0x80004004) // E_ABORT
                    {
                        if (i == 2) throw;      // hết số lần thử
                        await Task.Delay(300);  // chờ WebView2 cũ xả xong
                    }
                }

                var wwwroot = Path.Combine(WinApp.StartupPath, "wwwroot");
                webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "app", wwwroot, CoreWebView2HostResourceAccessKind.Allow);

                webView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
                webView.Source = new Uri("https://app/login.html");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khởi tạo WebView2: " + ex.Message, "Lỗi");
            }
        }

        private async void CoreWebView2_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                var json = e.WebMessageAsJson;
                var obj = JsonSerializer.Deserialize<JsonElement>(json);
                if (!obj.TryGetProperty("cmd", out var cmdEl)) return;

                var cmd = cmdEl.GetString();
                if (cmd == "login")
                {
                    var username = obj.GetProperty("username").GetString() ?? "";
                    var password = obj.GetProperty("password").GetString() ?? "";

                    using var db = new AccountingDbContext(_options);
                    var auth = new AuthService(db);
                    var (user, roles) = await auth.LoginAsync(username.Trim(), password);

                    if (user == null)
                    {
                        var fail = JsonSerializer.Serialize(new
                        {
                            type = "login_fail",
                            message = "Sai tài khoản hoặc mật khẩu!"
                        });
                        webView.CoreWebView2.PostWebMessageAsJson(fail);
                        return;
                    }

                    var ok = JsonSerializer.Serialize(new { type = "login_ok", user = new { username = user.Username, fullName = user.FullName }, roles });
                    webView.CoreWebView2.PostWebMessageAsJson(ok);

                    // Mở form chính
                    Hide();
                    var main = new Form1();
                    main.FormClosed += (_, __) => Close();
                    main.Show();
                }
            }
            catch (Exception ex)
            {
                var fail = JsonSerializer.Serialize(new
                {
                    type = "login_fail",
                    message = "Lỗi: " + ex.Message
                });
                webView.CoreWebView2?.PostWebMessageAsJson(fail);
            }
        }
    }
}
