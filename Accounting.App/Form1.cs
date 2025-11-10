using Accounting.Application.DTOs;
using Accounting.Application.Services;
using Accounting.Domain.Entities;
using Accounting.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System;
using System.Globalization;
using System.IO;
using System.Linq;

using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
namespace Accounting.App
{
    public partial class Form1 : Form
    {
        private static readonly JsonSerializerOptions JsonInOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private AccountingDbContext _db = default!;
        private PurchaseService _service = default!;
        private UserService _userService = default!;
        private UserDto? _currentUser;
        private CatalogService _catalog = default!;
        private ReportService _reportService = default!;
        private CongNoService _congNoService;
        private  DashboardService _dashboardService = default!;
        private BankService _bankService = default!;
        public DbSet<KiemKeQuy> KiemKeQuy { get; set; } = default!;
        private CashService _cashService = default!;
        private DbContextOptions<AccountingDbContext> _dbOptions = default!;
        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_LoadAsync;
        }

        // ===================== Khởi tạo =====================
        private async void Form1_LoadAsync(object? sender, EventArgs e)
        {
            try
            {
                var connStr =
                    "Server=.\\SQLEXPRESS04;Database=AccountingDB;Trusted_Connection=True;TrustServerCertificate=True";

                var options = new DbContextOptionsBuilder<AccountingDbContext>()
                    .UseSqlServer(connStr)
                    .EnableSensitiveDataLogging()
                    .Options;

                _dbOptions = new DbContextOptionsBuilder<AccountingDbContext>()
                    .UseSqlServer(connStr)
                    .EnableSensitiveDataLogging()
                    .Options;

                _db = new AccountingDbContext(_dbOptions);
                await _db.Database.EnsureCreatedAsync();
                _congNoService = new CongNoService(_db);

                var canConnect = await _db.Database.CanConnectAsync();
                int nccCount = 0;
                try { nccCount = await _db.NhaCungCap.CountAsync(); } catch { }

                MessageBox.Show(
                    canConnect
                        ? $"Kết nối SQL Server: OK\n- Chuỗi: {connStr}\n- NCC hiện có: {nccCount}"
                        : $"KHÔNG kết nối được SQL Server!\n- Chuỗi: {connStr}",
                    "Kiểm tra kết nối DB",
                    MessageBoxButtons.OK,
                    canConnect ? MessageBoxIcon.Information : MessageBoxIcon.Error
                );
                _dashboardService = new DashboardService(_db);
                _cashService = new CashService(_db);
                _bankService = new BankService(_db);
                _service = new PurchaseService(_db);
                _catalog = new CatalogService(_db);
                _reportService = new ReportService(_db);
                _userService = new UserService(_db);

                await EnsureWebViewAsync();

                var indexPath = Path.Combine(
                    System.Windows.Forms.Application.StartupPath,
                    "wwwroot", "index.html");

                if (!File.Exists(indexPath))
                {
                    MessageBox.Show($"Không tìm thấy file HTML: {indexPath}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                webView.Source = new Uri(indexPath);
                webView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khởi tạo: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static bool TryGetInt64(JsonElement obj, out long value, params string[] keys)
        {
            foreach (var k in keys)
            {
                if (obj.TryGetProperty(k, out var el))
                {
                    try { value = el.GetInt64(); return true; } catch { }
                }
            }
            value = 0; return false;
        }

        private static bool TryGet(JsonElement obj, out JsonElement value, params string[] keys)
        {
            foreach (var k in keys)
                if (obj.TryGetProperty(k, out value))
                    return true;
            value = default; return false;
        }
        private async Task EnsureWebViewAsync()
        {
            if (webView.CoreWebView2 == null)
            {
                var env = await CoreWebView2Environment.CreateAsync();
                await webView.EnsureCoreWebView2Async(env);
                webView.CoreWebView2.Settings.AreDevToolsEnabled = true;
                webView.CoreWebView2.Settings.IsZoomControlEnabled = true;
            }
        }
        // Tạo số hóa đơn dạng HD-YYYYMMDD-XXX (XXX là số thứ tự trong ngày)
        private async Task<string> GenerateNextApInvoiceCodeAsync(DateTime ngayHd)
        {
            var date = ngayHd.Date;
            var prefix = $"HD-{date:yyyyMMdd}-";

            // Lấy số HĐ cuối cùng của ngày đó (so sánh theo prefix)
            var lastSo = await _db.HoaDonMua
                .Where(h => h.NgayHoaDon.Date == date && h.SoCt.StartsWith(prefix))
                .OrderByDescending(h => h.SoCt)
                .Select(h => h.SoCt)
                .FirstOrDefaultAsync();

            var next = 1;
            if (!string.IsNullOrEmpty(lastSo))
            {
                // Bắt phần số ở cuối: HD-YYYYMMDD-<NNN>
                var m = System.Text.RegularExpressions.Regex.Match(lastSo, @"-(\d{3,})$");
                if (m.Success && int.TryParse(m.Groups[1].Value, out var seq))
                    next = seq + 1;
            }

            return $"{prefix}{next:D3}";
        }

        private async Task<string> GenerateNextMoCodeAsync(DateTime? ngayLenh, AccountingDbContext db)
        {
            var d = (ngayLenh ?? DateTime.Now).Date;
            var prefix = $"MO-{d:yyyyMMdd}-";

            // Lấy mã lớn nhất cùng ngày
            var last = await db.LenhSanXuat
                .AsNoTracking()
                .Where(x => x.Ma.StartsWith(prefix))
                .OrderByDescending(x => x.Ma)
                .Select(x => x.Ma)
                .FirstOrDefaultAsync();

            var next = 1;
            if (!string.IsNullOrEmpty(last))
            {
                var m = System.Text.RegularExpressions.Regex.Match(last, @"-(\d{3,})$");
                if (m.Success && int.TryParse(m.Groups[1].Value, out var seq)) next = seq + 1;
            }
            return $"{prefix}{next:D3}";
        }
        private void ReplyError(string message)
        {
            Reply(new { ok = false, error = message });
        }
        // ===================== Bridge JS ↔ C# =====================
        private async void CoreWebView2_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                var json = e.WebMessageAsJson;
                var msg = JsonSerializer.Deserialize<JsonElement>(json);
                var cmd = msg.GetProperty("cmd").GetString();

                switch (cmd)
                {
                    case "login":
                        {
                            string username = "";
                            string password = "";

                            if (msg.TryGetProperty("tenDangNhap", out var uEl) && uEl.ValueKind == JsonValueKind.String)
                                username = uEl.GetString() ?? "";
                            if (msg.TryGetProperty("matKhau", out var pEl) && pEl.ValueKind == JsonValueKind.String)
                                password = pEl.GetString() ?? "";

                            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                            {
                                ReplyError("Dữ liệu đăng nhập không hợp lệ.");
                                break;
                            }

                            var result = await _userService.LoginAsync(username, password);

                            if (!result.Success || result.User == null)
                            {
                                ReplyError(result.Error ?? "Đăng nhập thất bại.");
                                break;
                            }

                            _currentUser = result.User;

                            // Trả về user cho JS hiển thị góc phải
                            Reply(new
                            {
                                ok = true,
                                user = result.User
                            });

                            break;
                        }

                    case "logout":
                        {
                            _currentUser = null;
                            Reply(new { ok = true });
                            break;
                        }

                    // ===== USERS CRUD =====
                    case "user.list":
                        {
                            var users = await _userService.GetAllAsync();
                            Reply(new
                            {
                                ok = true,
                                items = users
                            });
                            break;
                        }

                    case "user.save":
                        {
                            // Dữ liệu đi thẳng trên msg: { cmd:"user.save", id, tenDangNhap, hoTen, vaiTro, dangHoatDong, matKhauMoi }
                            var dto = new UserEditDto();

                            if (msg.TryGetProperty("id", out var idEl) && idEl.ValueKind == JsonValueKind.Number)
                                dto.Id = idEl.GetInt32();

                            if (msg.TryGetProperty("tenDangNhap", out var userEl) && userEl.ValueKind == JsonValueKind.String)
                                dto.TenDangNhap = userEl.GetString() ?? "";

                            if (msg.TryGetProperty("hoTen", out var nameEl) && nameEl.ValueKind == JsonValueKind.String)
                                dto.HoTen = nameEl.GetString() ?? "";

                            if (msg.TryGetProperty("vaiTro", out var roleEl) && roleEl.ValueKind == JsonValueKind.String)
                                dto.VaiTro = roleEl.GetString() ?? "";

                            if (msg.TryGetProperty("dangHoatDong", out var activeEl) &&
                                activeEl.ValueKind == JsonValueKind.True ||
                                activeEl.ValueKind == JsonValueKind.False)
                            {
                                dto.DangHoatDong = activeEl.GetBoolean();
                            }
                            else
                            {
                                dto.DangHoatDong = true;
                            }

                            if (msg.TryGetProperty("matKhauMoi", out var passEl) && passEl.ValueKind == JsonValueKind.String)
                                dto.MatKhauMoi = passEl.GetString();

                            if (string.IsNullOrWhiteSpace(dto.TenDangNhap) || string.IsNullOrWhiteSpace(dto.HoTen))
                            {
                                ReplyError("Vui lòng nhập đầy đủ Tên đăng nhập và Họ tên.");
                                break;
                            }

                            // Nếu thêm mới mà không có mật khẩu → báo lỗi
                            if ((!dto.Id.HasValue || dto.Id.Value <= 0) &&
                                string.IsNullOrWhiteSpace(dto.MatKhauMoi))
                            {
                                ReplyError("Vui lòng nhập mật khẩu cho người dùng mới.");
                                break;
                            }

                            var saved = await _userService.SaveAsync(dto, _currentUser?.TenDangNhap);
                            Reply(new
                            {
                                ok = true,
                                user = saved
                            });
                            break;
                        }

                    case "user.delete":
                        {
                            if (!msg.TryGetProperty("id", out var idEl) || idEl.ValueKind != JsonValueKind.Number)
                            {
                                ReplyError("Id không hợp lệ.");
                                break;
                            }

                            var id = idEl.GetInt32();
                            if (id <= 0)
                            {
                                ReplyError("Id không hợp lệ.");
                                break;
                            }

                            await _userService.DeleteAsync(id);
                            Reply(new { ok = true });
                            break;
                        }
                    // ===== Danh sách đơn mua =====
                    case "list":
                    case "listDonMua":
                        {
                            var data = await _service.ListDonMuaAsync();
                            Reply(data);
                            break;
                        }
                    case "getCongNoKh":
                        {
                            bool chiConNo = msg.TryGetProperty("chiConNo", out var onlyEl) && onlyEl.GetBoolean();
                            var data = await _congNoService.GetCongNoKhachHangAsync(chiConNo);
                            Reply(data);    // hoặc webView.CoreWebView2.PostWebMessageAsString(JsonSerializer.Serialize(data));
                            break;
                        }
                    case "getNhacNoKh":
                        {
                            var today = DateTime.Today;
                            var data = await _congNoService.GetNhacNoKhachHangChiTietAsync(today);
                            Reply(data);
                            break;
                        }
                    // ===== BÁO CÁO: LỢI NHUẬN THEO KỲ =====
                    case "report.getProfit":
                        {
                            DateTime tuNgay = DateTime.Today.AddMonths(-11); // default 12 tháng gần nhất
                            DateTime denNgay = DateTime.Today;

                            if (msg.TryGetProperty("tuNgay", out var tnEl) && tnEl.ValueKind == JsonValueKind.String)
                                tuNgay = DateTime.Parse(tnEl.GetString()!);

                            if (msg.TryGetProperty("denNgay", out var dnEl) && dnEl.ValueKind == JsonValueKind.String)
                                denNgay = DateTime.Parse(dnEl.GetString()!);

                            var list = await _reportService.GetLoiNhuanTheoThangAsync(tuNgay, denNgay);
                            Reply(list);
                            break;
                        }

                    case "getDashboardSummary":
                        {
                            var summary = await _dashboardService.GetSummaryAsync();

                            var payload = new
                            {
                                cmd = "getDashboardSummary",
                                ok = true,
                                data = summary
                            };

                            var responseJson = JsonSerializer.Serialize(payload);
                            webView.CoreWebView2.PostWebMessageAsJson(responseJson);
                            break;
                        }

                    case "report.getArapSummary":
                        {
                            DateTime? tuNgay = null;
                            DateTime? denNgay = null;

                            if (msg.TryGetProperty("tuNgay", out var tnEl) && tnEl.ValueKind == JsonValueKind.String)
                                tuNgay = DateTime.Parse(tnEl.GetString()!);

                            if (msg.TryGetProperty("denNgay", out var dnEl) && dnEl.ValueKind == JsonValueKind.String)
                                denNgay = DateTime.Parse(dnEl.GetString()!);

                            var list = await _reportService.GetBaoCaoCongNoTongHopAsync(tuNgay, denNgay);
                            Reply(list);
                            break;
                        }


                    case "report.getInventory":
                        {
                            DateTime tuNgay = DateTime.Today.AddMonths(-1);
                            DateTime denNgay = DateTime.Today;
                            long? khoId = null;

                            if (msg.TryGetProperty("tuNgay", out var tnEl) && tnEl.ValueKind == JsonValueKind.String)
                                tuNgay = DateTime.Parse(tnEl.GetString()!);

                            if (msg.TryGetProperty("denNgay", out var dnEl) && dnEl.ValueKind == JsonValueKind.String)
                                denNgay = DateTime.Parse(dnEl.GetString()!);

                            if (msg.TryGetProperty("khoId", out var khoEl))
                            {
                                if (khoEl.ValueKind == JsonValueKind.Number && khoEl.TryGetInt64(out var id))
                                    khoId = id;
                                else if (khoEl.ValueKind == JsonValueKind.String && long.TryParse(khoEl.GetString(), out var id2))
                                    khoId = id2;
                            }

                            var list = await _reportService.GetBaoCaoTonKhoAsync(tuNgay, denNgay, khoId);
                            Reply(list);
                            break;
                        }


                    case "bank.listHoaDonBanNo":
                        {
                            var list = await _bankService.ListHoaDonBanConNoAsync();
                            // DTO đã đúng shape JS cần (id, soCt, ngay, doiTuong, ...)
                            Reply(list);
                            break;
                        }

                    case "bank.listHoaDonMuaNo":
                        {
                            var list = await _bankService.ListHoaDonMuaConNoAsync();
                            Reply(list);
                            break;
                        }


                    case "cash.listSoQuy":
                        {
                            DateTime? tuNgay = null, denNgay = null;

                            if (msg.TryGetProperty("tuNgay", out var tnEl) && tnEl.ValueKind == JsonValueKind.String)
                                tuNgay = DateTime.Parse(tnEl.GetString()!);

                            if (msg.TryGetProperty("denNgay", out var dnEl) && dnEl.ValueKind == JsonValueKind.String)
                                denNgay = DateTime.Parse(dnEl.GetString()!);

                            var list = await _cashService.GetSoQuyAsync(tuNgay, denNgay);
                            Reply(list);
                            break;
                        }

                    case "cash.listPhieuThu":
                        {
                            DateTime? tuNgay = null, denNgay = null;

                            if (msg.TryGetProperty("tuNgay", out var tnEl) && tnEl.ValueKind == JsonValueKind.String)
                                tuNgay = DateTime.Parse(tnEl.GetString()!);

                            if (msg.TryGetProperty("denNgay", out var dnEl) && dnEl.ValueKind == JsonValueKind.String)
                                denNgay = DateTime.Parse(dnEl.GetString()!);

                            var list = await _cashService.ListPhieuThuAsync(tuNgay, denNgay);
                            Reply(list);
                            break;
                        }

                    case "cash.listPhieuChi":
                        {
                            DateTime? tuNgay = null, denNgay = null;

                            if (msg.TryGetProperty("tuNgay", out var tnEl) && tnEl.ValueKind == JsonValueKind.String)
                                tuNgay = DateTime.Parse(tnEl.GetString()!);

                            if (msg.TryGetProperty("denNgay", out var dnEl) && dnEl.ValueKind == JsonValueKind.String)
                                denNgay = DateTime.Parse(dnEl.GetString()!);

                            var list = await _cashService.ListPhieuChiAsync(tuNgay, denNgay);
                            Reply(list);
                            break;
                        }

                    case "cash.savePhieuThu":
                        {
                            // JS gửi api("cash.savePhieuThu", dto)
                            // wrapper sẽ merge thành { cmd: "...", ...dto }
                            var dto = JsonSerializer.Deserialize<PhieuThuDto>(msg.GetRawText(), JsonInOptions)!;
                            var entity = await _cashService.SavePhieuThuAsync(dto, "admin");
                            Reply(new { entity.Id, entity.SoCt });
                            break;
                        }

                    case "cash.savePhieuChi":
                        {
                            var dto = JsonSerializer.Deserialize<PhieuChiDto>(msg.GetRawText(), JsonInOptions)!;
                            var entity = await _cashService.SavePhieuChiAsync(dto, "admin");
                            Reply(new { entity.Id, entity.SoCt });
                            break;
                        }

                    case "sales.listHoaDonBanChuaThu":
                        {
                            // Lấy danh sách hóa đơn bán còn công nợ (chưa thanh toán hết)
                            var list = await _db.HoaDonBan
                                .Where(h => !h.DaXoa && h.TrangThaiCongNo != "da_tt")
                                .Select(h => new
                                {
                                    id = h.Id,
                                    soHoaDon = h.SoHoaDon,
                                    // tạm thời chưa join tên KH, để chuỗi rỗng / sau này bổ sung sau
                                    tenKhachHang = "",
                                    tongTien = h.TongTien,
                                    daThanhToan = h.SoTienDaThanhToan,
                                    conNo = h.TongTien - h.SoTienDaThanhToan
                                })
                                .OrderBy(x => x.soHoaDon)
                                .ToListAsync();
                            Console.WriteLine($"HoaDonBan chưa thu: {list.Count}");
                            Reply(list);   // giống các case khác
                            break;
                        }



                    case "cash.getSoDuQuy":
                        {
                            DateTime? denNgay = null;
                            if (msg.TryGetProperty("denNgay", out var dnEl) && dnEl.ValueKind == JsonValueKind.String)
                                denNgay = DateTime.Parse(dnEl.GetString()!);

                            using var db = new AccountingDbContext(_dbOptions);
                            var cashService = new CashService(db);
                            var soDu = await cashService.GetSoDuQuyAsync(denNgay);

                            Reply(new { soDu });
                            break;
                        }

                    case "cash.saveKiemKeQuy":
                        {
                            using var db = new AccountingDbContext(_dbOptions);
                            var cashService = new CashService(db);

                            var dto = JsonSerializer.Deserialize<KiemKeQuyDto>(msg.GetRawText(), JsonInOptions)!;
                            var entity = await cashService.SaveKiemKeQuyAsync(dto, "admin");
                            Reply(new { entity.Id });
                            break;
                        }


                    case "purchases.listHoaDonMuaChuaTra":
                        {
                            // Lấy danh sách hóa đơn MUA còn công nợ (chưa thanh toán đủ)
                            var list = await _db.HoaDonMua
                                .Where(h => h.TrangThaiCongNo != "da_tt")  // bỏ !h.DaXoa nếu chưa có
                                .Select(h => new
                                {
                                    id = h.Id,
                                    soHoaDon = h.SoCt,                      // hoặc SoHoaDon tùy DB bạn
                                    tenNhaCungCap = "",                     // sau có thể join bảng NCC
                                    tongTien = h.TongTien,
                                    daThanhToan = h.SoTienDaThanhToan,
                                    conNo = h.TongTien - h.SoTienDaThanhToan
                                })
                                .OrderBy(x => x.soHoaDon)
                                .ToListAsync();

                            Reply(list);
                            break;
                        }

                    case "catalog.listBankAccounts":
                        {
                            var list = await _catalog.ListBankAccountsAsync();
                            // Trả về đúng cấu trúc mà JS đang dùng: id, ma, tenNganHang, soTaiKhoan, tienTe
                            var result = list.Select(x => new
                            {
                                id = x.Id,
                                ma = x.Ma,
                                tenNganHang = x.TenNganHang,
                                soTaiKhoan = x.SoTaiKhoan,
                                tienTe = x.TienTe
                            });
                            Reply(result);
                            break;
                        }


                    case "bank.listSoKe":
                        {
                            long taiKhoanId = msg.GetProperty("taiKhoanId").GetInt64();
                            DateTime? tuNgay = null, denNgay = null;

                            if (msg.TryGetProperty("tuNgay", out var tnEl) && tnEl.ValueKind == JsonValueKind.String)
                                tuNgay = DateTime.Parse(tnEl.GetString()!);

                            if (msg.TryGetProperty("denNgay", out var dnEl) && dnEl.ValueKind == JsonValueKind.String)
                                denNgay = DateTime.Parse(dnEl.GetString()!);

                            var list = await _bankService.GetSoNganHangAsync(taiKhoanId, tuNgay, denNgay);
                            Reply(list);
                            break;
                        }

                    case "bank.listPhieuThu":
                        {
                            long? taiKhoanId = null;
                            if (msg.TryGetProperty("taiKhoanId", out var tkEl) && tkEl.ValueKind == JsonValueKind.Number)
                                taiKhoanId = tkEl.GetInt64();

                            DateTime? tuNgay = null, denNgay = null;
                            if (msg.TryGetProperty("tuNgay", out var tnEl) && tnEl.ValueKind == JsonValueKind.String)
                                tuNgay = DateTime.Parse(tnEl.GetString()!);
                            if (msg.TryGetProperty("denNgay", out var dnEl) && dnEl.ValueKind == JsonValueKind.String)
                                denNgay = DateTime.Parse(dnEl.GetString()!);

                            var list = await _bankService.ListPhieuThuAsync(taiKhoanId, tuNgay, denNgay);
                            Reply(list);
                            break;
                        }

                    case "bank.listPhieuChi":
                        {
                            long? taiKhoanId = null;
                            if (msg.TryGetProperty("taiKhoanId", out var tkEl) && tkEl.ValueKind == JsonValueKind.Number)
                                taiKhoanId = tkEl.GetInt64();

                            DateTime? tuNgay = null, denNgay = null;
                            if (msg.TryGetProperty("tuNgay", out var tnEl) && tnEl.ValueKind == JsonValueKind.String)
                                tuNgay = DateTime.Parse(tnEl.GetString()!);
                            if (msg.TryGetProperty("denNgay", out var dnEl) && dnEl.ValueKind == JsonValueKind.String)
                                denNgay = DateTime.Parse(dnEl.GetString()!);

                            var list = await _bankService.ListPhieuChiAsync(taiKhoanId, tuNgay, denNgay);
                            Reply(list);
                            break;
                        }

                    case "bank.savePhieuThu":
                        {
                            var dto = JsonSerializer.Deserialize<PhieuThuNganHangDto>(msg.GetRawText(), JsonInOptions)!;
                            var entity = await _bankService.SavePhieuThuAsync(dto, "admin");
                            Reply(new { entity.Id, entity.SoCt });
                            break;
                        }

                    case "bank.savePhieuChi":
                        {
                            var dto = JsonSerializer.Deserialize<PhieuChiNganHangDto>(msg.GetRawText(), JsonInOptions)!;
                            var entity = await _bankService.SavePhieuChiAsync(dto, "admin");
                            Reply(new { entity.Id, entity.SoCt });
                            break;
                        }

                    case "bank.getSoDuTaiKhoan":
                        {
                            long taiKhoanId = msg.GetProperty("taiKhoanId").GetInt64();
                            DateTime? denNgay = null;
                            if (msg.TryGetProperty("denNgay", out var dnEl) && dnEl.ValueKind == JsonValueKind.String)
                                denNgay = DateTime.Parse(dnEl.GetString()!);

                            var soDu = await _bankService.GetSoDuTaiKhoanAsync(taiKhoanId, denNgay);
                            Reply(new { soDu });
                            break;
                        }


                    case "getKhachHangDetail":
                        {
                            long id = msg.GetProperty("id").GetInt64();
                            var info = await _congNoService.GetKhachHangDetailAsync(id);
                            Reply(info);
                            break;
                        }

                    case "getNccDetail":
                        {
                            long id = msg.GetProperty("id").GetInt64();
                            var info = await _congNoService.GetNhaCungCapDetailAsync(id);
                            Reply(info);
                            break;
                        }


                    case "getCongNoNcc":
                        {
                            bool chiConNo = msg.TryGetProperty("chiConNo", out var onlyEl) && onlyEl.GetBoolean();
                            var data = await _congNoService.GetCongNoNhaCungCapAsync(chiConNo);
                            Reply(data);
                            break;
                        }
                    case "getNhacNoNcc":
                        {
                            var today = DateTime.Today;
                            var data = await _congNoService.GetNhacNoNhaCungCapChiTietAsync(today);
                            Reply(data);
                            break;
                        }

                    // ===== Tạo đơn mua =====
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

                    // ===== Thêm dòng đơn mua =====
                    case "addDong":
                        {
                            var donMuaId = msg.GetProperty("donMuaId").GetInt32();
                            var dong = JsonSerializer.Deserialize<DonMuaDongDto>(
                                msg.GetProperty("dong").GetRawText(), JsonInOptions);

                            var updated = await _service.AddDongDonMuaAsync(donMuaId, dong!);
                            Reply(updated);
                            break;
                        }

                    // ===== Tạo phiếu nhập từ đơn =====
                    case "createPhieuNhap":
                        {
                            var pn = JsonSerializer.Deserialize<CreatePnDto>(
                                msg.GetProperty("phieuNhap").GetRawText(), JsonInOptions);
                            if (pn == null) { Reply(new { error = "payload_invalid" }); break; }

                            var result = await CreatePhieuNhapFromPoAsync(pn);
                            Reply(result); // { ok, pnId, status }
                            break;
                        }


                    // ===== Danh mục vật tư / thuế / tạo vật tư nhanh =====
                    case "getVatTu":
                        {
                            var list = await _db.VatTu
                                .Select(v => new { v.Id, v.Ma, v.Ten, v.DonViTinhId })
                                .ToListAsync();
                            var dvt = await _db.DonViTinh.Select(x => new { x.Id, x.Ten }).ToListAsync();
                            Reply(new { vatTu = list, dvt });
                            break;
                        }

                    case "getThueSuat":
                        {
                            var thue = await _db.ThueSuat
                                .OrderBy(x => x.TyLe)
                                .Select(x => new { x.Id, x.Ten, x.TyLe })
                                .ToListAsync();
                            Reply(thue);
                            break;
                        }

                    case "createVatTu":
                        {
                            var ma = msg.GetProperty("ma").GetString();
                            var ten = msg.TryGetProperty("ten", out var _ten) ? _ten.GetString() : ma;
                            var dvt = msg.TryGetProperty("dvtTen", out var _dvt) ? _dvt.GetString() : null;

                            var vt = await _catalog.CreateVatTuQuickAsync(ma!, ten, dvt);
                            Reply(new { ok = true, vt.Id, vt.Ma, vt.Ten, vt.DonViTinhId });
                            break;
                        }

                    // ======= Danh mục nhà cung cấp (mới thêm) =======
                    case "getNhaCungCap":
                        {
                            var list = await _db.NhaCungCap
                                .Where(x => !x.DaXoa)
                                .OrderBy(x => x.Ten)
                                .Select(x => new { x.Id, x.Ma, x.Ten })
                                .ToListAsync();
                            Reply(new { ncc = list });
                            break;
                        }


                    

                    case "createNhaCungCap":
                        {
                            // nhận payload { ncc: {...} } hoặc phẳng
                            CreateNccDto? dto = null;
                            if (msg.TryGetProperty("ncc", out var rawNcc))
                                dto = JsonSerializer.Deserialize<CreateNccDto>(rawNcc.GetRawText(), JsonInOptions);
                            else
                                dto = JsonSerializer.Deserialize<CreateNccDto>(msg.GetRawText(), JsonInOptions);

                            if (dto == null || string.IsNullOrWhiteSpace(dto.Ten))
                            {
                                Reply(new { error = "Thiếu tên nhà cung cấp." });
                                break;
                            }

                            var code = ToCode(dto.Ma, fallback: ToCode(dto.Ten, "NCC")).ToUpperInvariant();
                            code = await EnsureUniqueNccCodeAsync(code);

                            var ncc = new Accounting.Domain.Entities.NhaCungCap
                            {
                                Ma = code,
                                Ten = dto.Ten.Trim(),
                                MaSoThue = dto.MaSoThue,
                                DiaChi = dto.DiaChi,
                                SoDienThoai = dto.SoDienThoai,
                                Email = dto.Email,
                                NgayTao = DateTime.Now,
                                DaXoa = false
                            };

                            _db.NhaCungCap.Add(ncc);
                            await _db.SaveChangesAsync();

                            Reply(new { ok = true, id = ncc.Id, ma = ncc.Ma, ten = ncc.Ten });
                            break;
                        }

                    case "getDonMuaDetail":
                        {
                            var id = msg.GetProperty("id").GetInt64();

                            var dm = await _db.DonMua
                                .Where(x => x.Id == id)
                                .Select(x => new {
                                    x.Id,
                                    x.SoCt,
                                    x.NgayDon,
                                    x.TrangThai,
                                    x.GhiChu,
                                    NhaCungCapTen = _db.NhaCungCap
                                        .Where(n => n.Id == x.NhaCungCapId)
                                        .Select(n => n.Ten)
                                        .FirstOrDefault(),
                                    x.TienHang,
                                    x.TienThue,
                                    x.TongTien
                                })
                                .FirstOrDefaultAsync();

                            if (dm == null)
                            {
                                Reply(new { error = "not_found" });
                                break;
                            }

                            var lines = await _db.DonMuaDong
                                .Where(d => d.DonMuaId == id)
                                .Select(d => new {
                                    d.VatTuId,
                                    vatTuMa = _db.VatTu.Where(v => v.Id == d.VatTuId).Select(v => v.Ma).FirstOrDefault(),
                                    vatTuTen = _db.VatTu.Where(v => v.Id == d.VatTuId).Select(v => v.Ten).FirstOrDefault(),
                                    dvtTen = _db.DonViTinh.Where(u => u.Id == (
                                            _db.VatTu.Where(v => v.Id == d.VatTuId)
                                                     .Select(v => v.DonViTinhId).FirstOrDefault()
                                        )).Select(u => u.Ten).FirstOrDefault(),
                                    d.SoLuong,
                                    d.DonGia,
                                    d.ThueSuatId,
                                    thanhTien = d.ThanhTien
                                })
                                .ToListAsync();

                            // ✅ Thêm check hóa đơn tại đây
                            var hasInvoice = await _db.HoaDonMua.AnyAsync(h => h.DonMuaId == id);
                            var invoiceCount = await _db.HoaDonMua.CountAsync(h => h.DonMuaId == id);

                            // ✅ Trả data về FE
                            Reply(new
                            {
                                dm.Id,
                                dm.SoCt,
                                dm.NgayDon,
                                dm.TrangThai,
                                dm.GhiChu,
                                dm.NhaCungCapTen,
                                dm.TienHang,
                                dm.TienThue,
                                dm.TongTien,
                                hasInvoice,
                                invoiceCount,
                                lines
                            });
                            break;
                        }



                    // ===== Lấy NHÁP Phiếu nhập từ PO (prefill form PN) =====
                    case "getGrnDraftFromPO":
                        {
                            if (!TryGetInt64(msg, out var poId, "poId", "PoId"))
                            { Reply(new { error = "missing_poId" }); break; }
                            var draft = await GetGrnDraftFromPoAsync(poId);
                            Reply(new { draft });
                            break;
                        }
                    case "postGhiSoPO":
                        {
                            var poId = msg.GetProperty("id").GetInt64();
                            var status = await GhiSoDonMuaAsync(poId);
                            Reply(new { ok = true, status });
                            break;
                        }
                    case "listPnByPo":
                        {
                            if (!TryGetInt64(msg, out var poId, "poId", "PoId"))
                            { Reply(new { error = "missing_poId" }); break; }

                            var list = await _db.PhieuNhap
                                .Where(p => p.DonMuaId == poId)
                                .Select(p => new {
                                    p.Id,
                                    p.SoCt,
                                    p.NgayNhap,
                                    GiaTri = p.Dong.Sum(d => d.SoLuong * d.DonGia),
                                    SoDong = p.Dong.Count()
                                })
                                .OrderByDescending(x => x.NgayNhap)
                                .ToListAsync();

                            Reply(list);
                            break;
                        }
                    case "getGrnDetail":
                        {
                            var pnId = msg.GetProperty("pnId").GetInt64();

                            var header = await _db.PhieuNhap
                                .Where(p => p.Id == pnId)
                                .Select(p => new
                                {
                                    p.Id,
                                    p.SoCt,
                                    p.NgayNhap,
                                    p.DonMuaId,
                                    NccTen = _db.NhaCungCap.Where(n => n.Id == p.NhaCungCapId).Select(n => n.Ten).FirstOrDefault(),
                                    KhoTen = _db.Kho.Where(k => k.Id == p.KhoId).Select(k => k.Ten).FirstOrDefault(),
                                    p.GhiChu
                                })
                                .FirstOrDefaultAsync();

                            if (header == null) { Reply(new { error = "not_found" }); break; }

                            var lines = await _db.PhieuNhap
                                .Where(p => p.Id == pnId)
                                .SelectMany(p => p.Dong)
                                .Select(d => new
                                {
                                    VatTuMa = _db.VatTu.Where(v => v.Id == d.VatTuId).Select(v => v.Ma).FirstOrDefault(),
                                    VatTuTen = _db.VatTu.Where(v => v.Id == d.VatTuId).Select(v => v.Ten).FirstOrDefault(),
                                    DvtTen = _db.DonViTinh.Where(t => t.Id ==
                                                   _db.VatTu.Where(v => v.Id == d.VatTuId).Select(v => v.DonViTinhId).FirstOrDefault())
                                                   .Select(t => t.Ten).FirstOrDefault(),
                                    d.SoLuong,
                                    d.DonGia,
                                    GiaTri = (decimal?)(d.GiaTri) ?? d.SoLuong * d.DonGia
                                })
                                .ToListAsync();

                            var tong = lines.Sum(x => x.GiaTri);

                            Reply(new
                            {
                                header.Id,
                                header.SoCt,
                                header.NgayNhap,
                                header.DonMuaId,
                                header.NccTen,
                                header.KhoTen,
                                header.GhiChu,
                                TongGiaTri = tong,
                                Lines = lines
                            });
                            break;
                        }

                    // a) Danh sách HĐ theo PO
                    case "listHdByPo":
                        {
                            if (!TryGetInt64(msg, out var poId, "poId", "PoId"))
                            { Reply(new { error = "missing_poId" }); break; }

                            var list = await _db.HoaDonMua
                                .Where(h => h.DonMuaId == poId)
                                .Select(h => new {
                                    h.Id,
                                    soCt = h.SoCt,
                                    ngayHoaDon = h.NgayHoaDon,
                                    hanThanhToan = h.HanThanhToan,
                                    tongTien = h.TongTien ?? 0m,
                                    soDong = _db.HoaDonMuaDong.Count(d => d.HoaDonMuaId == h.Id),
                                    trangThai = h.TrangThai
                                })
                                .OrderByDescending(x => x.ngayHoaDon)
                                .ToListAsync();
                            Reply(list);
                            break;
                        }


                    // b) Chi tiết HĐ
                    case "getHdDetail":
                        {
                            long id;
                            if (msg.TryGetProperty("id", out var jId) && jId.ValueKind == JsonValueKind.Number)
                                id = jId.GetInt64();
                            else if (msg.TryGetProperty("hdId", out var jHd) && jHd.ValueKind == JsonValueKind.Number)
                                id = jHd.GetInt64();
                            else { Reply(new { error = "missing_id" }); break; }

                            var h = await _db.HoaDonMua
                                .Where(x => x.Id == id)
                                .Select(x => new {
                                    x.Id,
                                    soCt = x.SoCt,
                                    ngayHoaDon = x.NgayHoaDon,
                                    hanThanhToan = x.HanThanhToan,
                                    x.DonMuaId,
                                    x.NhaCungCapId,
                                    tongTien = x.TongTien ?? 0m,
                                    tienHang = x.TienHang ?? 0m,
                                    tienThue = x.TienThue ?? 0m,
                                    trangThai = x.TrangThai,
                                    nccTen = _db.NhaCungCap.Where(n => n.Id == x.NhaCungCapId).Select(n => n.Ten).FirstOrDefault()
                                })
                                .FirstOrDefaultAsync();
                            if (h == null) { Reply(new { error = "not_found" }); break; }

                            var lines = await _db.HoaDonMuaDong
                                .Where(d => d.HoaDonMuaId == id)
                                .Select(d => new {
                                    vatTuMa = _db.VatTu.Where(v => v.Id == d.VatTuId).Select(v => v.Ma).FirstOrDefault(),
                                    vatTuTen = _db.VatTu.Where(v => v.Id == d.VatTuId).Select(v => v.Ten).FirstOrDefault(),
                                    dvtTen = _db.DonViTinh.Where(t => t.Id == _db.VatTu.Where(v => v.Id == d.VatTuId)
                                                                      .Select(v => v.DonViTinhId).FirstOrDefault())
                                                           .Select(t => t.Ten).FirstOrDefault(),
                                    soLuong = d.SoLuong,
                                    donGia = d.DonGia,
                                    thueSuatId = d.ThueSuatId,
                                    tienThue = d.TienThue ?? 0m,
                                    thanhTien = d.ThanhTien ?? (d.SoLuong * d.DonGia) + (d.TienThue ?? 0m)
                                })
                                .ToListAsync();

                            Reply(new { header = h, lines });
                            break;
                        }


                    // c) Tạo HĐ từ PO
                    case "getHdDraftFromPO":
                        {
                            if (!TryGetInt64(msg, out var poId, "poId", "PoId"))
                            { Reply(new { error = "missing_poId" }); break; }

                            var draft = await GetHdDraftFromPoAsync(poId);
                            Reply(new { draft });
                            break;
                        }


                    case "createHoaDonMua":
                        {
                            var dto = JsonSerializer.Deserialize<HoaDonMuaCreateDto>(
                                msg.GetProperty("hoaDon").GetRawText(), JsonInOptions);
                            if (dto == null) { Reply(new { error = "payload_invalid" }); break; }

                            var res = await CreateHoaDonMuaAsync(dto);
                            Reply(res);
                            break;
                        }
                    // ===== KHO: Tổng hợp tồn kho =====
                    // ===== KHO: Tổng hợp tồn kho =====
                    // ====== KHO: cập nhật ngưỡng tồn (inline) ======
                    // ====== KHO: tổng hợp dữ liệu kho (chỉ Nhập; KHÔNG đụng NguongTon) ======
                    // ====== KHO: tổng hợp dữ liệu kho (tạm = tổng Nhập; không dùng NguongTon) ======
                    // ====== KHO: tổng hợp dữ liệu kho (tạm = tổng Nhập; không dùng NguongTon) ======
                    // ====== KHO: tổng hợp dữ liệu kho (tồn = tổng Nhập; đọc nguong_ton qua raw SQL) ======
                    // ====== KHO: tổng hợp dữ liệu kho (tồn = tổng Nhập; đọc nguong_ton raw SQL) ======
                    case "getInventorySummary":
 {
                            try
                            {
                                using var db = new AccountingDbContext(_dbOptions);

                                // đọc tham số từ message
                                string type = msg.TryGetProperty("type", out var pType) ? (pType.GetString() ?? "all") : "all";
                                string q = msg.TryGetProperty("q", out var pQ) ? (pQ.GetString() ?? "") : "";
                                var ql = (q ?? "").Trim().ToLowerInvariant();

                                bool hideZero = false;
                                if (msg.TryGetProperty("hideZero", out var pHz))
                                {
                                    if (pHz.ValueKind == JsonValueKind.True) hideZero = true;
                                    else if (pHz.ValueKind == JsonValueKind.False) hideZero = false;
                                    else if (pHz.ValueKind == JsonValueKind.String && bool.TryParse(pHz.GetString(), out var hz))
                                        hideZero = hz;
                                }

                                // 1) Load base list VatTu + DVT
                                var baseList = await (
                                    from vt in db.VatTu.AsNoTracking()
                                    join dvt in db.DonViTinh.AsNoTracking() on vt.DonViTinhId equals dvt.Id into gj
                                    from dvt in gj.DefaultIfEmpty()
                                    where !vt.DaXoa
                                    select new
                                    {
                                        vt.Id,
                                        vt.Ma,
                                        vt.Ten,
                                        DvtTen = dvt != null ? dvt.Ten : ""
                                    }
                                ).ToListAsync();

                                // 2) Đọc is_thanh_pham nếu cột tồn tại
                                var loaiMap = new Dictionary<long, bool>(); // true = thanh_pham
                                {
                                    var conn = db.Database.GetDbConnection();
                                    var needClose = false;
                                    if (conn.State != System.Data.ConnectionState.Open)
                                    {
                                        await conn.OpenAsync();
                                        needClose = true;
                                    }

                                    using (var chk = conn.CreateCommand())
                                    {
                                        chk.CommandText = "SELECT CASE WHEN COL_LENGTH('acc.vat_tu','is_thanh_pham') IS NULL THEN 0 ELSE 1 END";
                                        var hasCol = Convert.ToInt32(await chk.ExecuteScalarAsync()) == 1;
                                        if (hasCol)
                                        {
                                            using var sqlCmd = conn.CreateCommand();
                                            sqlCmd.CommandText = "SELECT id, CAST(is_thanh_pham AS INT) FROM acc.vat_tu";
                                            using var r = await sqlCmd.ExecuteReaderAsync();
                                            while (await r.ReadAsync())
                                            {
                                                var id = Convert.ToInt64(r.GetValue(0));
                                                var flag = r.IsDBNull(1) ? 0 : Convert.ToInt32(r.GetValue(1));
                                                loaiMap[id] = (flag == 1);
                                            }
                                        }
                                    }

                                    if (needClose)
                                        await conn.CloseAsync();
                                }

                                // áp bộ lọc q + type
                                var master = baseList
                                    .Where(x =>
                                        string.IsNullOrEmpty(ql) ||
                                        (x.Ma ?? "").ToLowerInvariant().Contains(ql) ||
                                        (x.Ten ?? "").ToLowerInvariant().Contains(ql))
                                    .Select(x => new
                                    {
                                        x.Id,
                                        x.Ma,
                                        x.Ten,
                                        x.DvtTen,
                                        Loai = loaiMap.TryGetValue(x.Id, out var tp) && tp ? "thanh_pham" : "vat_tu"
                                    })
                                    .ToList();

                                if (type == "vat_tu")
                                    master = master.Where(x => x.Loai == "vat_tu").ToList();
                                else if (type == "thanh_pham")
                                    master = master.Where(x => x.Loai == "thanh_pham").ToList();

                                // 3) Lấy ngưỡng tồn từ DB (acc.vat_tu.nguong_ton nếu có)
                                var minDict = new Dictionary<long, decimal>();
                                {
                                    var conn = db.Database.GetDbConnection();
                                    var needClose = false;
                                    if (conn.State != System.Data.ConnectionState.Open)
                                    {
                                        await conn.OpenAsync();
                                        needClose = true;
                                    }

                                    using var sqlCmd = conn.CreateCommand();
                                    sqlCmd.CommandText = @"
IF OBJECT_ID('acc.vat_tu','U') IS NOT NULL
    SELECT id, nguong_ton
    FROM acc.vat_tu;";
                                    using var rdr = await sqlCmd.ExecuteReaderAsync();
                                    while (await rdr.ReadAsync())
                                    {
                                        var id = Convert.ToInt64(rdr.GetValue(0));
                                        var val = rdr.IsDBNull(1)
                                            ? 0m
                                            : Convert.ToDecimal(rdr.GetValue(1), CultureInfo.InvariantCulture);
                                        minDict[id] = val;
                                    }

                                    if (needClose)
                                        await conn.CloseAsync();
                                }

                                // 4) Tính tồn (SL + GT) từ phiếu nhập / phiếu xuất
                                var nhapDict = await db.PhieuNhapDong
                                    .AsNoTracking()
                                    .GroupBy(d => d.VatTuId)
                                    .Select(g => new
                                    {
                                        VatTuId = g.Key,
                                        SlNhap = g.Sum(x => x.SoLuong),
                                        GtNhap = g.Sum(x => x.GiaTri)
                                    })
                                    .ToDictionaryAsync(x => x.VatTuId);

                                var xuatDict = await db.PhieuXuatDong
                                    .AsNoTracking()
                                    .GroupBy(d => d.VatTuId)
                                    .Select(g => new
                                    {
                                        VatTuId = g.Key,
                                        SlXuat = g.Sum(x => x.SoLuong),
                                        GtXuat = g.Sum(x => x.GiaTri)
                                    })
                                    .ToDictionaryAsync(x => x.VatTuId);

                                // 5) Build items trả ra UI
                                var items = master
                                    .OrderBy(x => x.Ten)
                                    .Select(m =>
                                    {
                                        nhapDict.TryGetValue(m.Id, out var n);
                                        xuatDict.TryGetValue(m.Id, out var x);

                                        var slNhap = n?.SlNhap ?? 0m;
                                        var slXuat = x?.SlXuat ?? 0m;
                                        var gtNhap = n?.GtNhap ?? 0m;
                                        var gtXuat = x?.GtXuat ?? 0m;

                                        var ton = slNhap - slXuat;
                                        var giaTriTon = gtNhap - gtXuat;

                                        var isTp = (m.Loai == "thanh_pham");

                                        // Nếu là thành phẩm mà tồn <= 0 & GT âm, ép về 0 và lấy trị tuyệt đối GT
                                        if (isTp && ton <= 0 && giaTriTon < 0)
                                        {
                                            ton = 0;
                                            giaTriTon = Math.Abs(giaTriTon);
                                        }

                                        minDict.TryGetValue(m.Id, out var minTon);

                                        return new
                                        {
                                            id = m.Id,
                                            ma = m.Ma,
                                            ten = m.Ten,
                                            dvtTen = m.DvtTen,
                                            ton,
                                            minTon,
                                            giaTriTon,
                                            loai = m.Loai,
                                            isThanhPham = isTp
                                        };
                                    })
                                    .Where(it => !hideZero || it.ton != 0 || it.giaTriTon != 0)  // backend filter hideZero
                                    .ToList();

                                Reply(new { items });
                            }
                            catch (Exception ex)
                            {
                                Reply(new { items = Array.Empty<object>(), error = "exception", message = ex.Message });
                            }
                            break;
                        }




                    case "createPhieuXuatFromSo":
                        {
                            await using var db = new AccountingDbContext(_dbOptions);
                            await using var tx = await db.Database.BeginTransactionAsync();
                            try
                            {
                                long soId = msg.GetProperty("soId").GetInt64();

                                var so = await db.DonBan.FirstOrDefaultAsync(x => x.Id == soId && !x.DaXoa);
                                if (so == null) { Reply(new { ok = false, error = "Không tìm thấy đơn bán." }); break; }

                                var linesSo = await db.DonBanDong
                                    .Where(d => d.DonBanId == soId && !d.DaXoa)
                                    .ToListAsync();

                                var px = new PhieuXuat
                                {
                                    SoCt = await GenerateNextPxCodeAsync(db),
                                    NgayXuat = DateTime.Now,
                                    GhiChu = $"Xuất theo đơn bán {so.SoCt}",
                                    DonBanId = so.Id,
                                    NgayTao = DateTime.Now,
                                    NguoiTao = "system"
                                };
                                db.PhieuXuat.Add(px);
                                await db.SaveChangesAsync(); // có px.Id
                                foreach (var d in linesSo)
                                {
                                    long? vtId = await db.VatTu
                                        .Where(v => v.Ten == d.TenHang)
                                        .Select(v => (long?)v.Id)
                                        .FirstOrDefaultAsync();

                                    db.PhieuXuatDong.Add(new PhieuXuatDong
                                    {
                                        PhieuXuatId = px.Id,
                                        VatTuId = vtId, // có thể null
                                        SoLuong = d.SoLuong,
                                        DonGia = d.DonGia,
                                        GiaTri = d.SoLuong * d.DonGia,
                                        SoLo = d.QuyCach
                                    });
                                }
                                await db.SaveChangesAsync();
                                so.TrangThai = "shipped";
                                so.NgayCapNhat = DateTime.Now;
                                await db.SaveChangesAsync();
                                await tx.CommitAsync();
                                Reply(new { ok = true, pxId = px.Id, pxCode = px.SoCt });
                            }
                            catch (DbUpdateException ex)
                            {
                                try { await tx.RollbackAsync(); } catch { }

                                // Lấy message sâu nhất
                                string errMsg = ex.InnerException?.Message
                                                ?? ex.GetBaseException().Message
                                                ?? ex.Message;

                                // Nếu dùng SqlClient, có thể lấy mã lỗi
                                if (ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx)
                                    errMsg = $"SQL {sqlEx.Number}: {sqlEx.Message}";

                                Reply(new { ok = false, error = "DB: " + errMsg });
                            }
                            catch (Exception ex)
                            {
                                try { await tx.RollbackAsync(); } catch { }
                                Reply(new { ok = false, error = ex.GetBaseException().Message });
                            }
                            break;
                        }


                    case "getSalesLinkedDocs":
                        {
                            try
                            {
                                long soId = msg.GetProperty("soId").GetInt64();
                                using var db = new AccountingDbContext(_dbOptions);

                                // Lấy header đơn bán (để biết SoCt dùng fallback tìm PX)
                                var so = await db.DonBan.AsNoTracking()
                                    .Where(x => x.Id == soId && !x.DaXoa)
                                    .Select(x => new { x.Id, x.SoCt })
                                    .FirstOrDefaultAsync();
                                if (so == null) { Reply(new { error = "not_found" }); break; }

                                // ===== Hóa đơn bán (đã có DonBanId trong schema bạn) =====
                                var invoices = await db.HoaDonBan.AsNoTracking()
                                    .Where(h => h.DonBanId == so.Id && !h.DaXoa)
                                    .OrderByDescending(h => h.NgayHoaDon)
                                    .Select(h => new {
                                        id = h.Id,
                                        so = h.SoHoaDon,
                                        ngay = h.NgayHoaDon,
                                        tong = h.TongTien,
                                        trangThai = h.TrangThai
                                    })
                                    .ToListAsync();

                                // ===== Phiếu xuất =====
                                // Ưu tiên tìm theo DonBanId nếu bạn đã thêm cột (khuyến nghị).
                                // Nếu chưa có cột này, fallback tìm theo GhiChu chứa số chứng từ đơn bán.
                                var pxQuery = db.PhieuXuat.AsNoTracking().AsQueryable();

                                bool hasDonBanIdColumn = true; // đặt cờ dùng truy vấn DonBanId; nếu bạn CHƯA thêm cột, set = false

                                List<object> pxs;
                                if (hasDonBanIdColumn)
                                {
                                    pxs = await pxQuery
                                        .Where(p => p.DonBanId == so.Id)   // dùng cột liên kết
                                        .OrderByDescending(p => p.NgayXuat)
                                        .Select(p => new {
                                            id = p.Id,
                                            so = p.SoCt,
                                            ngay = p.NgayXuat,
                                            ghiChu = p.GhiChu
                                        })
                                        .Cast<object>()
                                        .ToListAsync();
                                }
                                else
                                {
                                    // Fallback nếu chưa thêm cột DonBanId
                                    string needle = so.SoCt ?? "";
                                    pxs = await pxQuery
                                        .Where(p => p.GhiChu != null && p.GhiChu.Contains(needle))
                                        .OrderByDescending(p => p.NgayXuat)
                                        .Select(p => new {
                                            id = p.Id,
                                            so = p.SoCt,
                                            ngay = p.NgayXuat,
                                            ghiChu = p.GhiChu
                                        })
                                        .Cast<object>()
                                        .ToListAsync();
                                }

                                Reply(new { invoices, pxs });
                            }
                            catch (Exception ex)
                            {
                                Reply(new { error = "exception", message = ex.GetBaseException().Message });
                            }
                            break;
                        }

                    // ========== CHI TIẾT HÓA ĐƠN BÁN ==========
                    case "getHdBanDetail":
                        {
                            long id = msg.GetProperty("id").GetInt64();
                            using var db = new AccountingDbContext(_dbOptions);

                            var header = await db.HoaDonBan
                                .AsNoTracking()
                                .Where(x => x.Id == id && !x.DaXoa)
                                .Select(x => new {
                                    x.Id,
                                    soHoaDon = x.SoHoaDon,
                                    ngayHoaDon = x.NgayHoaDon,
                                    x.TrangThai,
                                    x.TienHang,
                                    x.TienThue,
                                    x.TongTien,
                                    donBanId = x.DonBanId
                                })
                                .FirstOrDefaultAsync();

                            if (header == null) { Reply(new { error = "not_found" }); break; }

                            // (tuỳ chọn) lấy số CT đơn bán để hiển thị ở UI
                            string? soCt = null;
                            if (header.donBanId > 0)
                                soCt = await db.DonBan.Where(s => s.Id == header.donBanId).Select(s => s.SoCt).FirstOrDefaultAsync();

                            var lines = await db.Set<HoaDonBanDong>()
                                .AsNoTracking()
                                .Where(d => d.HoaDonBanId == id && !d.DaXoa)
                                .Select(d => new {
                                    d.TenHang,
                                    d.QuyCach,
                                    d.DonViTinh,
                                    d.SoLuong,
                                    d.DonGia,
                                    d.TienHang,
                                    d.TienThue,
                                    d.ThanhTien,
                                    d.ThueSuat
                                })
                                .ToListAsync();

                            Reply(new { header = new { header.Id, header.soHoaDon, header.ngayHoaDon, header.TrangThai, header.TienHang, header.TienThue, header.TongTien, header.donBanId, donBanSo = soCt }, lines });
                            break;
                        }

                    // ========== CHI TIẾT PHIẾU XUẤT ==========
                    case "getPxDetail":
                        {
                            long id = msg.GetProperty("id").GetInt64();
                            using var db = new AccountingDbContext(_dbOptions);

                            var header = await db.PhieuXuat
                                .AsNoTracking()
                                .Where(x => x.Id == id)
                                .Select(x => new {
                                    x.Id,
                                    x.SoCt,
                                    x.NgayXuat,
                                    x.GhiChu,
                                    donBanId = x.DonBanId
                                })
                                .FirstOrDefaultAsync();

                            if (header == null) { Reply(new { error = "not_found" }); break; }

                            string? soCt = null;
                            if (header.donBanId != null)
                                soCt = await db.DonBan.Where(s => s.Id == header.donBanId).Select(s => s.SoCt).FirstOrDefaultAsync();

                            var lines = await db.PhieuXuatDong
                                .AsNoTracking()
                                .Where(d => d.PhieuXuatId == id)
                                .Join(db.VatTu, d => d.VatTuId, v => v.Id, (d, v) => new { d, v })
                                .Join(db.DonViTinh, dv => dv.v.DonViTinhId, u => u.Id, (dv, u) => new {
                                    vatTuMa = dv.v.Ma,
                                    vatTuTen = dv.v.Ten,
                                    dvtTen = u.Ten,
                                    soLuong = dv.d.SoLuong,
                                    donGia = dv.d.DonGia,
                                    giaTri = dv.d.GiaTri
                                })
                                .ToListAsync();

                            Reply(new { header = new { header.Id, soCt = header.SoCt, ngayXuat = header.NgayXuat, header.GhiChu, header.donBanId, donBanSo = soCt }, lines });
                            break;
                        }


                    case "createInvoiceFromSo":
                        {
                            await using var db = new AccountingDbContext(_dbOptions);
                            await using var tx = await db.Database.BeginTransactionAsync();
                            try
                            {
                                long soId = msg.GetProperty("soId").GetInt64();

                                var so = await db.DonBan.FirstOrDefaultAsync(x => x.Id == soId && !x.DaXoa);
                                if (so == null) { Reply(new { ok = false, error = "Không tìm thấy đơn bán." }); break; }

                                var linesSo = await db.DonBanDong
                                    .Where(d => d.DonBanId == soId && !d.DaXoa)
                                    .ToListAsync();

                                var hd = new HoaDonBan
                                {
                                    SoHoaDon = await GenerateNextHdCodeAsync(db),
                                    NgayHoaDon = DateTime.Now,
                                    DonBanId = so.Id,
                                    TrangThai = "issued",
                                    NgayTao = DateTime.Now,
                                    NguoiTao = "system",
                                    DaXoa = false
                                };
                                db.HoaDonBan.Add(hd);
                                await db.SaveChangesAsync(); // có hd.Id


                                decimal tienHang = 0, tienThue = 0;

                                foreach (var d in linesSo)
                                {
                                    var lineTienHang = d.SoLuong * d.DonGia;
                                    var lineTienThue = lineTienHang * (d.ThueSuat / 100m);
                                    var lineThanhTien = lineTienHang + lineTienThue;

                                    db.HoaDonBanDong.Add(new HoaDonBanDong
                                    {
                                        HoaDonBanId = hd.Id,
                                        TenHang = d.TenHang,
                                        QuyCach = d.QuyCach,
                                        DonViTinh = null,     // nếu PO line có đvt thì gán vào đây
                                        SoLuong = d.SoLuong,
                                        DonGia = d.DonGia,
                                        ThueSuat = d.ThueSuat,
                                        TienHang = lineTienHang,
                                        TienThue = lineTienThue,
                                        ThanhTien = lineThanhTien,
                                        DaXoa = false
                                    });

                                    tienHang += lineTienHang;
                                    tienThue += lineTienThue;
                                }

                                hd.TienHang = tienHang;
                                hd.TienThue = tienThue;
                                hd.TongTien = tienHang + tienThue;
                                hd.NgayCapNhat = DateTime.Now;

                                await db.SaveChangesAsync();
                                so.TrangThai = "invoiced";
                                so.NgayCapNhat = DateTime.Now;
                                await db.SaveChangesAsync();
                                await tx.CommitAsync();

                                Reply(new { ok = true, hdId = hd.Id, hdCode = hd.SoHoaDon });
                            }
                            catch (DbUpdateException ex)
                            {
                                try { await tx.RollbackAsync(); } catch { }

                                // Lấy message sâu nhất
                                string errMsg = ex.InnerException?.Message
                                                ?? ex.GetBaseException().Message
                                                ?? ex.Message;

                                // Nếu dùng SqlClient, có thể lấy mã lỗi
                                if (ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx)
                                    errMsg = $"SQL {sqlEx.Number}: {sqlEx.Message}";

                                Reply(new { ok = false, error = "DB: " + errMsg });
                            }
                            catch (Exception ex)
                            {
                                try { await tx.RollbackAsync(); } catch { }
                                Reply(new { ok = false, error = ex.GetBaseException().Message });
                            }
                            break;
                        }




                    // ====== KHO: cập nhật ngưỡng tồn (update acc.vat_tu.nguong_ton) ======
                    case "updateInventoryMin":
                        {
                            try
                            {
                                string idStr = msg.TryGetProperty("id", out var pId) ? pId.GetString() ?? "" : "";
                                if (string.IsNullOrWhiteSpace(idStr)) { Reply(new { error = "missing_id" }); break; }

                                if (!msg.TryGetProperty("minTon", out var pMin)) { Reply(new { error = "missing_minTon" }); break; }
                                decimal minTon;
                                try
                                {
                                    minTon = pMin.ValueKind == JsonValueKind.String
                                        ? decimal.Parse(pMin.GetString()!, System.Globalization.CultureInfo.InvariantCulture)
                                        : pMin.GetDecimal();
                                }
                                catch { Reply(new { error = "invalid_minTon" }); break; }
                                if (minTon < 0) { Reply(new { error = "invalid_minTon" }); break; }

                                // Lấy Id số (nếu client gửi Mã)
                                long vtId;
                                if (!long.TryParse(idStr, out vtId))
                                {
                                    var vtByMa = await _db.VatTu.AsNoTracking().FirstOrDefaultAsync(x => x.Ma == idStr);
                                    if (vtByMa == null) { Reply(new { error = "not_found" }); break; }
                                    vtId = vtByMa.Id;
                                }

                                // cập nhật trực tiếp cột acc.vat_tu.nguong_ton
                                var rows = await _db.Database.ExecuteSqlInterpolatedAsync(
                                    $"UPDATE acc.vat_tu SET nguong_ton = {minTon} WHERE id = {vtId}");

                                Reply(new { ok = rows >= 0, id = vtId, minTon });
                            }
                            catch (Exception ex)
                            {
                                Reply(new { error = "exception", message = ex.Message });
                            }
                            break;
                        }



                    // ===== MO: Danh sách (EF, fix type IIncludable/IQueryable) =====
                    case "getMoList":
                        {
                            try
                            {
                                using var db = new AccountingDbContext(_dbOptions);
                                string q = msg.TryGetProperty("q", out var pQ) ? (pQ.GetString() ?? "") : "";
                                string status = msg.TryGetProperty("status", out var pS) ? (pS.GetString() ?? "all") : "all";
                                q = q?.Trim() ?? "";

                                IQueryable<LenhSanXuat> query = db.LenhSanXuat.AsNoTracking().Where(m => !m.DaXoa);
                                if (!string.IsNullOrWhiteSpace(q))
                                    query = query.Where(m =>
                                        m.Ma.Contains(q) ||
                                        (m.TenKhachHang ?? "").Contains(q) ||
                                        (m.TenBaiIn ?? "").Contains(q) ||
                                        (m.MayIn ?? "").Contains(q));

                                if (status != "all") query = query.Where(m => m.TrangThai == status);

                                var items = await query
                                    .Include(m => m.SanPham)
                                    .OrderByDescending(m => m.NgayTao).ThenByDescending(m => m.Id)
                                    .Select(m => new {
                                        id = m.Id,
                                        ma = m.Ma,
                                        ngayLenh = m.NgayLenh.HasValue ? m.NgayLenh.Value.ToString("yyyy-MM-dd") : null,
                                        ngayIn = m.NgayIn.HasValue ? m.NgayIn.Value.ToString("yyyy-MM-dd") : null,
                                        sanPhamId = m.SanPhamId,
                                        sanPhamMa = m.SanPham != null ? m.SanPham.Ma : null,
                                        sanPhamTen = m.SanPham != null ? m.SanPham.Ten : null,
                                        soLuongKeHoach = m.SoLuongKeHoach,
                                        soLuongThucTe = m.SoLuongThucTe ?? 0m,
                                        // in ấn
                                        tenKhachHang = m.TenKhachHang,
                                        tenBaiIn = m.TenBaiIn,
                                        tenGiayIn = m.TenGiayIn,
                                        khoIn = m.KhoIn,
                                        soMauIn = m.SoMauIn ?? 0,
                                        hinhThucIn = m.HinhThucIn,
                                        soCon = m.SoCon ?? 0,
                                        mayIn = m.MayIn,
                                        soLuongThanhPham = m.SoLuongThanhPham ?? 0m,
                                        // meta
                                        trangThai = string.IsNullOrEmpty(m.TrangThai) ? "pending" : m.TrangThai
                                    })
                                    .Take(200)
                                    .ToListAsync();

                                Reply(new { items });
                            }
                            catch (Exception ex)
                            {
                                Reply(new { error = "exception", message = ex.Message });
                            }
                            break;
                        }


                    // using Microsoft.EntityFrameworkCore;  // đảm bảo có using này ở đầu file

                    case "getMoPresetFromSo":
                        {
                            try
                            {
                                // Lấy tham số soId từ body
                                long soId = 0;
                                if (msg.TryGetProperty("soId", out var soEl))
                                {
                                    soId = soEl.ValueKind == JsonValueKind.Number
                                        ? soEl.GetInt64()
                                        : long.TryParse(soEl.GetString(), out var tmp) ? tmp : 0;
                                }

                                using var db = new AccountingDbContext(_dbOptions);

                                // Lấy header + KH
                                var so = await db.DonBan.AsNoTracking()
                                    .FirstOrDefaultAsync(x => x.Id == soId && !x.DaXoa);

                                if (so == null) { Reply(new { error = "Không tìm thấy đơn bán." }); break; }

                                var khTen = await db.KhachHang.AsNoTracking()
                                    .Where(k => k.Id == so.KhachHangId)
                                    .Select(k => k.Ten)
                                    .FirstOrDefaultAsync();

                                // Lấy dòng đơn bán
                                var lines = await db.DonBanDong.AsNoTracking()
                                    .Where(d => d.DonBanId == so.Id && !d.DaXoa)
                                    .OrderBy(d => d.Id)
                                    .ToListAsync();

                                // Suy luận một số trường preset cho MO
                                var first = lines.FirstOrDefault();
                                var soLuongKH = lines.Sum(l => l.SoLuong); // có thể lấy tổng hoặc lấy của dòng đầu tùy nghiệp vụ

                                var header = new
                                {
                                    soId = so.Id,
                                    soCt = so.SoCt,
                                    khachHangId = so.KhachHangId,
                                    khachHangTen = khTen ?? "",

                                    // các trường phục vụ UI MO
                                    ngayDon = so.NgayDon,
                                    ngayLenh = DateTime.Today,
                                    ngayIn = (DateTime?)null,

                                    // gợi ý từ dòng đầu (nếu có)
                                    tenBaiIn = first?.TenHang ?? "",
                                    tenGiayIn = "",
                                    khoIn = "",
                                    soMauIn = 4,
                                    hinhThucIn = "",
                                    soCon = 0,
                                    mayIn = "",

                                    soLuongKeHoach = soLuongKH,   // số lượng kế hoạch mặc định
                                    note = so.GhiChu,

                                    // gợi ý Thành phẩm mới
                                    tpMa = "",
                                    tpTen = first?.TenHang ?? "",
                                    tpDvtId = (long?)null
                                };

                                // Nếu bạn đã có BOM/định mức ở đâu đó thì map vào đây; mặc định để trống
                                var dinhMuc = Array.Empty<object>();

                                Reply(new { ok = true, header, dinhMuc });
                            }
                            catch (Exception ex)
                            {
                                Reply(new { ok = false, error = ex.Message });
                            }
                            break;
                        }



                    // ===== MO: Tạo lệnh (EF)
                    case "createMo":
                        {
                            try
                            {
                                using var db = new AccountingDbContext(_dbOptions);

                                // ===== đọc payload chung
                                DateTime? ngayLenh = null;
                                if (msg.TryGetProperty("ngayLenh", out var pNgayLenh))
                                    if (DateTime.TryParse(pNgayLenh.GetString(), out var d1)) ngayLenh = d1;

                                decimal soLuongKeHoach = msg.GetProperty("soLuongKeHoach").GetDecimal();
                                string note = msg.TryGetProperty("note", out var pNote) ? (pNote.GetString() ?? "") : "";

                                // ngành in
                                string? tenKhachHang = msg.TryGetProperty("tenKhachHang", out var p1) ? p1.GetString() : null;
                                string? tenBaiIn = msg.TryGetProperty("tenBaiIn", out var p2) ? p2.GetString() : null;
                                string? tenGiayIn = msg.TryGetProperty("tenGiayIn", out var p3) ? p3.GetString() : null;
                                string? khoIn = msg.TryGetProperty("khoIn", out var p4) ? p4.GetString() : null;
                                int? soMauIn = msg.TryGetProperty("soMauIn", out var p5) && p5.ValueKind != JsonValueKind.Null ? p5.GetInt32() : (int?)null;
                                string? hinhThucIn = msg.TryGetProperty("hinhThucIn", out var p6) ? p6.GetString() : null;
                                int? soCon = msg.TryGetProperty("soCon", out var p7) && p7.ValueKind != JsonValueKind.Null ? p7.GetInt32() : (int?)null;
                                string? mayIn = msg.TryGetProperty("mayIn", out var p8) ? p8.GetString() : null;
                                DateTime? ngayIn = null;
                                if (msg.TryGetProperty("ngayIn", out var p9))
                                    if (!string.IsNullOrWhiteSpace(p9.GetString()) && DateTime.TryParse(p9.GetString(), out var d2)) ngayIn = d2;

                                // ===== đọc tpNew
                                if (!msg.TryGetProperty("tpNew", out var pTp) || pTp.ValueKind != JsonValueKind.Object)
                                {
                                    Reply(new { ok = false, error = "invalid_tpNew", message = "Thiếu tpNew" }); break;
                                }
                                string? tpMa = pTp.TryGetProperty("ma", out var pMa) ? pMa.GetString() : null;
                                string? tpTen = pTp.TryGetProperty("ten", out var pTen) ? pTen.GetString() : null;
                                long tpDvt = pTp.TryGetProperty("dvtId", out var pDvt) ? pDvt.GetInt64() : 0;

                                if (string.IsNullOrWhiteSpace(tpTen) || tpDvt <= 0)
                                {
                                    Reply(new { ok = false, error = "invalid_tp", message = "Tên TP và ĐVT là bắt buộc" }); break;
                                }
                                if (soLuongKeHoach <= 0) { Reply(new { ok = false, error = "invalid_qty" }); break; }

                                using var tx = await db.Database.BeginTransactionAsync();
                                try
                                {
                                    // 1) Tạo VatTu mới (thành phẩm)
                                    if (string.IsNullOrWhiteSpace(tpMa))
                                        tpMa = await GenerateNextProductCodeAsync(db);

                                    var vt = new VatTu
                                    {
                                        Ma = tpMa!,
                                        Ten = tpTen!,
                                        DonViTinhId = (int)tpDvt,         // nếu là bigint, đổi sang long phù hợp entity bạn
                                        DaXoa = false,
                                        NguongTon = 0m,

                                        IsThanhPham = true
                                        // nếu entity có cột NguongTon map: NguongTon = 0m
                                    };
                                    db.VatTu.Add(vt);
                                    await db.SaveChangesAsync();          // có Id

                                    // 2) Tạo mã lệnh SX
                                    var maMo = await GenerateNextMoCodeAsync(ngayLenh, db);

                                    // 3) Lưu header LenhSanXuat
                                    var mo = new LenhSanXuat
                                    {
                                        Ma = maMo,
                                        NgayLenh = ngayLenh,
                                        SanPhamId = vt.Id,               // dùng TP mới
                                        SoLuongKeHoach = soLuongKeHoach,
                                        SoLuongThucTe = 0m,

                                        TenKhachHang = tenKhachHang,
                                        TenBaiIn = tenBaiIn,
                                        TenGiayIn = tenGiayIn,
                                        KhoIn = khoIn,
                                        SoMauIn = soMauIn,
                                        HinhThucIn = hinhThucIn,
                                        SoCon = soCon,
                                        MayIn = mayIn,
                                        NgayIn = ngayIn,
                                        SoLuongThanhPham = 0m,

                                        TrangThai = "pending",
                                        GhiChu = note,
                                        NgayTao = DateTime.Now,
                                        DaXoa = false
                                    };
                                    db.LenhSanXuat.Add(mo);
                                    await db.SaveChangesAsync();

                                    // 4) Định mức (nếu có)
                                    var lines = new List<LenhSanXuatDong>();
                                    if (msg.TryGetProperty("dinhMuc", out var pDm) && pDm.ValueKind == JsonValueKind.Array)
                                    {
                                        foreach (var el in pDm.EnumerateArray())
                                        {
                                            try
                                            {
                                                var vtId = el.GetProperty("vatTuId").GetInt64();
                                                decimal heSo;
                                                var hsEl = el.GetProperty("heSo");
                                                heSo = hsEl.ValueKind == JsonValueKind.String
                                                     ? decimal.Parse(hsEl.GetString()!, System.Globalization.CultureInfo.InvariantCulture)
                                                     : hsEl.GetDecimal();
                                                var gc = el.TryGetProperty("ghiChu", out var pGc) ? pGc.GetString() : null;

                                                if (vtId > 0 && heSo > 0)
                                                {
                                                    lines.Add(new LenhSanXuatDong
                                                    {
                                                        LenhId = mo.Id,
                                                        VatTuId = vtId,
                                                        LoaiDong = "xuat",
                                                        HeSo = heSo,
                                                        SoLuong = 0m,        // ❗ thay null -> 0m
                                                        DonGia = 0m,        // ❗ thay null -> 0m
                                                        GiaTri = 0m,        // ❗ thay null -> 0m
                                                        GhiChu = gc,
                                                        DaXoa = false
                                                    });
                                                }
                                            }
                                            catch { /* skip dòng lỗi */ }
                                        }
                                    }
                                    if (lines.Count > 0)
                                    {
                                        db.LenhSanXuatDong.AddRange(lines);
                                        await db.SaveChangesAsync();
                                    }

                                    await tx.CommitAsync();
                                    Reply(new { ok = true, id = mo.Id, ma = mo.Ma, sanPhamId = vt.Id, sanPhamMa = vt.Ma });
                                }
                                catch (Exception inner)
                                {
                                    await tx.RollbackAsync();
                                    var m = inner.ToString();
                                       if (inner is DbUpdateException due && due.InnerException != null)
                                        m = due.InnerException.Message + " | " + due.ToString();
                                    Reply(new { ok = false, error = "exception", message = m });
                                }
                            }
                            catch (Exception ex)
                            {
                                Reply(new { ok = false, error = "exception", message = ex.ToString() });
                            }
                            break;
                        }



                    case "getMoDetail":
                        {
                            try
                            {
                                long id = msg.GetProperty("id").GetInt64();
                                using var db = new AccountingDbContext(_dbOptions);

                                var mo = await db.LenhSanXuat
                                    .Include(x => x.SanPham)
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(x => x.Id == id && !x.DaXoa);

                                if (mo == null)
                                {
                                    Reply(new { error = "not_found" });
                                    break;
                                }

                                // ========== Lấy danh sách vật tư định mức ==========
                                var dongs = await db.LenhSanXuatDong
                                    .AsNoTracking()
                                    .Where(d => d.LenhId == id && !d.DaXoa && d.LoaiDong == "xuat")
                                    .Join(db.VatTu, d => d.VatTuId, v => v.Id, (d, v) => new { d, v })
                                    .Join(db.DonViTinh, dv => dv.v.DonViTinhId, u => u.Id, (dv, u) => new
                                    {
                                        dv.d.VatTuId,
                                        dv.v.Ma,
                                        dv.v.Ten,
                                        dvtTen = u.Ten,
                                        HeSo = dv.d.HeSo ?? 0m,
                                        GhiChu = dv.d.GhiChu
                                    })
                                    .ToListAsync();

                                // ========== Tính tồn kho hiện tại ==========
                                const string TonSqlDyn = @"
DECLARE @id BIGINT = @p;
DECLARE @nhap DECIMAL(18,3)=0, @xuat DECIMAL(18,3)=0;
IF OBJECT_ID('acc.phieu_nhap_dong','U') IS NOT NULL
BEGIN
 IF COL_LENGTH('acc.phieu_nhap_dong','da_xoa') IS NOT NULL
  EXEC sp_executesql N'SELECT @out=ISNULL(SUM(so_luong),0) FROM acc.phieu_nhap_dong WHERE vat_tu_id=@id AND ISNULL(da_xoa,0)=0',
                     N'@id BIGINT,@out DECIMAL(18,3) OUTPUT', @id=@id,@out=@nhap OUTPUT;
 ELSE
  EXEC sp_executesql N'SELECT @out=ISNULL(SUM(so_luong),0) FROM acc.phieu_nhap_dong WHERE vat_tu_id=@id',
                     N'@id BIGINT,@out DECIMAL(18,3) OUTPUT', @id=@id,@out=@nhap OUTPUT;
END
IF OBJECT_ID('acc.phieu_xuat_dong','U') IS NOT NULL
BEGIN
 IF COL_LENGTH('acc.phieu_xuat_dong','da_xoa') IS NOT NULL
  EXEC sp_executesql N'SELECT @out=ISNULL(SUM(so_luong),0) FROM acc.phieu_xuat_dong WHERE vat_tu_id=@id AND ISNULL(da_xoa,0)=0',
                     N'@id BIGINT,@out DECIMAL(18,3) OUTPUT', @id=@id,@out=@xuat OUTPUT;
 ELSE
  EXEC sp_executesql N'SELECT @out=ISNULL(SUM(so_luong),0) FROM acc.phieu_xuat_dong WHERE vat_tu_id=@id',
                     N'@id BIGINT,@out DECIMAL(18,3) OUTPUT', @id=@id,@out=@xuat OUTPUT;
END
SELECT @nhap-@xuat;";

                                var ids = dongs.Select(x => x.VatTuId).Distinct().ToList();
                                var stocks = new Dictionary<long, decimal>();
                                if (ids.Count > 0)
                                {
                                    var conn = db.Database.GetDbConnection();
                                    if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync();
                                    foreach (var vtId in ids)
                                    {
                                        using var SQL2cmd = conn.CreateCommand();
                                        SQL2cmd.CommandText = TonSqlDyn;
                                        var p = SQL2cmd.CreateParameter(); p.ParameterName = "@p"; p.Value = vtId; SQL2cmd.Parameters.Add(p);
                                        var obj = await SQL2cmd.ExecuteScalarAsync();
                                        stocks[vtId] = (obj == null || obj == DBNull.Value) ? 0m : Convert.ToDecimal(obj);
                                    }
                                }

                                // ========== Ghép dữ liệu chi tiết NVL ==========
                                var plan = mo.SoLuongKeHoach;
                                var lines = dongs.Select(x =>
                                {
                                    var need = plan * x.HeSo;
                                    var ton = stocks.TryGetValue(x.VatTuId, out var t) ? t : 0m;
                                    var lack = Math.Max(0m, need - ton);
                                    return new
                                    {
                                        vatTuId = x.VatTuId,
                                        vatTuMa = x.Ma,
                                        vatTuTen = x.Ten,
                                        dvtTen = x.dvtTen,
                                        heSo = x.HeSo,
                                        slCan = need,
                                        ton,
                                        thieu = lack,
                                        ghiChu = x.GhiChu
                                    };
                                }).ToList();

                                // ========== Kiểm tra điều kiện nghiệp vụ ==========
                                // 1. Có dòng xuất NVL (đã duyệt NVL)
                                bool hasApprovedIssue = await db.LenhSanXuatDong
                                    .AnyAsync(d => d.LenhId == id && !d.DaXoa && d.LoaiDong == "xuat" && (d.SoLuong ?? 0) > 0);

                                // 2. Có dòng nhập thành phẩm hoặc có số lượng TP thực tế
                                bool hasProductIn = await db.LenhSanXuatDong
                                    .AnyAsync(d => d.LenhId == id && !d.DaXoa && d.LoaiDong == "nhap" && (d.SoLuong ?? 0) > 0);
                                bool hasTpQty = (mo.SoLuongThanhPham ?? 0) > 0;

                                // 3. Gán cờ cho UI
                                bool canApprove = (mo.TrangThai == "pending" || mo.TrangThai == "processing") && !hasApprovedIssue;
                                bool canFinish = (mo.TrangThai == "processing") && hasApprovedIssue && (hasProductIn || hasTpQty);

                                // ========== Trả dữ liệu ==========
                                Reply(new
                                {
                                    header = new
                                    {
                                        id = mo.Id,
                                        ma = mo.Ma,
                                        ngayLenh = mo.NgayLenh?.ToString("yyyy-MM-dd"),
                                        ngayIn = mo.NgayIn?.ToString("yyyy-MM-dd"),
                                        tenKhachHang = mo.TenKhachHang,
                                        tenBaiIn = mo.TenBaiIn,
                                        tenGiayIn = mo.TenGiayIn,
                                        khoIn = mo.KhoIn,
                                        soMauIn = mo.SoMauIn,
                                        hinhThucIn = mo.HinhThucIn,
                                        soCon = mo.SoCon,
                                        mayIn = mo.MayIn,
                                        soLuongKeHoach = mo.SoLuongKeHoach,
                                        soLuongThanhPham = mo.SoLuongThanhPham,
                                        trangThai = mo.TrangThai ?? "pending",
                                        sanPhamId = mo.SanPhamId,
                                        sanPhamMa = mo.SanPham?.Ma,
                                        sanPhamTen = mo.SanPham?.Ten,
                                        note = mo.GhiChu
                                    },
                                    lines,
                                    flags = new
                                    {
                                        canApprove,
                                        canFinish
                                    }
                                });
                            }
                            catch (Exception ex)
                            {
                                Reply(new { error = "exception", message = ex.Message });
                            }
                            break;
                        }




                    case "getTonKhoBatch":
                        {
                            try
                            {
                                // Lấy danh sách ids từ payload: { ids: [1,2,3] }
                                var ids = new List<long>();
                                if (msg.TryGetProperty("ids", out var pIds) && pIds.ValueKind == JsonValueKind.Array)
                                    ids = pIds.EnumerateArray().Select(x => x.GetInt64()).Distinct().ToList();

                                var stocks = new Dictionary<long, decimal>();
                                if (ids.Count == 0)
                                {
                                    Reply(new { stocks }); // rỗng cũng OK
                                    break;
                                }

                                // SQL tính tồn an toàn (dùng dynamic SQL để không vướng cột da_xoa nếu không tồn tại)
                                const string TonSqlDyn = @"
DECLARE @id BIGINT = @p;
DECLARE @nhap DECIMAL(18,3) = 0, @xuat DECIMAL(18,3) = 0;

IF OBJECT_ID('acc.phieu_nhap_dong','U') IS NOT NULL
BEGIN
    IF COL_LENGTH('acc.phieu_nhap_dong','da_xoa') IS NOT NULL
        EXEC sp_executesql
            N'SELECT @out = ISNULL(SUM(so_luong),0)
              FROM acc.phieu_nhap_dong WHERE vat_tu_id=@id AND ISNULL(da_xoa,0)=0',
            N'@id BIGINT, @out DECIMAL(18,3) OUTPUT',
            @id=@id, @out=@nhap OUTPUT;
    ELSE
        EXEC sp_executesql
            N'SELECT @out = ISNULL(SUM(so_luong),0)
              FROM acc.phieu_nhap_dong WHERE vat_tu_id=@id',
            N'@id BIGINT, @out DECIMAL(18,3) OUTPUT',
            @id=@id, @out=@nhap OUTPUT;
END

IF OBJECT_ID('acc.phieu_xuat_dong','U') IS NOT NULL
BEGIN
    IF COL_LENGTH('acc.phieu_xuat_dong','da_xoa') IS NOT NULL
        EXEC sp_executesql
            N'SELECT @out = ISNULL(SUM(so_luong),0)
              FROM acc.phieu_xuat_dong WHERE vat_tu_id=@id AND ISNULL(da_xoa,0)=0',
            N'@id BIGINT, @out DECIMAL(18,3) OUTPUT',
            @id=@id, @out=@xuat OUTPUT;
    ELSE
        EXEC sp_executesql
            N'SELECT @out = ISNULL(SUM(so_luong),0)
              FROM acc.phieu_xuat_dong WHERE vat_tu_id=@id',
            N'@id BIGINT, @out DECIMAL(18,3) OUTPUT',
            @id=@id, @out=@xuat OUTPUT;
END

SELECT @nhap - @xuat;
";

                                // Tạo DbContext riêng để tránh lỗi "A second operation was started..."
                                using var db = new AccountingDbContext(_dbOptions);
                                var conn = db.Database.GetDbConnection();
                                if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync();

                                foreach (var vtId in ids)
                                {
                                    using var SQL1cmd = conn.CreateCommand();
                                    SQL1cmd.CommandText = TonSqlDyn;
                                    var p = SQL1cmd.CreateParameter(); p.ParameterName = "@p"; p.Value = vtId; SQL1cmd.Parameters.Add(p);
                                    var obj = await SQL1cmd.ExecuteScalarAsync();
                                    stocks[vtId] = (obj == null || obj == DBNull.Value) ? 0m : Convert.ToDecimal(obj);
                                }

                                Reply(new { stocks });
                            }
                            catch (Exception ex)
                            {
                                Reply(new { error = "exception", message = ex.Message });
                            }
                            break;
                        }

                    case "approveMo":
                        {
                            try
                            {
                                long id = msg.GetProperty("id").GetInt64();

                                using var db = new AccountingDbContext(_dbOptions);
                                var mo = await db.LenhSanXuat.FirstOrDefaultAsync(x => x.Id == id && !x.DaXoa);
                                if (mo == null) { Reply(new { ok = false, error = "not_found" }); break; }
                                if ((mo.TrangThai ?? "pending") != "pending")
                                {
                                    Reply(new { ok = false, error = "bad_state", message = "Chỉ duyệt khi trạng thái CHỜ DUYỆT." });
                                    break;
                                }

                                // BOM (định mức) → nhu cầu
                                var bom = await db.LenhSanXuatDong
                                    .Where(d => d.LenhId == id && !d.DaXoa && d.LoaiDong == "xuat")
                                    .Select(d => new { d.VatTuId, HeSo = (decimal)(d.HeSo ?? 0m) })
                                    .ToListAsync();
                                if (bom.Count == 0)
                                {
                                    Reply(new { ok = false, error = "no_bom", message = "Chưa khai báo định mức vật tư." });
                                    break;
                                }
                                var planQty = mo.SoLuongKeHoach;
                                var needPlan = bom
                                    .GroupBy(x => x.VatTuId)
                                    .ToDictionary(g => g.Key, g => g.Sum(x => x.HeSo) * planQty);

                                var conn = db.Database.GetDbConnection();
                                if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync();

                                using var tx = await db.Database.BeginTransactionAsync();
                                var dbTx = tx.GetDbTransaction();

                                // 1) Kiểm tra tồn (PN - PX)
                                foreach (var kv in needPlan)
                                {
                                    using var c = conn.CreateCommand();
                                    c.Transaction = dbTx;
                                    c.CommandText = @"
DECLARE @id BIGINT = @p;
DECLARE @nhap DECIMAL(18,3) = 0, @xuat DECIMAL(18,3) = 0;
IF OBJECT_ID('acc.phieu_nhap_dong','U') IS NOT NULL
    SELECT @nhap = ISNULL(SUM(so_luong),0) FROM acc.phieu_nhap_dong WHERE vat_tu_id=@id;
IF OBJECT_ID('acc.phieu_xuat_dong','U') IS NOT NULL
    SELECT @xuat = ISNULL(SUM(so_luong),0) FROM acc.phieu_xuat_dong WHERE vat_tu_id=@id;
SELECT @nhap - @xuat;";
                                    var p = c.CreateParameter(); p.ParameterName = "@p"; p.Value = kv.Key; c.Parameters.Add(p);

                                    var obj = await c.ExecuteScalarAsync();
                                    var ton = obj == null || obj == DBNull.Value ? 0m : Convert.ToDecimal(obj);
                                    if (ton + 0.000001m < kv.Value)
                                    {
                                        await tx.RollbackAsync();
                                        Reply(new { ok = false, error = "insufficient_stock", message = "Không đủ tồn vật tư để duyệt." });
                                        goto END_APPROVE;
                                    }
                                }

                                // 2) Tạo PX (chỉ chèn cột chắc chắn tồn tại)
                                long pxId;
                                var khoId = 0L;
                                using (var c = conn.CreateCommand())
                                {
                                    c.Transaction = dbTx;
                                    c.CommandText = @"
INSERT INTO acc.phieu_xuat (so_ct)
VALUES (CONCAT('PX-', FORMAT(GETDATE(), 'yyyyMMdd-HHmm')));
SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";
                                    pxId = Convert.ToInt64(await c.ExecuteScalarAsync());
                                }
                                using (var link = conn.CreateCommand())
                                {
                                    link.Transaction = dbTx;
                                    link.CommandText = @"INSERT INTO acc.mo_phieu_xuat(mo_id, phieu_xuat_id) VALUES (@mo, @px);";
                                    var pmo = link.CreateParameter(); pmo.ParameterName = "@mo"; pmo.Value = mo.Id; link.Parameters.Add(pmo);
                                    var ppx = link.CreateParameter(); ppx.ParameterName = "@px"; ppx.Value = pxId; link.Parameters.Add(ppx);
                                    await link.ExecuteNonQueryAsync();
                                }
                                // (tuỳ schema: update thêm thông tin nếu có cột)
                                using (var u = conn.CreateCommand())
                                {
                                    u.Transaction = dbTx;
                                    u.CommandText = @"
IF COL_LENGTH('acc.phieu_xuat','ngay_xuat') IS NOT NULL
    UPDATE acc.phieu_xuat SET ngay_xuat = CAST(GETDATE() AS DATE) WHERE id = @px;
IF COL_LENGTH('acc.phieu_xuat','ghi_chu') IS NOT NULL
    UPDATE acc.phieu_xuat SET ghi_chu = N'Duyệt MO xuất vật tư kế hoạch' WHERE id = @px;
IF COL_LENGTH('acc.phieu_xuat','ngay_tao') IS NOT NULL
    UPDATE acc.phieu_xuat SET ngay_tao = GETDATE() WHERE id = @px;
IF COL_LENGTH('acc.phieu_xuat','nguoi_tao') IS NOT NULL
    UPDATE acc.phieu_xuat SET nguoi_tao = SYSTEM_USER WHERE id = @px;";
                                    var p = u.CreateParameter(); p.ParameterName = "@px"; p.Value = pxId; u.Parameters.Add(p);
                                    await u.ExecuteNonQueryAsync();
                                }

                                // 3) PX dòng – KHÔNG NULL cho don_gia/gia_tri
                                foreach (var kv in needPlan)
                                {
                                    using var c = conn.CreateCommand();
                                    c.Transaction = dbTx;
                                    c.CommandText = @"
INSERT INTO acc.phieu_xuat_dong
    (phieu_xuat_id, vat_tu_id, so_luong, don_gia, gia_tri)
VALUES
    (@px, @vt, @sl, @dg, @gt);";


                                    var dg = 0m;
                                    var sl = Convert.ToDecimal(kv.Value);
                                    var gt = Math.Round(sl * dg, 2);

                                    c.Parameters.Clear();
                                    var p1 = c.CreateParameter(); p1.ParameterName = "@px"; p1.Value = pxId; c.Parameters.Add(p1);
                                    var p2 = c.CreateParameter(); p2.ParameterName = "@vt"; p2.Value = kv.Key; c.Parameters.Add(p2);
                                    var p3 = c.CreateParameter(); p3.ParameterName = "@sl"; p3.Value = sl; c.Parameters.Add(p3);
                                    var p4 = c.CreateParameter(); p4.ParameterName = "@dg"; p4.Value = dg; c.Parameters.Add(p4);
                                    var p5 = c.CreateParameter(); p5.ParameterName = "@gt"; p5.Value = gt; c.Parameters.Add(p5);

                                    await c.ExecuteNonQueryAsync();
                                }

                                // 4) Cập nhật trạng thái
                                mo.TrangThai = "processing";
                                await db.SaveChangesAsync();

                                await tx.CommitAsync();
                                Reply(new { ok = true, pxId });

                            END_APPROVE:;
                            }
                            catch (Exception ex)
                            {
                                Reply(new { ok = false, error = "exception", message = ex.ToString() });
                            }
                            break;
                        }



                    case "completeMo":
                        {
                            try
                            {
                                long id = msg.GetProperty("id").GetInt64();
                                decimal slTp = msg.TryGetProperty("slTp", out var jSl) ? (decimal)jSl.GetDecimal() : 0m;

                                using var db = new AccountingDbContext(_dbOptions);
                                var mo = await db.LenhSanXuat.Include(x => x.SanPham)
                                                             .FirstOrDefaultAsync(x => x.Id == id && !x.DaXoa);
                                if (mo == null) { Reply(new { ok = false, error = "not_found" }); break; }
                                if ((mo.TrangThai ?? "pending") != "processing")
                                {
                                    Reply(new { ok = false, error = "bad_state", message = "Chỉ hoàn thành khi trạng thái ĐANG SẢN XUẤT." });
                                    break;
                                }
                                if (slTp <= 0) { Reply(new { ok = false, error = "bad_input", message = "SL thành phẩm phải > 0." }); break; }

                                var conn = db.Database.GetDbConnection();
                                if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync();

                                using var tx = await db.Database.BeginTransactionAsync();
                                var dbTx = tx.GetDbTransaction();

                                // 1) PN header (GIỮ NGUYÊN theo yêu cầu: chỉ chèn so_ct)
                                long pnId;
                                using (var c = conn.CreateCommand())
                                {
                                    c.Transaction = dbTx;
                                    c.CommandText = @"
INSERT INTO acc.phieu_nhap (so_ct)
VALUES (CONCAT('PN-', FORMAT(GETDATE(),'yyyyMMdd-HHmm')));
SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";
                                    pnId = Convert.ToInt64(await c.ExecuteScalarAsync());
                                }

                                // Bổ sung metadata (không đụng kho_id theo yêu cầu trước đó)
                                using (var u = conn.CreateCommand())
                                {
                                    u.Transaction = dbTx;
                                    u.CommandText = @"
IF COL_LENGTH('acc.phieu_nhap','ngay_nhap') IS NOT NULL
    UPDATE acc.phieu_nhap SET ngay_nhap = CAST(GETDATE() AS DATE) WHERE id = @pn;
IF COL_LENGTH('acc.phieu_nhap','ghi_chu') IS NOT NULL
    UPDATE acc.phieu_nhap SET ghi_chu = N'Nhập thành phẩm từ lệnh sản xuất' WHERE id = @pn;
IF COL_LENGTH('acc.phieu_nhap','ngay_tao') IS NOT NULL
    UPDATE acc.phieu_nhap SET ngay_tao = GETDATE() WHERE id = @pn;
IF COL_LENGTH('acc.phieu_nhap','nguoi_tao') IS NOT NULL
    UPDATE acc.phieu_nhap SET nguoi_tao = SYSTEM_USER WHERE id = @pn;";
                                    var p = u.CreateParameter(); p.ParameterName = "@pn"; p.Value = pnId; u.Parameters.Add(p);
                                    await u.ExecuteNonQueryAsync();
                                }

                                // 2) PN dòng – nhập THÀNH PHẨM (don_gia/gia_tri = 0, không để NULL)
                                var tpVatTuId = mo.SanPhamId;  // TP đã tạo ở createMo
                                using (var c = conn.CreateCommand())
                                {
                                    c.Transaction = dbTx;
                                    c.CommandText = @"
INSERT INTO acc.phieu_nhap_dong
    (phieu_nhap_id, vat_tu_id, so_luong, don_gia, gia_tri)
VALUES
    (@pn, @vt, @sl, @dg, @gt);";
                                    var dg = 0m;
                                    var sl = slTp;
                                    var gt = Math.Round(sl * dg, 2);

                                    c.Parameters.Clear();
                                    var p1 = c.CreateParameter(); p1.ParameterName = "@pn"; p1.Value = pnId; c.Parameters.Add(p1);
                                    var p2 = c.CreateParameter(); p2.ParameterName = "@vt"; p2.Value = tpVatTuId; c.Parameters.Add(p2);
                                    var p3 = c.CreateParameter(); p3.ParameterName = "@sl"; p3.Value = sl; c.Parameters.Add(p3);
                                    var p4 = c.CreateParameter(); p4.ParameterName = "@dg"; p4.Value = dg; c.Parameters.Add(p4);
                                    var p5 = c.CreateParameter(); p5.ParameterName = "@gt"; p5.Value = gt; c.Parameters.Add(p5);
                                    await c.ExecuteNonQueryAsync();
                                }

                                // 2.1) Đảm bảo cờ phân loại TP (để UI lọc đúng)
                                using (var cFlag = conn.CreateCommand())
                                {
                                    cFlag.Transaction = dbTx;
                                    cFlag.CommandText = @"
IF COL_LENGTH('acc.vat_tu','is_thanh_pham') IS NOT NULL
    UPDATE acc.vat_tu SET is_thanh_pham = 1 WHERE id = @vt;";
                                    var pvt = cFlag.CreateParameter(); pvt.ParameterName = "@vt"; pvt.Value = tpVatTuId; cFlag.Parameters.Add(pvt);
                                    await cFlag.ExecuteNonQueryAsync();
                                }

                                // 3) TÍNH TRẢ VẬT TƯ DƯ
                                // 3.1) BOM của MO (Loại 'xuat'): nhu cầu thực = HeSo * slTp
                                var bom = await db.LenhSanXuatDong
                                    .Where(d => d.LenhId == mo.Id && !d.DaXoa && d.LoaiDong == "xuat")
                                    .Select(d => new { d.VatTuId, HeSo = (decimal)(d.HeSo ?? 0m) })
                                    .ToListAsync();

                                var actualNeed = bom
                                    .GroupBy(x => x.VatTuId)
                                    .ToDictionary(g => g.Key, g => g.Sum(x => x.HeSo) * slTp);

                                // 3.2) Tổng đã xuất cho MO này
                                var issued = new Dictionary<long, decimal>();

                                // ƯU TIÊN: bảng mapping acc.mo_phieu_xuat (nếu có)
                                bool hasMap = false;
                                using (var chk = conn.CreateCommand())
                                {
                                    chk.Transaction = dbTx;
                                    chk.CommandText = "SELECT CASE WHEN OBJECT_ID('acc.mo_phieu_xuat','U') IS NULL THEN 0 ELSE 1 END";
                                    hasMap = Convert.ToInt32(await chk.ExecuteScalarAsync()) == 1;
                                }

                                if (hasMap)
                                {
                                    using var c2 = conn.CreateCommand();
                                    c2.Transaction = dbTx;
                                    c2.CommandText = @"
SELECT d.vat_tu_id, SUM(d.so_luong) AS sl
FROM acc.phieu_xuat_dong d
JOIN acc.mo_phieu_xuat mpx ON mpx.phieu_xuat_id = d.phieu_xuat_id
WHERE mpx.mo_id = @mo
GROUP BY d.vat_tu_id;";
                                    var pmo = c2.CreateParameter(); pmo.ParameterName = "@mo"; pmo.Value = mo.Id; c2.Parameters.Add(pmo);

                                    using var rdr = await c2.ExecuteReaderAsync();
                                    while (await rdr.ReadAsync())
                                    {
                                        var vt = Convert.ToInt64(rdr.GetValue(0));
                                        var sl = Convert.ToDecimal(rdr.GetValue(1));
                                        issued[vt] = sl;
                                    }
                                }
                                else
                                {
                                    // FALLBACK: dò PX theo ghi_chu có 'MO:<id>'
                                    using var c2 = conn.CreateCommand();
                                    c2.Transaction = dbTx;
                                    c2.CommandText = @"
SELECT d.vat_tu_id, SUM(d.so_luong) AS sl
FROM acc.phieu_xuat_dong d
JOIN acc.phieu_xuat px ON px.id = d.phieu_xuat_id
WHERE px.ghi_chu LIKE @tag
GROUP BY d.vat_tu_id;";
                                    var ptag = c2.CreateParameter(); ptag.ParameterName = "@tag"; ptag.Value = $"%MO:{mo.Id}%"; c2.Parameters.Add(ptag);

                                    using var rdr = await c2.ExecuteReaderAsync();
                                    while (await rdr.ReadAsync())
                                    {
                                        var vt = Convert.ToInt64(rdr.GetValue(0));
                                        var sl = Convert.ToDecimal(rdr.GetValue(1));
                                        issued[vt] = sl;
                                    }
                                }

                                // 3.3) Tính phần dư = đã xuất - nhu cầu thực; lập PN trả nếu có dư
                                var returns = new List<(long vtId, decimal sl)>();
                                foreach (var kv in issued)
                                {
                                    var vt = kv.Key;
                                    var slIssued = kv.Value;
                                    var need = actualNeed.TryGetValue(vt, out var n) ? n : 0m;
                                    var du = slIssued - need;
                                    if (du > 0.000001m)
                                        returns.Add((vt, decimal.Round(du, 3, MidpointRounding.AwayFromZero)));
                                }

                                long? pnTraId = null;
                                if (returns.Count > 0)
                                {
                                    // PN trả vật tư dư (nhẹ nhàng giữ style: chèn mỗi so_ct)
                                    using (var c3 = conn.CreateCommand())
                                    {
                                        c3.Transaction = dbTx;
                                        c3.CommandText = @"
INSERT INTO acc.phieu_nhap (so_ct)
VALUES (CONCAT('PN-TRAVT-', FORMAT(GETDATE(),'yyyyMMdd-HHmm')));
SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";
                                        pnTraId = Convert.ToInt64(await c3.ExecuteScalarAsync());
                                    }

                                    // ghi chú mô tả PN trả
                                    using (var u2 = conn.CreateCommand())
                                    {
                                        u2.Transaction = dbTx;
                                        u2.CommandText = @"
IF COL_LENGTH('acc.phieu_nhap','ngay_nhap') IS NOT NULL
    UPDATE acc.phieu_nhap SET ngay_nhap = CAST(GETDATE() AS DATE) WHERE id = @pn;
IF COL_LENGTH('acc.phieu_nhap','ghi_chu') IS NOT NULL
    UPDATE acc.phieu_nhap SET ghi_chu = CONCAT(N'Nhập trả vật tư dư | MO:', @mo) WHERE id = @pn;
IF COL_LENGTH('acc.phieu_nhap','ngay_tao') IS NOT NULL
    UPDATE acc.phieu_nhap SET ngay_tao = GETDATE() WHERE id = @pn;
IF COL_LENGTH('acc.phieu_nhap','nguoi_tao') IS NOT NULL
    UPDATE acc.phieu_nhap SET nguoi_tao = SYSTEM_USER WHERE id = @pn;";
                                        var pPn = u2.CreateParameter(); pPn.ParameterName = "@pn"; pPn.Value = pnTraId; u2.Parameters.Add(pPn);
                                        var pMo = u2.CreateParameter(); pMo.ParameterName = "@mo"; pMo.Value = mo.Id; u2.Parameters.Add(pMo);
                                        await u2.ExecuteNonQueryAsync();
                                    }

                                    // các dòng trả
                                    foreach (var (vtId, slDu) in returns)
                                    {
                                        using var c4 = conn.CreateCommand();
                                        c4.Transaction = dbTx;
                                        c4.CommandText = @"
INSERT INTO acc.phieu_nhap_dong (phieu_nhap_id, vat_tu_id, so_luong, don_gia, gia_tri)
VALUES (@pn, @vt, @sl, @dg, @gt);";
                                        var dg = 0m; var gt = 0m;

                                        var q1 = c4.CreateParameter(); q1.ParameterName = "@pn"; q1.Value = pnTraId; c4.Parameters.Add(q1);
                                        var q2 = c4.CreateParameter(); q2.ParameterName = "@vt"; q2.Value = vtId; c4.Parameters.Add(q2);
                                        var q3 = c4.CreateParameter(); q3.ParameterName = "@sl"; q3.Value = slDu; c4.Parameters.Add(q3);
                                        var q4 = c4.CreateParameter(); q4.ParameterName = "@dg"; q4.Value = dg; c4.Parameters.Add(q4);
                                        var q5 = c4.CreateParameter(); q5.ParameterName = "@gt"; q5.Value = gt; c4.Parameters.Add(q5);
                                        await c4.ExecuteNonQueryAsync();
                                    }
                                }

                                // 4) Cập nhật trạng thái & SL thành phẩm
                                mo.TrangThai = "done";
                                mo.SoLuongThucTe = slTp;
                                // nếu entity có cột này (theo code createMo trước đó), set luôn để UI hiển thị đúng:
                                mo.SoLuongThanhPham = slTp;

                                await db.SaveChangesAsync();
                                await tx.CommitAsync();

                                Reply(new { ok = true, pnId, pnTraId });
                            }
                            catch (Exception ex)
                            {
                                Reply(new { ok = false, error = "exception", message = ex.ToString() });
                            }
                            break;
                        }




                    case "getSalesList":
                        {
                            using var db = new AccountingDbContext(_dbOptions);

                            var list = await db.DonBan
                                .AsNoTracking()
                                .Where(x => !x.DaXoa)
                                .OrderByDescending(x => x.NgayDon)
                                .Select(x => new
                                {
                                    x.Id,
                                    x.SoCt,
                                    x.NgayDon,
                                    x.TrangThai,
                                    GiaTri = x.TongTien,            // <- dùng TongTien để hiển thị "Giá trị"
                                    KhachHang = db.KhachHang
                                        .Where(k => k.Id == x.KhachHangId)
                                        .Select(k => k.Ten)
                                        .FirstOrDefault()
                                })
                                .ToListAsync();

                            Reply(new { items = list });
                            break;
                        }


                    case "getSalesDetail":
                        {
                            long id = msg.GetProperty("id").GetInt64();
                            using var db = new AccountingDbContext(_dbOptions);

                            var header = await db.DonBan
                                .AsNoTracking()
                                .Where(x => x.Id == id && !x.DaXoa)
                                .Select(x => new {
                                    x.Id,
                                    x.SoCt,
                                    x.NgayDon,
                                    x.TrangThai,
                                    x.GhiChu,
                                    x.KhachHangId,
                                    KhachHangTen = db.KhachHang.Where(k => k.Id == x.KhachHangId)
                                                                .Select(k => k.Ten).FirstOrDefault()
                                })
                                .FirstOrDefaultAsync();

                            if (header == null) { Reply(new { error = "not_found" }); break; }

                            var lines = await db.DonBanDong
                                .AsNoTracking()
                                .Where(d => d.DonBanId == id && !d.DaXoa)
                                .Select(d => new {
                                    d.Id,
                                    d.DonBanId,
                                    d.TenHang,
                                    d.QuyCach,
                                    d.SoLuong,
                                    d.DonGia,
                                    d.ThueSuat,
                                    d.ThanhTien
                                })
                                .ToListAsync();

                            Reply(new { header, lines });
                            break;
                        }



                    case "saveSalesOrder":
                        {
                            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                            var header = msg.GetProperty("header").Deserialize<DonBan>(opts) ?? new DonBan();
                            var lines = msg.GetProperty("lines").Deserialize<List<DonBanDong>>(opts) ?? new();

                            using var db = new AccountingDbContext(_dbOptions);
                            await using var tx = await db.Database.BeginTransactionAsync();

                            // 0) Validate
                            if (header.KhachHangId <= 0)
                            {
                                Reply(new { error = "khach_hang_required" });
                                break;
                            }

                            // 1) Tính tổng từ lines
                            decimal tienHang = 0, tienThue = 0;
                            foreach (var l in lines)
                            {
                                var hang = l.SoLuong * l.DonGia;
                                var thue = hang * (l.ThueSuat / 100m);
                                l.ThanhTien = hang + thue;
                                l.DaXoa = false;
                                tienHang += hang;
                                tienThue += thue;
                            }
                            header.TienHang = tienHang;
                            header.TienThue = tienThue;
                            header.TongTien = tienHang + tienThue;

                            // 2) Lưu header trước để có Id
                            if (header.Id == 0)
                            {
                                // tự sinh số nếu cần
                                if (string.IsNullOrWhiteSpace(header.SoCt))
                                    header.SoCt = await GenerateNextSoCtAsync(db, "SO");

                                header.NgayTao = DateTime.Now;
                                db.DonBan.Add(header);
                                await db.SaveChangesAsync(); // <-- LÚC NÀY mới có header.Id
                            }
                            else
                            {
                                header.NgayCapNhat = DateTime.Now;

                                // Không để EF coi header là entity mới
                                db.DonBan.Attach(header);
                                db.Entry(header).State = EntityState.Modified;
                                await db.SaveChangesAsync();

                                // Xoá hết dòng cũ rồi thêm lại
                                var oldLines = await db.DonBanDong
                                    .Where(x => x.DonBanId == header.Id)
                                    .ToListAsync();
                                db.DonBanDong.RemoveRange(oldLines);
                                await db.SaveChangesAsync();
                            }

                            // 3) Gán DonBanId cho từng dòng rồi thêm
                            foreach (var l in lines)
                            {
                                l.Id = 0;                   // đảm bảo INSERT
                                l.DonBanId = header.Id;     // <-- quan trọng
                                l.DaXoa = false;
                            }
                            db.DonBanDong.AddRange(lines);
                            await db.SaveChangesAsync();

                            await tx.CommitAsync();

                            Reply(new { ok = true, id = header.Id, soCt = header.SoCt });
                            break;
                        }


                    case "changeSalesStatus":
                        {
                            using var db = new AccountingDbContext(_dbOptions);

                            var idProp = msg.GetProperty("id");
                            long id = idProp.ValueKind == JsonValueKind.Number
                                ? idProp.GetInt64()
                                : long.Parse(idProp.GetString() ?? "0");

                            var action = msg.GetProperty("action").GetString();   // confirm | start_production | ship | invoice | pay | cancel
                            var note = msg.TryGetProperty("note", out var np) ? np.GetString() : null;

                            var o = await db.DonBan.FirstOrDefaultAsync(x => x.Id == id && !x.DaXoa);
                            if (o == null) { Reply(new { error = "not_found" }); break; }

                            string next = ResolveNextStatus(o.TrangThai ?? "draft", action ?? "");
                            if (next == null)
                            {
                                Reply(new { error = "invalid_transition", from = o.TrangThai, action });
                                break;
                            }

                            // (tùy chọn) kiểm tra điều kiện nghiệp vụ trước khi cho qua:
                            // - confirm: phải có ≥1 dòng
                            // - ship: đã có phiếu xuất thành phẩm
                            // - invoice: đã tạo hóa đơn
                            // - pay: đã có chứng từ thu
                            // => nếu chưa đủ, Reply lỗi tương ứng

                            o.TrangThai = next;
                            o.NgayCapNhat = DateTime.Now;
                            await db.SaveChangesAsync();

                            Reply(new { ok = true, status = o.TrangThai });
                            break;
                        }

                    // Tìm KH theo từ khóa (mã / tên / sđt / email)
                    case "searchCustomers":
                        {
                            var q = msg.GetProperty("q").GetString() ?? "";
                            var limit = msg.TryGetProperty("limit", out var lp) && lp.ValueKind == JsonValueKind.Number ? lp.GetInt32() : 10;

                            using var db = new AccountingDbContext(_dbOptions);
                            var items = await db.KhachHang
                                .AsNoTracking()
                                .Where(k => !k.DaXoa &&
                                       (k.Ten.Contains(q) || k.Ma.Contains(q) || (k.SoDienThoai ?? "").Contains(q) || (k.Email ?? "").Contains(q)))
                                .OrderBy(k => k.Ten)
                                .Take(limit)
                                .Select(k => new { k.Id, k.Ma, k.Ten, k.SoDienThoai, k.Email })
                                .ToListAsync();

                            Reply(new { items });
                            break;
                        }

                    // KH đã từng mua gần đây (ưu tiên có đơn bán, fallback KH mới cập nhật)
                    case "recentCustomers":
                        {
                            var limit = msg.TryGetProperty("limit", out var lp) && lp.ValueKind == JsonValueKind.Number ? lp.GetInt32() : 12;
                            using var db = new AccountingDbContext(_dbOptions);

                            var items = await db.DonBan
                                .Where(d => !d.DaXoa)
                                .OrderByDescending(d => d.NgayDon)
                                .Select(d => d.KhachHangId)
                                .Distinct()
                                .Take(limit)
                                .Join(db.KhachHang, id => id, k => k.Id, (id, k) => new { k.Id, k.Ma, k.Ten, k.SoDienThoai, k.Email })
                                .ToListAsync();

                            // nếu chưa có đơn nào, fallback danh sách KH mới tạo
                            if (!items.Any())
                            {
                                items = await db.KhachHang
                                    .AsNoTracking()
                                    .Where(k => !k.DaXoa)
                                    .OrderByDescending(k => k.NgayCapNhat ?? k.NgayTao)
                                    .Take(limit)
                                    .Select(k => new { k.Id, k.Ma, k.Ten, k.SoDienThoai, k.Email })
                                    .ToListAsync();
                            }

                            Reply(new { items });
                            break;
                        }

                    // Tạo khách hàng nhanh
                    case "quickCreateCustomer":
                        {
                            using var db = new AccountingDbContext(_dbOptions);

                            var ten = msg.GetProperty("Ten").GetString() ?? "";

                            string? ma = null, sdt = null, email = null, diachi = null, mst = null;

                            if (msg.TryGetProperty("Ma", out var maEl)) ma = maEl.GetString();
                            if (msg.TryGetProperty("SoDienThoai", out var sdtEl)) sdt = sdtEl.GetString();
                            if (msg.TryGetProperty("Email", out var emailEl)) email = emailEl.GetString();
                            if (msg.TryGetProperty("DiaChi", out var diachiEl)) diachi = diachiEl.GetString();
                            if (msg.TryGetProperty("MaSoThue", out var mstEl)) mst = mstEl.GetString();

                            if (string.IsNullOrWhiteSpace(ten)) { Reply(new { error = "ten_required" }); break; }

                            // Tự sinh mã nếu trống
                            if (string.IsNullOrWhiteSpace(ma))
                            {
                                var prefix = "KH" + DateTime.Now.ToString("yyMM");
                                var lastMa = await db.KhachHang
                                    .Where(x => x.Ma.StartsWith(prefix))
                                    .OrderByDescending(x => x.Ma)
                                    .Select(x => x.Ma)
                                    .FirstOrDefaultAsync();

                                int next = 1;
                                if (!string.IsNullOrEmpty(lastMa) && int.TryParse(lastMa[^4..], out var lastNo))
                                    next = lastNo + 1;

                                ma = $"{prefix}{next:D4}";
                            }

                            var kh = new KhachHang
                            {
                                Ma = ma!,
                                Ten = ten,
                                SoDienThoai = sdt,
                                Email = email,
                                DiaChi = diachi,
                                MaSoThue = mst,
                                NgayTao = DateTime.Now,
                                DaXoa = false
                            };

                            db.KhachHang.Add(kh);
                            await db.SaveChangesAsync();

                            Reply(new { ok = true, id = kh.Id, ma = kh.Ma });
                            break;
                        }

                    case "exportSalesQuote":
                        {
                            using var db = new AccountingDbContext(_dbOptions);

                            long id = msg.GetProperty("id").GetInt64();

                            var h = await db.DonBan.AsNoTracking()
                                .Where(x => x.Id == id && !x.DaXoa)
                                .Select(x => new {
                                    x.Id,
                                    x.SoCt,
                                    x.NgayDon,
                                    x.GhiChu,
                                    x.TrangThai,
                                    Khach = db.KhachHang.Where(k => k.Id == x.KhachHangId).Select(k => k.Ten).FirstOrDefault()
                                }).FirstOrDefaultAsync();
                            if (h == null) { Reply(new { error = "not_found" }); break; }

                            var lines = await db.DonBanDong.AsNoTracking()
                                .Where(d => d.DonBanId == id && !d.DaXoa)
                                .Select(d => new { d.TenHang, d.QuyCach, d.SoLuong, d.DonGia, d.ThueSuat })
                                .ToListAsync();

                            decimal hang = 0, thue = 0; foreach (var l in lines) { hang += l.SoLuong * l.DonGia; thue += (l.SoLuong * l.DonGia) * (l.ThueSuat / 100m); }
                            var tong = hang + thue;

                            // === nơi lưu: bin\Debug\netX\wwwroot\quotes ===
                            var root = Path.Combine(AppContext.BaseDirectory, "wwwroot", "quotes");
                            Directory.CreateDirectory(root);

                            var fileName = $"Quote_{(h.SoCt ?? "").Replace("/", "-").Replace("\\", "-").Replace(":", "_")}_{DateTime.Now:yyyyMMdd_HHmm}.pdf";
                            var fullPath = Path.Combine(root, fileName);

                            // ---- QuestPDF render ----
                            var viVN = new System.Globalization.CultureInfo("vi-VN");
                            string Money(decimal v) => string.Format(viVN, "{0:C0}", v);
                            QuestPDF.Settings.License = LicenseType.Community;

                            var doc = Document.Create(c => c.Page(p =>
                            {
                                p.Margin(36);
                                p.Header().Text($"BÁO GIÁ ĐƠN HÀNG: {h.SoCt}").SemiBold().FontSize(16);
                                p.Content().Column(col =>
                                {
                                    col.Spacing(6);
                                    col.Item().Text($"Khách hàng: {h.Khach}");
                                    col.Item().Text($"Ngày: {h.NgayDon:dd/MM/yyyy}");
                                    if (!string.IsNullOrWhiteSpace(h.GhiChu)) col.Item().Text($"Ghi chú: {h.GhiChu}");

                                    col.Item().Table(t =>
                                    {
                                        t.ColumnsDefinition(x => { x.RelativeColumn(3); x.RelativeColumn(3); x.RelativeColumn(1); x.RelativeColumn(2); x.RelativeColumn(1); x.RelativeColumn(2); });
                                        t.Header(hd => { hd.Cell().Text("Tên hàng").SemiBold(); hd.Cell().Text("Quy cách").SemiBold(); hd.Cell().Text("SL").SemiBold(); hd.Cell().Text("Đơn giá").SemiBold(); hd.Cell().Text("VAT%").SemiBold(); hd.Cell().Text("Thành tiền").SemiBold(); });
                                        foreach (var l in lines)
                                        {
                                            var h1 = l.SoLuong * l.DonGia; var t1 = h1 * (l.ThueSuat / 100m); var tt = h1 + t1;
                                            t.Cell().Text(l.TenHang ?? ""); t.Cell().Text(l.QuyCach ?? ""); t.Cell().Text(l.SoLuong.ToString("0.##"));
                                            t.Cell().Text(Money(l.DonGia)); t.Cell().Text(l.ThueSuat.ToString("0.#")); t.Cell().Text(Money(tt));
                                        }
                                    });
                                });
                                p.Footer().Column(c2 => {
                                    c2.Item().AlignRight().Text($"Tổng tiền hàng: {Money(hang)}");
                                    c2.Item().AlignRight().Text($"Thuế VAT: {Money(thue)}");
                                    c2.Item().AlignRight().Text($"Tổng cộng: {Money(tong)}").SemiBold();
                                    c2.Item().AlignRight().Text($"Generated at {DateTime.Now:dd/MM/yyyy HH:mm}");
                                });
                            }));
                            doc.GeneratePdf(fullPath);

                            // cập nhật trạng thái
                            var ord = await db.DonBan.FirstAsync(x => x.Id == id);
                            ord.TrangThai = "quote";
                            ord.NgayCapNhat = DateTime.Now;
                            await db.SaveChangesAsync();

                            // Trả cả 2 loại URL: nếu không host static, dùng fileUrl
                            var downloadUrl = $"/quotes/{fileName}";
                            var fileUrl = new Uri(fullPath).AbsoluteUri;   // file:///C:/...

                            Reply(new { ok = true, downloadUrl, fileUrl, status = "quote" });
                            break;
                        }

                    case "makeQuote":   // hoặc "exportSalesQuote"
                        {
                            try
                            {
                                using var db = new AccountingDbContext(_dbOptions);

                                // --- đọc id an toàn ---
                                long id = msg.TryGetProperty("id", out var j)
                                        ? (j.ValueKind == JsonValueKind.Number ? j.GetInt64() : long.Parse(j.GetString() ?? "0"))
                                        : 0;
                                if (id <= 0) { Reply(new { ok = false, error = "invalid_id" }); break; }

                                // --- header ---
                                var h = await db.DonBan
                                    .AsNoTracking()
                                    .Where(x => x.Id == id && !x.DaXoa)
                                    .Select(x => new {
                                        x.Id,
                                        x.SoCt,
                                        x.NgayDon,
                                        x.GhiChu,
                                        x.TrangThai,
                                        x.KhachHangId,
                                        Khach = db.KhachHang.Where(k => k.Id == x.KhachHangId).Select(k => k.Ten).FirstOrDefault()
                                    })
                                    .FirstOrDefaultAsync();

                                if (h == null) { Reply(new { ok = false, error = "not_found" }); break; }

                                // --- lines ---
                                var lines = await db.DonBanDong
                                    .AsNoTracking()
                                    .Where(d => d.DonBanId == id && !d.DaXoa)
                                    .Select(d => new { d.TenHang, d.QuyCach, d.SoLuong, d.DonGia, d.ThueSuat })
                                    .ToListAsync();

                                if (lines.Count == 0) { Reply(new { ok = false, error = "no_lines" }); break; }

                                // --- tính tổng ---
                                decimal tienHang = 0, tienThue = 0;
                                foreach (var l in lines)
                                {
                                    var hang = l.SoLuong * l.DonGia;
                                    var thue = hang * (l.ThueSuat / 100m);
                                    tienHang += hang; tienThue += thue;
                                }
                                var tong = tienHang + tienThue;

                                // --- tạo đường dẫn file ---
                                var root = Path.Combine(AppContext.BaseDirectory, "wwwroot", "quotes");
                                Directory.CreateDirectory(root);
                                var safeSoCt = (h.SoCt ?? "").Replace("/", "-").Replace("\\", "-").Replace(":", "_");
                                var fileName = $"Quote_{(string.IsNullOrWhiteSpace(safeSoCt) ? id.ToString() : safeSoCt)}_{DateTime.Now:yyyyMMdd_HHmm}.pdf";
                                var fullPath = Path.Combine(root, fileName);

                                // --- render PDF bằng QuestPDF ---
                                QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
                                var viVN = new System.Globalization.CultureInfo("vi-VN");
                                string Money(decimal v) => string.Format(viVN, "{0:C0}", v);

                                var doc = QuestPDF.Fluent.Document.Create(container =>
                                {
                                    container.Page(page =>
                                    {
                                        page.Margin(36);

                                        page.Header().Column(col =>
                                        {
                                            col.Spacing(4);
                                            col.Item().Text($"BÁO GIÁ ĐƠN HÀNG: {h.SoCt ?? $"#{id}"}").SemiBold().FontSize(16);
                                            col.Item().Text($"Khách hàng: {h.Khach}");
                                            col.Item().Text($"Ngày: {h.NgayDon:dd/MM/yyyy}");
                                            if (!string.IsNullOrWhiteSpace(h.GhiChu)) col.Item().Text($"Ghi chú: {h.GhiChu}");
                                        });

                                        page.Content().Table(t =>
                                        {
                                            t.ColumnsDefinition(c =>
                                            {
                                                c.RelativeColumn(3);
                                                c.RelativeColumn(3);
                                                c.RelativeColumn(1);
                                                c.RelativeColumn(2);
                                                c.RelativeColumn(1);
                                                c.RelativeColumn(2);
                                            });

                                            t.Header(hd =>
                                            {
                                                hd.Cell().Text("Tên hàng").SemiBold();
                                                hd.Cell().Text("Quy cách").SemiBold();
                                                hd.Cell().Text("SL").SemiBold();
                                                hd.Cell().Text("Đơn giá").SemiBold();
                                                hd.Cell().Text("VAT%").SemiBold();
                                                hd.Cell().Text("Thành tiền").SemiBold();
                                            });

                                            foreach (var l in lines)
                                            {
                                                var hang = l.SoLuong * l.DonGia;
                                                var thue = hang * (l.ThueSuat / 100m);
                                                var tt = hang + thue;

                                                t.Cell().Text(l.TenHang ?? "");
                                                t.Cell().Text(l.QuyCach ?? "");
                                                t.Cell().Text(l.SoLuong.ToString("0.##"));
                                                t.Cell().Text(Money(l.DonGia));
                                                t.Cell().Text(l.ThueSuat.ToString("0.#"));
                                                t.Cell().Text(Money(tt));
                                            }
                                        });

                                        page.Footer().Column(col =>
                                        {
                                            col.Spacing(3);
                                            col.Item().AlignRight().Text($"Tổng tiền hàng: {Money(tienHang)}");
                                            col.Item().AlignRight().Text($"Thuế VAT: {Money(tienThue)}");
                                            col.Item().AlignRight().Text($"Tổng cộng: {Money(tong)}").SemiBold();
                                        });
                                    });
                                });

                                doc.GeneratePdf(fullPath);

                                // --- cập nhật trạng thái ---
                                var ord = await db.DonBan.FirstAsync(x => x.Id == id);
                                ord.TrangThai = "quote";
                                ord.NgayCapNhat = DateTime.Now;
                                await db.SaveChangesAsync();

                                var downloadUrl = $"/quotes/{fileName}";
                                var fileUrl = new Uri(fullPath).AbsoluteUri; // file:///C:/...

                                Reply(new { ok = true, downloadUrl, fileUrl, status = "quote" });
                            }
                            catch (Exception ex)
                            {
                                Reply(new { ok = false, error = ex.Message });
                            }
                            break;
                        }


                    case "saveMoFromSo":
                        {
                            await using var db = new AccountingDbContext(_dbOptions);
                            await using var tx = await db.Database.BeginTransactionAsync();

                            try
                            {
                                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                                var body = msg.Deserialize<Dictionary<string, object>>(opts)!;

                                if (!body.TryGetValue("header", out var headerObj))
                                {
                                    Reply(new { ok = false, error = "Thiếu 'header'." });
                                    break;
                                }

                                var headerEl = (JsonElement)headerObj;
                                var tpNewEl = body.TryGetValue("tpNew", out var tpObj) ? (JsonElement)tpObj : default;
                                var dinhMucEl = body.TryGetValue("dinhMuc", out var dmObj) ? (JsonElement)dmObj : default;

                                static string? GetStr(JsonElement e, string name)
                                    => e.ValueKind != JsonValueKind.Undefined && e.TryGetProperty(name, out var v) ? v.GetString() : null;

                                static DateTime? GetDateN(JsonElement e, string name)
                                {
                                    if (e.ValueKind == JsonValueKind.Undefined || !e.TryGetProperty(name, out var v)) return null;
                                    if (v.ValueKind == JsonValueKind.Null) return null;
                                    if (v.ValueKind == JsonValueKind.String && DateTime.TryParse(v.GetString(), out var d1)) return d1;
                                    if (v.ValueKind == JsonValueKind.Number && v.TryGetDateTime(out var d2)) return d2;
                                    return null;
                                }

                                static int GetInt(JsonElement e, string name, int def = 0)
                                {
                                    if (e.ValueKind == JsonValueKind.Undefined || !e.TryGetProperty(name, out var v)) return def;
                                    if (v.ValueKind == JsonValueKind.Number) return (int)Math.Round(v.GetDouble());
                                    if (int.TryParse(v.GetString(), out var n)) return n;
                                    return def;
                                }

                                static decimal GetDec(JsonElement e, string name, decimal def = 0)
                                {
                                    if (e.ValueKind == JsonValueKind.Undefined || !e.TryGetProperty(name, out var v)) return def;
                                    if (v.ValueKind == JsonValueKind.Number) return (decimal)v.GetDouble();
                                    if (decimal.TryParse(v.GetString(), out var n)) return n;
                                    return def;
                                }

                                long soId = 0;
                                if (headerEl.TryGetProperty("SoId", out var soIdEl))
                                {
                                    soId = soIdEl.ValueKind == JsonValueKind.Number
                                        ? soIdEl.GetInt64()
                                        : (long.TryParse(soIdEl.GetString(), out var t) ? t : 0);
                                }

                                var so = await db.DonBan.FirstOrDefaultAsync(x => x.Id == soId && !x.DaXoa);
                                if (so == null) { Reply(new { ok = false, error = "Không tìm thấy đơn bán." }); break; }

                                var tenKhach = await db.KhachHang
                                    .Where(k => k.Id == so.KhachHangId).Select(k => k.Ten).FirstOrDefaultAsync();

                                if (tpNewEl.ValueKind == JsonValueKind.Undefined)
                                {
                                    Reply(new { ok = false, error = "Thiếu 'tpNew'." }); break;
                                }

                                var tpTen = GetStr(tpNewEl, "Ten")?.Trim();
                                long? dvtId = null;
                                if (tpNewEl.TryGetProperty("DvtId", out var dvtEl))
                                {
                                    dvtId = dvtEl.ValueKind == JsonValueKind.Number ? dvtEl.GetInt64()
                                        : (long.TryParse(dvtEl.GetString(), out var t) ? t : (long?)null);
                                }
                                if (string.IsNullOrWhiteSpace(tpTen) || dvtId is null || dvtId <= 0)
                                {
                                    Reply(new { ok = false, error = "Thiếu Tên TP hoặc ĐVT." }); break;
                                }
                                if (!await db.DonViTinh.AnyAsync(x => x.Id == dvtId.Value))
                                {
                                    Reply(new { ok = false, error = "ĐVT không tồn tại." }); break;
                                }

                                var tpMa = GetStr(tpNewEl, "Ma")?.Trim();
                                if (string.IsNullOrWhiteSpace(tpMa))
                                    tpMa = await GenerateNextTpCodeAsync(db);

                                // ===== Tạo TP mới (chặn NULL ở nguong_ton / is_thanh_pham)
                                var tp = new VatTu
                                {
                                    Ma = tpMa!,
                                    Ten = tpTen!,
                                    DonViTinhId = dvtId.Value,
                                    NguongTon = 0m,        // tránh NULL
                                    IsThanhPham = true,    // cờ thành phẩm
                                    DaXoa = false
                                };
                                db.VatTu.Add(tp);
                                await db.SaveChangesAsync(); // có tp.Id

                                // ===== Tạo MO
                                var mo = new LenhSanXuat
                                {
                                    Ma = await GenerateNextMoCodeAsync(db),
                                    NgayLenh = GetDateN(headerEl, "NgayLenh") ?? DateTime.Today,
                                    NgayIn = GetDateN(headerEl, "NgayIn"),

                                    // ⬇⬇⬇ CHỈNH Ở ĐÂY
                                    TrangThai = "pending",

                                    TenKhachHang = !string.IsNullOrWhiteSpace(GetStr(headerEl, "TenKhachHang"))
                                                   ? GetStr(headerEl, "TenKhachHang")
                                                   : (tenKhach ?? ""),
                                    TenBaiIn = GetStr(headerEl, "TenBaiIn") ?? "",
                                    TenGiayIn = GetStr(headerEl, "TenGiayIn") ?? "",
                                    KhoIn = GetStr(headerEl, "KhoIn"),
                                    SoMauIn = GetInt(headerEl, "SoMauIn"),
                                    HinhThucIn = GetStr(headerEl, "HinhThucIn"),
                                    SoCon = GetInt(headerEl, "SoCon"),
                                    MayIn = GetStr(headerEl, "MayIn"),
                                    SoLuongKeHoach = GetDec(headerEl, "SoLuongKeHoach"),
                                    SoLuongThucTe = null,
                                    SoLuongThanhPham = 0,
                                    GhiChu = $"SO#{so.Id}" + (string.IsNullOrWhiteSpace(GetStr(headerEl, "Note")) ? "" : $" | {GetStr(headerEl, "Note")}"),
                                    DaXoa = false,
                                    SanPhamId = tp.Id
                                };
                                if (mo.SoLuongKeHoach <= 0) { Reply(new { ok = false, error = "SL kế hoạch phải > 0." }); break; }

                                db.LenhSanXuat.Add(mo);
                                await db.SaveChangesAsync(); // có mo.Id

                                // ===== Thêm các dòng định mức (XUẤT NVL kế hoạch)
                                if (dinhMucEl.ValueKind == JsonValueKind.Array)
                                {
                                    foreach (var dm in dinhMucEl.EnumerateArray())
                                    {
                                        long vtId = 0;
                                        if (dm.TryGetProperty("VatTuId", out var vte))
                                        {
                                            vtId = vte.ValueKind == JsonValueKind.Number ? vte.GetInt64()
                                                : (long.TryParse(vte.GetString(), out var t) ? t : 0);
                                        }
                                        var heSo = dm.TryGetProperty("HeSo", out var hse) && hse.ValueKind == JsonValueKind.Number
                                                   ? (decimal)hse.GetDouble() : 0m;
                                        var ghiChu = dm.TryGetProperty("GhiChu", out var gce) ? (gce.GetString() ?? "") : "";

                                        if (vtId <= 0 || heSo <= 0) continue;
                                        if (!await db.VatTu.AnyAsync(x => x.Id == vtId))
                                        {
                                            await tx.RollbackAsync();
                                            Reply(new { ok = false, error = $"Vật tư #{vtId} không tồn tại." });
                                            break;
                                        }

                                        // >>> CHẶN NULL: SoLuong/DonGia/GiaTri ép về 0 ngay từ đầu
                                        db.LenhSanXuatDong.Add(new LenhSanXuatDong
                                        {
                                            LenhId = mo.Id,
                                            VatTuId = vtId,
                                            LoaiDong = "xuat",
                                            HeSo = heSo,
                                            SoLuong = 0m,          // kế hoạch -> để 0, khi duyệt sẽ xuất theo nhu cầu
                                            DonGia = 0m,          // TRÁNH NULL cho cột NOT NULL
                                            GiaTri = 0m,
                                            GhiChu = ghiChu,
                                            DaXoa = false
                                        });
                                    }
                                    await db.SaveChangesAsync();
                                }

                                // ===== Cập nhật trạng thái đơn bán
                                so.TrangThai = "in_production";
                                so.NgayCapNhat = DateTime.Now;
                                await db.SaveChangesAsync();

                                await tx.CommitAsync();
                                Reply(new { ok = true, moId = mo.Id, moCode = mo.Ma, tpId = tp.Id, tpMa = tp.Ma });
                            }
                            catch (DbUpdateException ex)
                            {
                                var baseMsg = ex.InnerException?.Message ?? ex.GetBaseException().Message ?? ex.Message;
                                try { await tx.RollbackAsync(); } catch { }
                                Reply(new { ok = false, error = "DB: " + baseMsg });
                            }
                            catch (Exception ex)
                            {
                                try { await tx.RollbackAsync(); } catch { }
                                Reply(new { ok = false, error = ex.Message });
                            }
                            break;
                        }







                    default:
                        Reply(new { error = $"Unknown cmd: {cmd}" });
                        break;
                }

            }
            catch (Exception ex)
            {
                Reply(new { error = ex.ToString() });
            }

        }
        // using Microsoft.EntityFrameworkCore;  // nhớ có using này ở đầu file
        private static async Task<string> GenerateNextPxCodeAsync(AccountingDbContext db, DateTime? when = null)
        {
            var dt = when ?? DateTime.Now;
            var ymd = dt.ToString("yyyyMMdd");
            var prefix = $"PX-{ymd}-";

            var max = await db.PhieuXuat
                .AsNoTracking()
                .Where(x => x.SoCt.StartsWith(prefix))
                .Select(x => x.SoCt)
                .OrderByDescending(x => x)
                .FirstOrDefaultAsync();

            var next = 1;
            if (!string.IsNullOrEmpty(max))
            {
                var tail = max.Substring(prefix.Length);
                if (int.TryParse(tail, out var n)) next = n + 1;
            }
            return $"{prefix}{next:D3}";
        }





        private static async Task<string> GenerateNextTpCodeAsync(AccountingDbContext db, DateTime? when = null)
        {
            var dt = when ?? DateTime.Now;
            var ymd = dt.ToString("yyyyMMdd");
            var prefix = $"TP-{ymd}-";

            // Nếu entity VatTu có cờ IsThanhPham -> lọc cho chắc
            var query = db.VatTu.AsNoTracking().Where(v => v.Ma.StartsWith(prefix));
            // Nếu bạn có thuộc tính IsThanhPham (bool) thì mở dòng sau:
            // query = query.Where(v => v.IsThanhPham);

            var maxCode = await query
                .Select(v => v.Ma)
                .OrderByDescending(m => m)
                .FirstOrDefaultAsync();

            int next = 1;
            if (!string.IsNullOrEmpty(maxCode))
            {
                var tail = maxCode.Substring(prefix.Length);
                if (int.TryParse(tail, out var n)) next = n + 1;
            }

            return $"{prefix}{next:D3}";
        }

        private static async Task<string> GenerateNextSoCtAsync(AccountingDbContext db, string prefix)
        {
            var today = DateTime.Today;
            var yymm = $"{today:yyMM}";
            var start = $"{prefix}-{yymm}-";
            var last = await db.DonBan
                .Where(x => x.SoCt.StartsWith(start))
                .OrderByDescending(x => x.SoCt)
                .Select(x => x.SoCt)
                .FirstOrDefaultAsync();

            var seq = 1;
            if (!string.IsNullOrEmpty(last))
            {
                var tail = last.Split('-').LastOrDefault();
                if (int.TryParse(tail, out var n)) seq = n + 1;
            }
            return $"{prefix}-{yymm}-{seq:0000}";
        }
        static async Task<string> GenerateNextMoCodeAsync(AccountingDbContext db)
        {
            var prefix = $"MO-{DateTime.Now:yyMM}-";
            var last = await db.LenhSanXuat
                .Where(x => !x.DaXoa && x.Ma.StartsWith(prefix))
                .OrderByDescending(x => x.Ma)
                .Select(x => x.Ma)
                .FirstOrDefaultAsync();
            var seq = 1;
            if (!string.IsNullOrEmpty(last) && int.TryParse(last[^4..], out var n)) seq = n + 1;
            return $"{prefix}{seq:D4}";
        }

        static string? ResolveNextStatus(string current, string action)
        {
            var map = new Dictionary<(string from, string action), string>(new CaseInsensitiveTupleComparer())
    {
        // tạo báo giá từ nháp
        { ("draft", "make_quote"),       "quote" },
        // khách xác nhận
        { ("quote", "customer_confirm"), "confirmed" },
        // quản lý duyệt
        { ("confirmed", "approve"),      "approved" },
        // vào sản xuất
        { ("approved", "start_production"), "in_production" },
        // xuất kho thành phẩm
        { ("in_production", "ship"),     "shipped" },
        // lập hóa đơn
        { ("shipped", "invoice"),        "invoiced" },
        // thu tiền
        { ("invoiced", "pay"),           "paid" },

        // hủy trước khi SX
        { ("draft", "cancel"),           "canceled" },
        { ("quote", "cancel"),           "canceled" },
        { ("confirmed", "cancel"),       "canceled" },
        { ("approved", "cancel"),        "canceled" },
    };
            return map.TryGetValue((current, action), out var next) ? next : null;
        }


        class CaseInsensitiveTupleComparer : IEqualityComparer<(string from, string action)>
        {
            public bool Equals((string from, string action) x, (string from, string action) y)
            {
                return string.Equals(x.from, y.from, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(x.action, y.action, StringComparison.OrdinalIgnoreCase);
            }

            public int GetHashCode((string from, string action) obj)
            {
                int h1 = obj.from?.ToLowerInvariant().GetHashCode() ?? 0;
                int h2 = obj.action?.ToLowerInvariant().GetHashCode() ?? 0;
                return HashCode.Combine(h1, h2);
            }
        }
        // ===================== Helper gửi dữ liệu về JS =====================
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

        // ===================== Helpers cho NCC (mới thêm) =====================
        private record CreateNccDto(
            string? Ma,
            string Ten,
            string? MaSoThue,
            string? DiaChi,
            string? SoDienThoai,
            string? Email
        );

        private static string ToCode(string? s, string fallback = "NCC")
        {
            s = (s ?? "").Trim();
            if (string.IsNullOrEmpty(s)) s = fallback;

            // bỏ dấu
            var norm = s.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var ch in norm)
                if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            var noAccent = sb.ToString().Normalize(NormalizationForm.FormC);

            // chỉ giữ [A-Za-z0-9_] và gộp dấu cách thành _
            var code = Regex.Replace(noAccent, @"[^A-Za-z0-9]+", "_").Trim('_');
            if (string.IsNullOrEmpty(code)) code = fallback;
            return code;
        }

        private async Task<string> EnsureUniqueNccCodeAsync(string code)
        {
            var baseCode = code;
            int i = 1;
            while (await _db.NhaCungCap.AnyAsync(x => x.Ma == code))
                code = $"{baseCode}_{++i}";
            return code;
        }

        // ===================== NHÁP PHIẾU NHẬP: DTO & Helpers =====================

        // DTO nháp PN (đặt tạm trong Form1 cho nhanh)
        private sealed record GrnDraftDto(
            long PoId,
            long SupplierId,
            string SupplierName,
            string SuggestedPnCode,
            DateTime Now,
            System.Collections.Generic.List<GrnDraftLine> Lines
        );

        private sealed record GrnDraftLine(
            long ItemId,
            string ItemCode,
            string ItemName,
            string Uom,
            decimal OrderedQty,
            decimal ReceivedQty,
            decimal RemainingQty,
            decimal Qty,        // mặc định = RemainingQty
            decimal UnitCost,
            decimal TaxRate
        );
        private sealed record CreatePnLineDto(long ItemId, decimal Qty, decimal UnitCost);
        private sealed record CreatePnDto(long PoId, string PnCode, DateTime ReceiptDate, string? Source, System.Collections.Generic.List<CreatePnLineDto> Lines);
        // Sinh mã PN demo (nên thay bằng numbering riêng sau)
        private sealed record ApInvoiceLineDto(long ItemId, decimal Qty, decimal UnitPrice, long? ThueSuatId);
        private sealed record ApInvoiceCreateDto(
            long PoId,
            string SoCt,
            DateTime NgayHoaDon,
            DateTime? HanThanhToan,
            System.Collections.Generic.List<ApInvoiceLineDto> Lines
        );
        // ==== HOA DON MUA: DRAFT & CREATE DTOs (đặt trong Form1, cạnh các record khác) ====
        private sealed record HdDraftLine(
            long ItemId, string ItemCode, string ItemName, string Uom,
            decimal Qty, decimal UnitPrice, decimal TaxRate);

        private sealed record HdDraftDto(
            long PoId, long? SupplierId, string SupplierName,
            string SuggestSoHd, DateTime NgayHd, DateTime? HanThanhToan,
            int DieuKhoanDays, string PhuongThucTt,
            System.Collections.Generic.List<HdDraftLine> Lines);


        // Payload FE khi bấm “Lưu hóa đơn”
        private sealed record HoaDonMuaLineDto(long ItemId, decimal Qty, decimal UnitPrice, long? ThueSuatId);

        private sealed record HoaDonMuaCreateDto(
            long PoId,
            string? SoCt,                      // ← cho phép null, sẽ auto-generate
            DateTime NgayHd,
            DateTime? HanThanhToan,
            string? PhuongThucThanhToan,
            string? GhiChu,
            System.Collections.Generic.List<HoaDonMuaLineDto> Lines
        );
        // Tạo HĐM từ PO, không đổi trạng thái PO
        private async Task<object> CreateApInvoiceFromPoAsync(ApInvoiceCreateDto dto)
        {
            var po = await _db.DonMua.FirstOrDefaultAsync(x => x.Id == dto.PoId);
            if (po == null) return new { error = "po_not_found" };

            // 1 PO 1 HĐ (nếu muốn nhiều, bỏ chặn này)
            if (await _db.HoaDonMua.AnyAsync(x => x.DonMuaId == dto.PoId))
                return new { error = "invoice_exists" };

            if (dto.Lines == null || dto.Lines.Count == 0) return new { error = "no_lines" };

            // map thuế suất %
            var taxMap = await _db.ThueSuat.ToDictionaryAsync(x => x.Id, x => (decimal?)x.TyLe);

            decimal tienHang = 0m, tienThue = 0m;
            var entity = new Accounting.Domain.Entities.HoaDonMua
            {
                SoCt = dto.SoCt,
                DonMuaId = po.Id,
                NhaCungCapId = po.NhaCungCapId,
                NgayHoaDon = dto.NgayHoaDon,
                HanThanhToan = dto.HanThanhToan,
                TrangThai = "draft",
                NgayTao = DateTime.Now,
                NguoiTao = Environment.UserName
            };

            entity.Dong = dto.Lines.Select(l =>
            {
                var lineAmount = l.Qty * l.UnitPrice;
                tienHang += lineAmount;

                decimal rate = 0m;
                if (l.ThueSuatId.HasValue && taxMap.TryGetValue(l.ThueSuatId.Value, out var r) && r.HasValue)
                    rate = r.Value; // ví dụ 10 (%)

                var lineTax = Math.Round(lineAmount * rate / 100m, 0, MidpointRounding.AwayFromZero);
                tienThue += lineTax;

                return new Accounting.Domain.Entities.HoaDonMuaDong
                {
                    VatTuId = l.ItemId,
                    SoLuong = l.Qty,
                    DonGia = l.UnitPrice,
                    ThueSuatId = l.ThueSuatId,
                    TienThue = lineTax,
                    ThanhTien = lineAmount + lineTax
                };
            }).ToList();

            entity.TienHang = tienHang;
            entity.TienThue = tienThue;
            entity.TongTien = tienHang + tienThue;

            _db.HoaDonMua.Add(entity);
            await _db.SaveChangesAsync();

            return new { ok = true, hdId = entity.Id };
        }

        private async Task<string> GenerateNextPnCodeAsync()
        {
            var ymd = DateTime.Now.ToString("yyyyMMdd");
            var count = 0;
            try { count = await _db.PhieuNhap.CountAsync(); } catch { }
            return $"PN-{ymd}-{(count + 1):D4}";
        }
        // Sinh số HĐ tự động: HD-YYYYMMDD-001, 002, ...

        private static Task<string> GenerateNextHdCodeAsync(AccountingDbContext db)
            => GenerateNextHdCodeAsync(db, null);

        private static async Task<string> GenerateNextHdCodeAsync(AccountingDbContext db, DateTime? when)
        {
            var dt = when ?? DateTime.Now;
            var ymd = dt.ToString("yyyyMMdd");
            var prefix = $"HDB-{ymd}-";

            var max = await db.HoaDonBan
                .AsNoTracking()
                .Where(x => x.SoHoaDon.StartsWith(prefix))
                .Select(x => x.SoHoaDon)
                .OrderByDescending(x => x)
                .FirstOrDefaultAsync();

            var next = 1;
            if (!string.IsNullOrEmpty(max))
            {
                var tail = max.Substring(prefix.Length);
                if (int.TryParse(tail, out var n)) next = n + 1;
            }
            return $"{prefix}{next:D3}";
        }

        private async Task<string> GenerateNextSoHdAsync()
        {
            var ymd = DateTime.Now.ToString("yyyyMMdd");
            var cnt = 0;
            try { cnt = await _db.HoaDonMua.CountAsync(); } catch { }
            return $"HD-{ymd}-{(cnt + 1):D4}";
        }
        private async Task<HdDraftDto> GetHdDraftFromPoAsync(long poId)
        {
            var po = await _db.DonMua.Where(p => p.Id == poId).Select(p => new {
                p.Id,
                p.NhaCungCapId,
                SupplierName = _db.NhaCungCap.Where(n => n.Id == p.NhaCungCapId).Select(n => n.Ten).FirstOrDefault(),
                Lines = _db.DonMuaDong.Where(l => l.DonMuaId == p.Id).Select(l => new {
                    l.VatTuId,
                    l.SoLuong,
                    l.DonGia,
                    l.ThueSuatId,
                    ItemCode = _db.VatTu.Where(v => v.Id == l.VatTuId).Select(v => v.Ma).FirstOrDefault(),
                    ItemName = _db.VatTu.Where(v => v.Id == l.VatTuId).Select(v => v.Ten).FirstOrDefault(),
                    Uom = _db.DonViTinh.Where(t => t.Id ==
                        _db.VatTu.Where(v => v.Id == l.VatTuId).Select(v => v.DonViTinhId).FirstOrDefault())
                        .Select(t => t.Ten).FirstOrDefault(),
                    TaxRate = _db.ThueSuat.Where(t => t.Id == l.ThueSuatId).Select(t => t.TyLe).FirstOrDefault()
                }).ToList()
            }).FirstOrDefaultAsync();

            if (po == null) throw new InvalidOperationException("Không tìm thấy đơn mua.");

            // Đã nhận theo VT
            var received = await _db.PhieuNhap.Where(pn => pn.DonMuaId == po.Id)
                .SelectMany(pn => pn.Dong).Where(d => d.VatTuId != null)
                .GroupBy(d => d.VatTuId!.Value)
                .Select(g => new { ItemId = g.Key, Qty = g.Sum(x => x.SoLuong) })
                .ToDictionaryAsync(x => x.ItemId, x => x.Qty);

            // Đã xuất HĐ theo VT
            var invoiced = await _db.HoaDonMua.Where(h => h.DonMuaId == po.Id)
                .SelectMany(h => h.Dong).Where(d => d.VatTuId != null)
                .GroupBy(d => d.VatTuId!.Value)
                .Select(g => new { ItemId = g.Key, Qty = g.Sum(x => x.SoLuong) })
                .ToDictionaryAsync(x => x.ItemId, x => x.Qty);

            var lines = new System.Collections.Generic.List<HdDraftLine>();
            foreach (var l in po.Lines.Where(x => x.VatTuId != null))
            {
                var vt = l.VatTuId!.Value;
                var ordered = l.SoLuong;
                var got = received.TryGetValue(vt, out var rc) ? rc : 0m;
                var inv = invoiced.TryGetValue(vt, out var ic) ? ic : 0m;

                var can = Math.Min(ordered, got) - inv;   // còn có thể lập hóa đơn
                if (can <= 0) continue;

                lines.Add(new HdDraftLine(vt, l.ItemCode ?? "", l.ItemName ?? "", l.Uom ?? "",
                                          can, l.DonGia, l.TaxRate));
            }
            await using var db = new AccountingDbContext(_dbOptions);
            var so = await GenerateNextHdCodeAsync(db);
            var ngay = DateTime.Now.Date;
            var dkDays = 30;

            return new HdDraftDto(
                PoId: po.Id,
                SupplierId: po.NhaCungCapId,
                SupplierName: po.SupplierName ?? "",
                SuggestSoHd: await GenerateNextApInvoiceCodeAsync(ngay),   // <— dùng hàm mới
                NgayHd: ngay,
                HanThanhToan: ngay.AddDays(dkDays),
                DieuKhoanDays: dkDays,
                PhuongThucTt: "cong_no",
                Lines: lines
            );
        }
        private async Task<object> CreateHoaDonMuaAsync(HoaDonMuaCreateDto dto)
        {
            var po = await _db.DonMua.FirstOrDefaultAsync(x => x.Id == dto.PoId);
            if (po == null) return new { error = "po_not_found" };

            if (dto.Lines == null || dto.Lines.Count == 0)
                return new { error = "no_lines" };

            // Bảo đảm có số HĐ
            var soCt = string.IsNullOrWhiteSpace(dto.SoCt)
                ? await GenerateNextSoHdAsync()
                : dto.SoCt.Trim();

            // map thuế
            var taxMap = await _db.ThueSuat.ToDictionaryAsync(x => x.Id, x => (decimal?)x.TyLe);

            decimal tienHang = 0m, tienThue = 0m;

            var entity = new Accounting.Domain.Entities.HoaDonMua
            {
                SoCt = soCt,                                  // ← QUAN TRỌNG
                DonMuaId = po.Id,
                NhaCungCapId = po.NhaCungCapId,
                NgayHoaDon = dto.NgayHd,
                HanThanhToan = dto.HanThanhToan,
                TrangThai = "draft",
                NgayTao = DateTime.Now,
                NguoiTao = Environment.UserName,
            };

            entity.Dong = dto.Lines.Select(l =>
            {
                var lineAmt = l.Qty * l.UnitPrice;
                tienHang += lineAmt;

                decimal rate = 0m;
                if (l.ThueSuatId.HasValue && taxMap.TryGetValue(l.ThueSuatId.Value, out var r) && r.HasValue)
                    rate = r.Value;

                var lineTax = Math.Round(lineAmt * rate / 100m, 0, MidpointRounding.AwayFromZero);
                tienThue += lineTax;

                return new Accounting.Domain.Entities.HoaDonMuaDong
                {
                    VatTuId = l.ItemId,
                    SoLuong = l.Qty,
                    DonGia = l.UnitPrice,
                    ThueSuatId = l.ThueSuatId,
                    TienThue = lineTax,
                    ThanhTien = lineAmt + lineTax
                };
            }).ToList();

            entity.TienHang = tienHang;
            entity.TienThue = tienThue;
            entity.TongTien = tienHang + tienThue;

            _db.HoaDonMua.Add(entity);
            await _db.SaveChangesAsync();

            return new { ok = true, hdId = entity.Id, soCt = entity.SoCt };
        }





        // Lấy bản nháp PN từ PO (Phương án A: Remaining theo Vật tư)
        private async Task<GrnDraftDto> GetGrnDraftFromPoAsync(long poId)
        {
            // 1) Lấy PO header + lines
            var po = await _db.DonMua
                .Where(p => p.Id == poId)
                .Select(p => new
                {
                    p.Id,
                    p.NhaCungCapId,
                    SupplierName = _db.NhaCungCap.Where(n => n.Id == p.NhaCungCapId).Select(n => n.Ten).FirstOrDefault(),
                    Lines = _db.DonMuaDong
                        .Where(l => l.DonMuaId == p.Id)
                        .Select(l => new
                        {
                            l.VatTuId,
                            ItemCode = _db.VatTu.Where(v => v.Id == l.VatTuId).Select(v => v.Ma).FirstOrDefault(),
                            ItemName = _db.VatTu.Where(v => v.Id == l.VatTuId).Select(v => v.Ten).FirstOrDefault(),
                            Uom = _db.DonViTinh.Where(d => d.Id == _db.VatTu.Where(v => v.Id == l.VatTuId).Select(v => v.DonViTinhId).FirstOrDefault()).Select(d => d.Ten).FirstOrDefault(),
                            OrderedQty = l.SoLuong,
                            UnitPrice = l.DonGia,
                            TaxRate = _db.ThueSuat.Where(t => t.Id == l.ThueSuatId).Select(t => t.TyLe).FirstOrDefault()
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (po == null) throw new InvalidOperationException("Không tìm thấy đơn mua.");

            // 2) Tổng đã nhập theo Vật tư (mọi PN thuộc PO này)
            var receivedByItem = await _db.PhieuNhap
                .Where(pn => pn.DonMuaId == po.Id)
                .SelectMany(pn => pn.Dong)
                .GroupBy(d => d.VatTuId)
                .Select(g => new { ItemId = g.Key, Qty = g.Sum(x => x.SoLuong) })
                .ToDictionaryAsync(x => x.ItemId, x => x.Qty);

            // 3) Gom theo vật tư (nếu 1 VT xuất hiện nhiều dòng trong PO)
            var lines = po.Lines
                .Where(l => l.VatTuId != null)
                .GroupBy(l => new {
                    VatTuId = l.VatTuId!.Value,   // <-- ép về long, không còn nullable
                    l.ItemCode,
                    l.ItemName,
                    l.Uom,
                    l.UnitPrice,
                    l.TaxRate
                })
                .Select(g =>
                {
                    var ordered = g.Sum(x => x.OrderedQty);
                    decimal received = 0m;
                    receivedByItem.TryGetValue(g.Key.VatTuId, out received); // key: long
                    var remaining = ordered - received;

                    return new GrnDraftLine(
                        ItemId: g.Key.VatTuId,                    // giờ là long
                        ItemCode: g.Key.ItemCode ?? "",
                        ItemName: g.Key.ItemName ?? "",
                        Uom: g.Key.Uom ?? "",
                        OrderedQty: ordered,
                        ReceivedQty: received,
                        RemainingQty: remaining,
                        Qty: remaining > 0 ? remaining : 0m,
                        UnitCost: g.Key.UnitPrice,
                        TaxRate: g.Key.TaxRate
                    );
                })
                .Where(x => x.RemainingQty > 0)
                .OrderBy(x => x.ItemName)
                .ToList();


            var code = await GenerateNextPnCodeAsync();

            return new GrnDraftDto(
                PoId: po.Id,
                SupplierId: po.NhaCungCapId,
                SupplierName: po.SupplierName ?? "",
                SuggestedPnCode: code,
                Now: DateTime.Now,
                Lines: lines
            );
        }
        // ====== Ghi sổ / cập nhật trạng thái Đơn mua ======
        // ====== Ghi sổ Đơn mua: chỉ 1 lần, từ draft/nhap -> approved ======
        private async Task<string> GhiSoDonMuaAsync(long poId)
        {
            var po = await _db.DonMua.FirstOrDefaultAsync(x => x.Id == poId);
            if (po == null) throw new InvalidOperationException("Không tìm thấy đơn mua.");

            var cur = (po.TrangThai ?? "").ToLowerInvariant();
            if (cur == "" || cur == "nhap" || cur == "draft")
            {
                po.TrangThai = "approved";
                await _db.SaveChangesAsync();
            }
            // nếu đã approved/partial/received thì giữ nguyên, không đổi
            return po.TrangThai ?? "approved";
        }

        // ====== Tạo Phiếu nhập từ PO & cập nhật trạng thái PO ======
        // ====== Tạo Phiếu nhập từ PO & cập nhật trạng thái PO ======
        private async Task<object> CreatePhieuNhapFromPoAsync(CreatePnDto dto)
        {
            // Tìm PO & kiểm tra đã duyệt chưa
            var po = await _db.DonMua.FirstOrDefaultAsync(x => x.Id == dto.PoId);
            if (po == null) return new { error = "po_not_found" };

            var st = (po.TrangThai ?? "").ToLowerInvariant();
            if (st is "draft" or "nhap")
                return new { error = "po_not_approved", message = "Đơn chưa duyệt, không được nhập kho." };

            if (dto.Lines == null || dto.Lines.Count == 0) return new { error = "no_lines" };
            if (dto.Lines.Any(l => l.Qty <= 0)) return new { error = "invalid_qty" };

            // Không cho vượt remaining
            var orderedByItem = await _db.DonMuaDong
                .Where(l => l.DonMuaId == dto.PoId && l.VatTuId != null)
                .GroupBy(l => l.VatTuId!.Value)
                .Select(g => new { ItemId = g.Key, Qty = g.Sum(x => x.SoLuong) })
                .ToDictionaryAsync(x => x.ItemId, x => x.Qty);

            var receivedByItem = await _db.PhieuNhap
                .Where(pn => pn.DonMuaId == dto.PoId)
                .SelectMany(pn => pn.Dong)
                .Where(d => d.VatTuId != null)
                .GroupBy(d => d.VatTuId!.Value)
                .Select(g => new { ItemId = g.Key, Qty = g.Sum(x => x.SoLuong) })
                .ToDictionaryAsync(x => x.ItemId, x => x.Qty);

            foreach (var ln in dto.Lines)
            {
                var ordered = orderedByItem.TryGetValue(ln.ItemId, out var oq) ? oq : 0m;
                var received = receivedByItem.TryGetValue(ln.ItemId, out var rq) ? rq : 0m;
                var remaining = Math.Max(0, ordered - received);
                if (ln.Qty > remaining)
                    return new { error = "qty_exceeds_remaining", itemId = ln.ItemId, remaining };
            }

            // ===== LƯU PHIẾU NHẬP (mapping theo entity của bạn) =====
            var entity = new Accounting.Domain.Entities.PhieuNhap
            {
                SoCt = dto.PnCode,
                DonMuaId = dto.PoId,
                NhaCungCapId = po.NhaCungCapId,          // map từ PO
                NgayNhap = dto.ReceiptDate,
                GhiChu = dto.Source,               // tạm dùng Source làm ghi chú
                NgayTao = DateTime.Now,
                NguoiTao = Environment.UserName
                // KhoId: nếu có chọn kho ở UI thì set thêm
            };

            entity.Dong = dto.Lines.Select(l =>
            {
                var qty = l.Qty;
                var cost = l.UnitCost;
                return new Accounting.Domain.Entities.PhieuNhapDong
                {
                    VatTuId = l.ItemId,
                    SoLuong = qty,
                    DonGia = cost,
                    GiaTri = qty * cost   // ★ BẮT BUỘC: cột NOT NULL trong DB
                                          // Thêm gì khác nếu entity có, ví dụ: ThueSuatId, ThanhTien, ...
                };
            }).ToList();

            _db.PhieuNhap.Add(entity);
            await _db.SaveChangesAsync();

            // Cập nhật trạng thái PO: partial/received
            var status = await UpdatePoStatusFromReceiptsAsync(dto.PoId);

            return new { ok = true, pnId = entity.Id, status };
        }

        private async Task<string> GenerateNextProductCodeAsync(AccountingDbContext db)
        {
            // Mã TP: TP-000001, TP-000002...
            var last = await db.VatTu
                .AsNoTracking()
                .Where(v => v.Ma.StartsWith("TP-"))
                .OrderByDescending(v => v.Ma)
                .Select(v => v.Ma)
                .FirstOrDefaultAsync();

            var next = 1;
            if (!string.IsNullOrEmpty(last))
            {
                var m = System.Text.RegularExpressions.Regex.Match(last, @"TP-(\d+)$");
                if (m.Success && int.TryParse(m.Groups[1].Value, out var seq)) next = seq + 1;
            }
            return $"TP-{next:D6}";
        }


        // ====== Tính trạng thái từ các PN: partial / received ======
        private async Task<string> UpdatePoStatusFromReceiptsAsync(long poId)
        {
            var po = await _db.DonMua.FirstOrDefaultAsync(x => x.Id == poId);
            if (po == null) throw new InvalidOperationException("Không tìm thấy đơn mua.");

            var ordered = await _db.DonMuaDong
                .Where(l => l.DonMuaId == poId && l.VatTuId != null)
                .GroupBy(l => l.VatTuId!.Value)
                .Select(g => new { ItemId = g.Key, Qty = g.Sum(x => x.SoLuong) })
                .ToDictionaryAsync(x => x.ItemId, x => x.Qty);

            var received = await _db.PhieuNhap
                .Where(pn => pn.DonMuaId == poId)
                .SelectMany(pn => pn.Dong)
                .Where(d => d.VatTuId != null)
                .GroupBy(d => d.VatTuId!.Value)
                .Select(g => new { ItemId = g.Key, Qty = g.Sum(x => x.SoLuong) })
                .ToDictionaryAsync(x => x.ItemId, x => x.Qty);

            if (ordered.Count == 0)
            {
                po.TrangThai = "approved";    // không có dòng đặt → vẫn coi là đã duyệt
                await _db.SaveChangesAsync();
                return po.TrangThai;
            }

            bool allReceived = true;
            foreach (var (itemId, qtyOrdered) in ordered)
            {
                var got = received.TryGetValue(itemId, out var rc) ? rc : 0m;
                if (got < qtyOrdered) { allReceived = false; break; }
            }

            po.TrangThai = allReceived ? "received" : "partial";
            await _db.SaveChangesAsync();
            return po.TrangThai;
        }

    }
}
