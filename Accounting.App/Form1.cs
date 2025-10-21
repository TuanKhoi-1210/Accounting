using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

using Microsoft.EntityFrameworkCore;
using Accounting.Infrastructure;
using Accounting.Application.Services;
using Accounting.Application.DTOs;

namespace Accounting.App
{
    public partial class Form1 : Form

    {
        private static readonly JsonSerializerOptions JsonInOptions = new()
        {
            PropertyNameCaseInsensitive = true // <— CHÌA KHOÁ
        };
        private AccountingDbContext _db = default!;
        private PurchaseService _service = default!;

        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_LoadAsync;
        }

        // ===== Khởi tạo =======================================================
        private async void Form1_LoadAsync(object? sender, EventArgs e)
        {
            try
            {
                // 1) Kết nối EF Core (ĐỔI Server=... theo máy bạn nếu khác)
                var connStr =
                    @"Server=localhost\SQLEXPRESS;Database=acc_demo;Trusted_Connection=True;TrustServerCertificate=True";

                var options = new DbContextOptionsBuilder<AccountingDbContext>()
                    .UseSqlServer(connStr)
                    .EnableSensitiveDataLogging() // chỉ dùng khi dev/debug
                    .Options;

                _db = new AccountingDbContext(options);

                // Tạo DB nếu chưa có (nếu bạn dùng migrations thì có thể thay bằng Migrate())
                await _db.Database.EnsureCreatedAsync();

                // ---- Test kết nối + test truy vấn nhẹ
                var canConnect = await _db.Database.CanConnectAsync();
                int nccCount = 0;
                try { nccCount = await _db.NhaCungCap.CountAsync(); } catch { /* bảng có thể chưa có dữ liệu */ }

                MessageBox.Show(
                    canConnect
                        ? $"Kết nối SQL Server: OK\n- Chuỗi: {connStr}\n- NCC hiện có: {nccCount}"
                        : $"KHÔNG kết nối được SQL Server!\n- Chuỗi: {connStr}",
                    "Kiểm tra kết nối DB",
                    MessageBoxButtons.OK,
                    canConnect ? MessageBoxIcon.Information : MessageBoxIcon.Error
                );

                // 2) Service nghiệp vụ
                _service = new PurchaseService(_db);

                // 3) Khởi tạo WebView2
                await EnsureWebViewAsync();

                // 4) Nạp wwwroot/index.html (đã Copy to Output)
                var indexPath = Path.Combine(
                    System.Windows.Forms.Application.StartupPath, // tránh xung đột namespace 'Application'
                    "wwwroot", "index.html");

                if (!File.Exists(indexPath))
                {
                    MessageBox.Show($"Không tìm thấy file HTML: {indexPath}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                webView.Source = new Uri(indexPath); // file:///C:/.../wwwroot/index.html

                // 5) Lắng nghe message từ JS
                webView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khởi tạo: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task EnsureWebViewAsync()
        {
            if (webView.CoreWebView2 == null)
            {
                var env = await CoreWebView2Environment.CreateAsync();
                await webView.EnsureCoreWebView2Async(env);

                // Tùy chọn devtools (tiện debug, có thể tắt khi release)
                webView.CoreWebView2.Settings.AreDevToolsEnabled = true;
                webView.CoreWebView2.Settings.IsZoomControlEnabled = true;
            }
        }

        // ===== Bridge JS ↔ C# =================================================
        private async void CoreWebView2_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                // Nhận JSON từ JS
                var json = e.WebMessageAsJson;
                var msg = JsonSerializer.Deserialize<JsonElement>(json);
                var cmd = msg.GetProperty("cmd").GetString();

                switch (cmd)
                {
                    // Danh sách đơn mua
                    case "list":
                    case "listDonMua":
                        {
                            var data = await _service.ListDonMuaAsync();
                            Reply(data);
                            break;
                        }

                    // Tạo đơn mua
                    case "createDonMua":
                        {
                            var donmua = JsonSerializer.Deserialize<DonMuaCreateDto>(
                                msg.GetProperty("donmua").GetRawText(), JsonInOptions);

                            var lines = msg.TryGetProperty("lines", out var rawLines)
                                ? JsonSerializer.Deserialize<DonMuaDongDto[]>(rawLines.GetRawText(), JsonInOptions)
                                : Array.Empty<DonMuaDongDto>();

                            var created = await _service.CreateDonMuaAsync(donmua!, lines);
                            Reply(created);
                            break;
                        }

                    // Thêm dòng vào đơn mua
                    case "addDong":
                        {
                            var donMuaId = msg.GetProperty("donMuaId").GetInt32();
                            var dong = JsonSerializer.Deserialize<DonMuaDongDto>(
                                msg.GetProperty("dong").GetRawText(), JsonInOptions);

                            var updated = await _service.AddDongDonMuaAsync(donMuaId, dong!);
                            Reply(updated);
                            break;
                        }

                    // Tạo Phiếu nhập từ đơn
                    case "createPhieuNhap":
                        {
                            var pn = JsonSerializer.Deserialize<PhieuNhapCreateDto>(
                                msg.GetProperty("phieuNhap").GetRawText(), JsonInOptions);

                            var created = await _service.CreatePhieuNhapTuDonAsync(pn!);
                            Reply(created);
                            break;
                        }

                    // Tạo Hóa đơn mua từ đơn
                    case "createHoaDonMua":
                        {
                            var hd = JsonSerializer.Deserialize<HoaDonMuaCreateDto>(
                                msg.GetProperty("hoaDon").GetRawText(), JsonInOptions);

                            var created = await _service.CreateHoaDonMuaTuDonAsync(hd!);
                            Reply(created);
                            break;
                        }

                    default:
                        Reply(new { error = $"Unknown cmd: {cmd}" });
                        break;
                }
            }
            catch (Exception ex)
            {
                // Trả về toàn bộ lỗi chi tiết (bao gồm stack trace)
                Reply(new { error = ex.ToString() });
            }

        }

        // ===== Helper gửi dữ liệu về JS ======================================
        private void Reply(object data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                });
                webView.CoreWebView2.PostWebMessageAsString(json);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Reply error: " + ex.Message);
            }
        }
    }
}
