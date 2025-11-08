using System;
using Microsoft.EntityFrameworkCore;
using Accounting.Infrastructure;
using Accounting.Application.Services;
using Accounting.Domain.Entities;

// alias rõ ràng để tránh trùng namespace
using WinFormsApp = System.Windows.Forms.Application;

namespace Accounting.App
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

            // === KẾT NỐI THỐNG NHẤT CHO TOÀN APP ===
            var options = new DbContextOptionsBuilder<AccountingDbContext>()
                .UseSqlServer(@"Server=localhost\SQLEXPRESS;Database=AccountingDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true")
                .Options;

            // Tạo DB nếu chưa có + đảm bảo admin tồn tại / reset mật khẩu
            try
            {
                using var db = new AccountingDbContext(options);
                db.Database.EnsureCreated();
                var auth = new AuthService(db);
                auth.UpsertAdminAsync("admin", "Administrator", "Admin@123").Wait();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    "Lỗi DB khởi tạo/seed admin:\n" + ex.Message,
                    "Database Error",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
                return;
            }

            // Mở LoginForm (WebView2 + HTML)
            var login = new LoginForm(options);
            System.Windows.Forms.Application.Run(login);
        }
    }
}
