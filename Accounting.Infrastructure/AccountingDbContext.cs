using Accounting.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Accounting.Infrastructure
{
    /// <summary>
    /// DbContext cho hệ thống Kế toán In ấn Bao bì (schema: acc)
    /// </summary>
    public class AccountingDbContext : DbContext
    {
        public const string Schema = "acc";

        // DI/options
        public AccountingDbContext(DbContextOptions<AccountingDbContext> options) : base(options) { }
        // Khởi tạo mặc định (Form1 có thể new trực tiếp)
        public AccountingDbContext() { }

        // ===== DbSet Danh mục =====
        public DbSet<KhachHang> KhachHang => Set<KhachHang>();
        public DbSet<NhaCungCap> NhaCungCap => Set<NhaCungCap>();
        public DbSet<DonViTinh> DonViTinh => Set<DonViTinh>();
        public DbSet<VatTu> VatTu => Set<VatTu>();
        public DbSet<Kho> Kho => Set<Kho>();
        public DbSet<TaiKhoanNganHang> TaiKhoanNganHang => Set<TaiKhoanNganHang>();
        public DbSet<ThueSuat> ThueSuat => Set<ThueSuat>();

        public DbSet<NguoiDung> NguoiDung { get; set; } = default!;
        // ===== Nghiệp vụ Mua hàng =====
        public DbSet<DonMua> DonMua => Set<DonMua>();
        public DbSet<DonMuaDong> DonMuaDong => Set<DonMuaDong>();
        public DbSet<PhieuNhap> PhieuNhap => Set<PhieuNhap>();
        public DbSet<PhieuNhapDong> PhieuNhapDong => Set<PhieuNhapDong>();

        public DbSet<PhieuXuat> PhieuXuat => Set<PhieuXuat>();
        public DbSet<PhieuXuatDong> PhieuXuatDong => Set<PhieuXuatDong>();
        public DbSet<HoaDonMua> HoaDonMua => Set<HoaDonMua>();
        public DbSet<HoaDonMuaDong> HoaDonMuaDong => Set<HoaDonMuaDong>();

        public DbSet<PhieuThu> PhieuThu => Set<PhieuThu>();
        public DbSet<PhieuChi> PhieuChi => Set<PhieuChi>();
        public DbSet<DonBan> DonBan => Set<DonBan>();
        public DbSet<PhieuThuNganHang> PhieuThuNganHang => Set<PhieuThuNganHang>();
        public DbSet<PhieuChiNganHang> PhieuChiNganHang => Set<PhieuChiNganHang>();
        public DbSet<DonBanDong> DonBanDong => Set<DonBanDong>();
        public DbSet<KiemKeQuy> KiemKeQuy { get; set; } = default!;
        public DbSet<HoaDonBan> HoaDonBan => Set<HoaDonBan>();
        public DbSet<HoaDonBanDong> HoaDonBanDong => Set<HoaDonBanDong>();

        // ===== Nghiệp vụ Sản xuất (đã tối giản) =====
        public DbSet<LenhSanXuat> LenhSanXuat { get; set; } = default!;
        public DbSet<LenhSanXuatDong> LenhSanXuatDong { get; set; } = default!;
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Chỉnh đúng instance SQL của bạn nếu cần
                optionsBuilder.UseSqlServer(
                    "Server=.\\SQLEXPRESS04;Database=AccountingDB;Trusted_Connection=True;TrustServerCertificate=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder b)
        {
            b.HasDefaultSchema(Schema);

            // =========================
            // KHÁCH HÀNG
            // =========================
            b.Entity<KhachHang>(e =>
            {
                e.ToTable("khach_hang");
                e.HasKey(x => x.Id);

                e.Property(x => x.Ma).HasColumnName("ma").HasMaxLength(50).IsRequired();
                e.HasIndex(x => x.Ma).IsUnique();

                e.Property(x => x.Ten).HasColumnName("ten").HasMaxLength(200).IsRequired();
                e.Property(x => x.MaSoThue).HasColumnName("ma_so_thue");
                e.Property(x => x.DiaChi).HasColumnName("dia_chi");
                e.Property(x => x.SoDienThoai).HasColumnName("so_dien_thoai");
                e.Property(x => x.Email).HasColumnName("email");

                e.Property(x => x.NgayTao).HasColumnName("ngay_tao");
                e.Property(x => x.NguoiTao).HasColumnName("nguoi_tao");
                e.Property(x => x.NgayCapNhat).HasColumnName("ngay_cap_nhat");
                e.Property(x => x.NguoiCapNhat).HasColumnName("nguoi_cap_nhat");
                e.Property(x => x.DaXoa).HasColumnName("da_xoa").HasDefaultValue(false);
            });

            // ===== PHIẾU THU NGÂN HÀNG =====
            b.Entity<PhieuThuNganHang>(e =>
            {
                e.ToTable("phieu_thu_ngan_hang");
                e.HasKey(x => x.Id);

                e.Property(x => x.SoCt)
                    .HasColumnName("so_ct")
                    .HasMaxLength(50)
                    .IsRequired();

                e.HasIndex(x => x.SoCt).IsUnique();

                e.Property(x => x.NgayCt)
                    .HasColumnName("ngay_ct");

                e.Property(x => x.TaiKhoanNganHangId)
                    .HasColumnName("tai_khoan_ngan_hang_id");

                e.Property(x => x.NguoiNop)
                    .HasColumnName("nguoi_nop")
                    .HasMaxLength(200)
                    .IsRequired();

                e.Property(x => x.SoTien)
                    .HasColumnName("so_tien")
                    .HasPrecision(18, 2)
                    .HasDefaultValue(0m);

                e.Property(x => x.LyDo)
                    .HasColumnName("ly_do")
                    .HasMaxLength(500);

                e.Property(x => x.HoaDonBanId)
                    .HasColumnName("hoa_don_ban_id");

                e.Property(x => x.NgayTao).HasColumnName("ngay_tao");
                e.Property(x => x.NguoiTao).HasColumnName("nguoi_tao").HasMaxLength(100);
                e.Property(x => x.NgayCapNhat).HasColumnName("ngay_cap_nhat");
                e.Property(x => x.NguoiCapNhat).HasColumnName("nguoi_cap_nhat").HasMaxLength(100);
                e.Property(x => x.DaXoa).HasColumnName("da_xoa").HasDefaultValue(false);
            });
            b.Entity<NguoiDung>(b =>
            {
                b.ToTable("nguoi_dung", "acc");
                b.HasKey(x => x.Id);

                b.Property(x => x.TenDangNhap)
                    .HasColumnName("ten_dang_nhap")
                    .HasMaxLength(50)
                    .IsRequired();

                b.Property(x => x.HoTen)
                    .HasColumnName("ho_ten")
                    .HasMaxLength(200)
                    .IsRequired();

                b.Property(x => x.MatKhauHash)
                    .HasColumnName("mat_khau_hash")
                    .HasMaxLength(256)
                    .IsRequired();

                b.Property(x => x.VaiTro)
                    .HasColumnName("vai_tro")
                    .HasMaxLength(50)
                    .IsRequired();

                b.Property(x => x.DangHoatDong)
                    .HasColumnName("dang_hoat_dong");

                b.Property(x => x.NgayTao).HasColumnName("ngay_tao");
                b.Property(x => x.NguoiTao).HasColumnName("nguoi_tao");
                b.Property(x => x.NgaySua).HasColumnName("ngay_sua");
                b.Property(x => x.NguoiSua).HasColumnName("nguoi_sua");

                b.HasIndex(x => x.TenDangNhap).IsUnique();
            });


            // ===== PHIẾU CHI NGÂN HÀNG =====
            b.Entity<PhieuChiNganHang>(e =>
            {
                e.ToTable("phieu_chi_ngan_hang");
                e.HasKey(x => x.Id);

                e.Property(x => x.SoCt)
                    .HasColumnName("so_ct")
                    .HasMaxLength(50)
                    .IsRequired();

                e.HasIndex(x => x.SoCt).IsUnique();

                e.Property(x => x.NgayCt)
                    .HasColumnName("ngay_ct");

                e.Property(x => x.TaiKhoanNganHangId)
                    .HasColumnName("tai_khoan_ngan_hang_id");

                e.Property(x => x.NguoiNhan)
                    .HasColumnName("nguoi_nhan")
                    .HasMaxLength(200)
                    .IsRequired();

                e.Property(x => x.SoTien)
                    .HasColumnName("so_tien")
                    .HasPrecision(18, 2)
                    .HasDefaultValue(0m);

                e.Property(x => x.LyDo)
                    .HasColumnName("ly_do")
                    .HasMaxLength(500);

                e.Property(x => x.HoaDonMuaId)
                    .HasColumnName("hoa_don_mua_id");

                e.Property(x => x.NgayTao).HasColumnName("ngay_tao");
                e.Property(x => x.NguoiTao).HasColumnName("nguoi_tao").HasMaxLength(100);
                e.Property(x => x.NgayCapNhat).HasColumnName("ngay_cap_nhat");
                e.Property(x => x.NguoiCapNhat).HasColumnName("nguoi_cap_nhat").HasMaxLength(100);
                e.Property(x => x.DaXoa).HasColumnName("da_xoa").HasDefaultValue(false);
            });



            b.Entity<NhaCungCap>(e =>
            {
                e.ToTable("nha_cung_cap");
                e.HasKey(x => x.Id);

                e.Property(x => x.Ma).HasColumnName("ma").HasMaxLength(50).IsRequired();
                e.HasIndex(x => x.Ma).IsUnique();

                e.Property(x => x.Ten).HasColumnName("ten").HasMaxLength(200).IsRequired();
                e.Property(x => x.MaSoThue).HasColumnName("ma_so_thue");
                e.Property(x => x.DiaChi).HasColumnName("dia_chi");
                e.Property(x => x.SoDienThoai).HasColumnName("so_dien_thoai");
                e.Property(x => x.Email).HasColumnName("email");

                e.Property(x => x.NgayTao).HasColumnName("ngay_tao");
                e.Property(x => x.NguoiTao).HasColumnName("nguoi_tao");
                e.Property(x => x.NgayCapNhat).HasColumnName("ngay_cap_nhat");
                e.Property(x => x.NguoiCapNhat).HasColumnName("nguoi_cap_nhat");
                e.Property(x => x.DaXoa).HasColumnName("da_xoa").HasDefaultValue(false);
            });

            // =========================
            // ĐƠN VỊ TÍNH
            // =========================
            b.Entity<DonViTinh>(e =>
            {
                e.ToTable("don_vi_tinh");
                e.HasKey(x => x.Id);

                e.Property(x => x.Ma).HasColumnName("ma").HasMaxLength(20).IsRequired();
                e.HasIndex(x => x.Ma).IsUnique();

                e.Property(x => x.Ten).HasColumnName("ten").HasMaxLength(50).IsRequired();
            });

            // =========================
            // VẬT TƯ (đã tối giản mapping)
            // =========================
            b.Entity<VatTu>(e =>
            {
                e.ToTable("vat_tu");
                e.HasKey(x => x.Id);

                e.Property(x => x.Ma).HasColumnName("ma").HasMaxLength(50).IsRequired();
                e.HasIndex(x => x.Ma).IsUnique();

                e.Property(x => x.Ten).HasColumnName("ten").HasMaxLength(200).IsRequired();

                e.Property(x => x.DonViTinhId).HasColumnName("don_vi_tinh_id");
                e.HasOne<DonViTinh>().WithMany().HasForeignKey(x => x.DonViTinhId);

                e.Property(x => x.DaXoa).HasColumnName("da_xoa").HasDefaultValue(false);
                e.Property(x => x.NguongTon).HasColumnName("nguong_ton");

                e.Property(x => x.IsThanhPham)
  .HasColumnName("is_thanh_pham")
  .HasDefaultValue(false)
  .IsRequired();
            });

            // =========================
            // KHO
            // =========================
            b.Entity<Kho>(e =>
            {
                e.ToTable("kho");
                e.HasKey(x => x.Id);

                e.Property(x => x.Ma).HasColumnName("ma").HasMaxLength(50).IsRequired();
                e.HasIndex(x => x.Ma).IsUnique();

                e.Property(x => x.Ten).HasColumnName("ten").HasMaxLength(200).IsRequired();

                e.Property(x => x.NgayTao).HasColumnName("ngay_tao");
                e.Property(x => x.NguoiTao).HasColumnName("nguoi_tao");
                e.Property(x => x.NgayCapNhat).HasColumnName("ngay_cap_nhat");
                e.Property(x => x.NguoiCapNhat).HasColumnName("nguoi_cap_nhat");
                e.Property(x => x.DaXoa).HasColumnName("da_xoa").HasDefaultValue(false);
            });

            // =========================
            // TÀI KHOẢN NGÂN HÀNG
            // =========================
            b.Entity<TaiKhoanNganHang>(e =>
            {
                e.ToTable("tai_khoan_ngan_hang");
                e.HasKey(x => x.Id);

                e.Property(x => x.Ma).HasColumnName("ma").HasMaxLength(50).IsRequired();
                e.HasIndex(x => x.Ma).IsUnique();

                e.Property(x => x.TenNganHang).HasColumnName("ten_ngan_hang").IsRequired();
                e.Property(x => x.SoTaiKhoan).HasColumnName("so_tai_khoan").IsRequired();
                e.Property(x => x.TienTe).HasColumnName("tien_te").HasMaxLength(10).HasDefaultValue("VND");
            });

            // =========================
            // THUẾ SUẤT
            // =========================
            b.Entity<ThueSuat>(e =>
            {
                e.ToTable("thue_suat");
                e.HasKey(x => x.Id);

                e.Property(x => x.Ten).HasColumnName("ten").IsRequired();
                e.Property(x => x.TyLe).HasColumnName("ty_le").HasPrecision(9, 4).IsRequired();
                e.Property(x => x.DangHoatDong).HasColumnName("dang_hoat_dong").HasDefaultValue(true);
            });

            // ===== ĐƠN MUA =====
            b.Entity<DonMua>(e =>
            {
                e.ToTable("don_mua");
                e.HasKey(x => x.Id);

                e.Property(x => x.SoCt).HasColumnName("so_ct").HasMaxLength(50).IsRequired();
                e.HasIndex(x => x.SoCt).IsUnique();

                e.Property(x => x.NhaCungCapId).HasColumnName("nha_cung_cap_id");
                e.Property(x => x.NgayDon).HasColumnName("ngay_don");
                e.Property(x => x.TienTe).HasColumnName("tien_te");
                e.Property(x => x.TyGia).HasColumnName("ty_gia").HasPrecision(9, 4).HasDefaultValue(1.0000m);
                e.Property(x => x.CoHopDongLon).HasColumnName("co_hop_dong_lon").HasDefaultValue(false);
                e.Property(x => x.GhiChu).HasColumnName("ghi_chu");
                e.Property(x => x.TrangThai).HasColumnName("trang_thai").HasMaxLength(20).HasDefaultValue("nhap");

                // ➜ các tổng tiền: NOT NULL + default 0
                e.Property(x => x.TienHang).HasColumnName("tien_hang").HasPrecision(18, 2).HasDefaultValue(0m);
                e.Property(x => x.TienThue).HasColumnName("tien_thue").HasPrecision(18, 2).HasDefaultValue(0m);
                e.Property(x => x.TongTien).HasColumnName("tong_tien").HasPrecision(18, 2).HasDefaultValue(0m);

                e.HasOne<NhaCungCap>().WithMany().HasForeignKey(x => x.NhaCungCapId);
            });
            // ===== PHIẾU THU =====
            b.Entity<PhieuThu>(e =>
            {
                e.ToTable("phieu_thu");      // mặc định schema "acc" ở trên rồi
                e.HasKey(x => x.Id);

                // Số chứng từ
                e.Property(x => x.SoCt)
                    .HasColumnName("so_ct")
                    .HasMaxLength(50)
                    .IsRequired();
                e.HasIndex(x => x.SoCt).IsUnique();

                // Ngày chứng từ
                e.Property(x => x.NgayCt)
                    .HasColumnName("ngay_ct");

                // Người nộp
                e.Property(x => x.NguoiNop)
                    .HasColumnName("nguoi_nop")
                    .HasMaxLength(200)
                    .IsRequired();

                // Số tiền
                e.Property(x => x.SoTien)
                    .HasColumnName("so_tien")
                    .HasPrecision(18, 2)
                    .HasDefaultValue(0m);

                // Lý do
                e.Property(x => x.LyDo)
                    .HasColumnName("ly_do")
                    .HasMaxLength(500);

                // 🔹 map khóa ngoại tới hóa đơn bán
                e.Property(x => x.HoaDonBanId)
                    .HasColumnName("hoa_don_ban_id");   // đúng tên cột bạn đã ALTER TABLE

                e.HasOne(x => x.HoaDonBan)
                    .WithMany(h => h.PhieuThus)
                    .HasForeignKey(x => x.HoaDonBanId);

                // Audit + soft delete
                e.Property(x => x.NgayTao).HasColumnName("ngay_tao");
                e.Property(x => x.NguoiTao).HasColumnName("nguoi_tao").HasMaxLength(100);
                e.Property(x => x.NgayCapNhat).HasColumnName("ngay_cap_nhat");
                e.Property(x => x.NguoiCapNhat).HasColumnName("nguoi_cap_nhat").HasMaxLength(100);
                e.Property(x => x.DaXoa).HasColumnName("da_xoa").HasDefaultValue(false);
            });


            // ===== PHIẾU CHI =====
            b.Entity<PhieuChi>(e =>
            {
                e.ToTable("phieu_chi");
                e.HasKey(x => x.Id);

                e.Property(x => x.SoCt)
                    .HasColumnName("so_ct")
                    .HasMaxLength(50)
                    .IsRequired();
                e.HasIndex(x => x.SoCt).IsUnique();

                e.Property(x => x.NgayCt).HasColumnName("ngay_ct");

                e.Property(x => x.NguoiNhan)
                    .HasColumnName("nguoi_nhan")
                    .HasMaxLength(200)
                    .IsRequired();

                e.Property(x => x.SoTien)
                    .HasColumnName("so_tien")
                    .HasPrecision(18, 2)
                    .HasDefaultValue(0m);

                e.Property(x => x.LyDo)
                    .HasColumnName("ly_do")
                    .HasMaxLength(500);

                // 🔹 map khóa ngoại tới hóa đơn mua
                e.Property(x => x.HoaDonMuaId)
                    .HasColumnName("hoa_don_mua_id");

                e.HasOne(x => x.HoaDonMua)
                    .WithMany(h => h.PhieuChis)
                    .HasForeignKey(x => x.HoaDonMuaId);

                e.Property(x => x.NgayTao).HasColumnName("ngay_tao");
                e.Property(x => x.NguoiTao).HasColumnName("nguoi_tao").HasMaxLength(100);
                e.Property(x => x.NgayCapNhat).HasColumnName("ngay_cap_nhat");
                e.Property(x => x.NguoiCapNhat).HasColumnName("nguoi_cap_nhat").HasMaxLength(100);
                e.Property(x => x.DaXoa).HasColumnName("da_xoa").HasDefaultValue(false);
            });
            b.Entity<KiemKeQuy>(e =>
            {
                e.ToTable("kiem_ke_quy", "acc");
                e.HasKey(x => x.Id);

                e.Property(x => x.NgayKk).HasColumnName("ngay_kk");
                e.Property(x => x.SoDuSo)
                    .HasColumnName("so_du_so")
                    .HasPrecision(18, 2);
                e.Property(x => x.SoDuThucTe)
                    .HasColumnName("so_du_thuc_te")
                    .HasPrecision(18, 2);
                e.Property(x => x.ChenhLech)
                    .HasColumnName("chenh_lech")
                    .HasPrecision(18, 2);
                e.Property(x => x.GhiChu)
                    .HasColumnName("ghi_chu")
                    .HasMaxLength(500);

                e.Property(x => x.NgayTao).HasColumnName("ngay_tao");
                e.Property(x => x.NguoiTao).HasColumnName("nguoi_tao").HasMaxLength(100);
                e.Property(x => x.NgayCapNhat).HasColumnName("ngay_cap_nhat");
                e.Property(x => x.NguoiCapNhat).HasColumnName("nguoi_cap_nhat").HasMaxLength(100);
                e.Property(x => x.DaXoa).HasColumnName("da_xoa").HasDefaultValue(false);
            });


            // ===== ĐƠN MUA DÒNG =====
            b.Entity<DonMuaDong>(e =>
            {
                e.ToTable("don_mua_dong");
                e.HasKey(x => x.Id);

                e.Property(x => x.DonMuaId).HasColumnName("don_mua_id");
                e.HasOne<DonMua>()
                    .WithMany(d => d.Dong)
                    .HasForeignKey(x => x.DonMuaId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.Property(x => x.VatTuId).HasColumnName("vat_tu_id");
                e.HasOne<VatTu>().WithMany().HasForeignKey(x => x.VatTuId);

                e.Property(x => x.KichThuoc).HasColumnName("kich_thuoc");
                e.Property(x => x.LoaiGiay).HasColumnName("loai_giay");
                e.Property(x => x.DinhLuongGsm).HasColumnName("dinh_luong_gsm");
                e.Property(x => x.MauIn).HasColumnName("mau_in");
                e.Property(x => x.GiaCong).HasColumnName("gia_cong");

                e.Property(x => x.SoLuong).HasColumnName("so_luong").HasPrecision(18, 3);
                e.Property(x => x.DonGia).HasColumnName("don_gia").HasPrecision(18, 2);

                e.Property(x => x.ThueSuatId).HasColumnName("thue_suat_id");
                e.HasOne<ThueSuat>().WithMany().HasForeignKey(x => x.ThueSuatId);

                e.Property(x => x.TienThue).HasColumnName("tien_thue").HasPrecision(18, 2);
                e.Property(x => x.ThanhTien).HasColumnName("thanh_tien").HasPrecision(18, 2);
            });
            // =========================
            // ĐƠN BÁN (job in ấn)
            // =========================
            b.Entity<DonBan>(e =>
            {
                e.ToTable("don_ban");
                e.HasKey(x => x.Id);

                e.Property(x => x.SoCt).HasColumnName("so_ct").HasMaxLength(50).IsRequired();
                e.HasIndex(x => x.SoCt).IsUnique();

                e.Property(x => x.NgayDon).HasColumnName("ngay_don");
                e.Property(x => x.KhachHangId).HasColumnName("khach_hang_id");
                e.Property(x => x.KhoId).HasColumnName("kho_id");
                e.Property(x => x.TrangThai).HasColumnName("trang_thai").HasMaxLength(30).HasDefaultValue("draft");

                e.Property(x => x.TienHang).HasColumnName("tien_hang").HasColumnType("decimal(18,2)");
                e.Property(x => x.TienChietKhau).HasColumnName("tien_ck").HasColumnType("decimal(18,2)");
                e.Property(x => x.TienThue).HasColumnName("tien_thue").HasColumnType("decimal(18,2)");
                e.Property(x => x.TongTien).HasColumnName("tong_tien").HasColumnType("decimal(18,2)");
                e.Property(x => x.GhiChu).HasColumnName("ghi_chu").HasMaxLength(500);

                e.Property(x => x.NgayTao).HasColumnName("ngay_tao");
                e.Property(x => x.NguoiTao).HasColumnName("nguoi_tao");
                e.Property(x => x.NgayCapNhat).HasColumnName("ngay_cap_nhat");
                e.Property(x => x.NguoiCapNhat).HasColumnName("nguoi_cap_nhat");
                e.Property(x => x.DaXoa).HasColumnName("da_xoa").HasDefaultValue(false);

                e.HasMany(x => x.Dongs).WithOne().HasForeignKey(d => d.DonBanId);
            });

            b.Entity<DonBanDong>(e =>
            {
                e.ToTable("don_ban_dong");
                e.HasKey(x => x.Id);

                e.Property(x => x.DonBanId).HasColumnName("don_ban_id");
                e.Property(x => x.SanPhamId).HasColumnName("san_pham_id");
                e.Property(x => x.TenHang).HasColumnName("ten_hang").HasMaxLength(200).IsRequired();
                e.Property(x => x.QuyCach).HasColumnName("quy_cach").HasMaxLength(500);
                e.Property(x => x.SpecJson).HasColumnName("spec_json"); // nvarchar(max)

                e.Property(x => x.SoLuong).HasColumnName("so_luong").HasColumnType("decimal(18,3)");
                e.Property(x => x.DonViTinh).HasColumnName("dvt").HasMaxLength(50);
                e.Property(x => x.DonGia).HasColumnName("don_gia").HasColumnType("decimal(18,2)");

                e.Property(x => x.TienHang).HasColumnName("tien_hang").HasColumnType("decimal(18,2)");
                e.Property(x => x.TienChietKhau).HasColumnName("tien_ck").HasColumnType("decimal(18,2)");
                e.Property(x => x.ThueSuat).HasColumnName("thue_suat").HasColumnType("decimal(9,4)");
                e.Property(x => x.TienThue).HasColumnName("tien_thue").HasColumnType("decimal(18,2)");
                e.Property(x => x.ThanhTien).HasColumnName("thanh_tien").HasColumnType("decimal(18,2)");

                e.Property(x => x.DaXoa).HasColumnName("da_xoa").HasDefaultValue(false);

                e.HasIndex(x => new { x.DonBanId, x.TenHang });
            });
            b.Entity<HoaDonBan>(e =>
            {
                e.ToTable("hoa_don_ban");
                e.HasKey(x => x.Id);

                e.Property(x => x.SoHoaDon).HasColumnName("so_hoa_don").HasMaxLength(50).IsRequired();
                e.HasIndex(x => x.SoHoaDon).IsUnique();

                e.Property(x => x.NgayHoaDon).HasColumnName("ngay_hoa_don");
                e.Property(x => x.DonBanId).HasColumnName("don_ban_id");

                e.Property(x => x.TienHang).HasColumnName("tien_hang").HasColumnType("decimal(18,2)");
                e.Property(x => x.TienThue).HasColumnName("tien_thue").HasColumnType("decimal(18,2)");
                e.Property(x => x.TongTien).HasColumnName("tong_tien").HasColumnType("decimal(18,2)");

                e.Property(x => x.SoTienDaThanhToan)
    .HasColumnName("so_tien_da_thanh_toan")
    .HasPrecision(18, 2)
    .HasDefaultValue(0m);

                e.Property(x => x.TrangThaiCongNo)
                    .HasColumnName("trang_thai_cong_no")
                    .HasMaxLength(20)
                    .HasDefaultValue("chua_tt");

                e.Property(x => x.TrangThai).HasColumnName("trang_thai").HasMaxLength(20).HasDefaultValue("draft");

                e.Property(x => x.NgayTao).HasColumnName("ngay_tao");
                e.Property(x => x.NguoiTao).HasColumnName("nguoi_tao");
                e.Property(x => x.NgayCapNhat).HasColumnName("ngay_cap_nhat");
                e.Property(x => x.NguoiCapNhat).HasColumnName("nguoi_cap_nhat");
                e.Property(x => x.DaXoa).HasColumnName("da_xoa").HasDefaultValue(false);

                e.HasMany(x => x.Dongs).WithOne().HasForeignKey(d => d.HoaDonBanId);
            });

            b.Entity<HoaDonBanDong>(e =>
            {
                e.ToTable("hoa_don_ban_dong");
                e.HasKey(x => x.Id);

                e.Property(x => x.HoaDonBanId).HasColumnName("hoa_don_ban_id");
                e.Property(x => x.TenHang).HasColumnName("ten_hang").HasMaxLength(200);
                e.Property(x => x.QuyCach).HasColumnName("quy_cach").HasMaxLength(500);
                e.Property(x => x.DonViTinh).HasColumnName("dvt").HasMaxLength(50);
                e.Property(x => x.SoLuong).HasColumnName("so_luong").HasColumnType("decimal(18,3)");
                e.Property(x => x.DonGia).HasColumnName("don_gia").HasColumnType("decimal(18,2)");

                e.Property(x => x.TienHang).HasColumnName("tien_hang").HasColumnType("decimal(18,2)");
                e.Property(x => x.TienThue).HasColumnName("tien_thue").HasColumnType("decimal(18,2)");
                e.Property(x => x.ThanhTien).HasColumnName("thanh_tien").HasColumnType("decimal(18,2)");
                e.Property(x => x.ThueSuat).HasColumnName("thue_suat").HasColumnType("decimal(9,4)");

                e.Property(x => x.DaXoa).HasColumnName("da_xoa").HasDefaultValue(false);
            });
            // ===== PHIẾU NHẬP =====
            b.Entity<PhieuNhap>(e =>
            {
                e.ToTable("phieu_nhap");
                e.HasKey(x => x.Id);

                e.Property(x => x.SoCt).HasColumnName("so_ct").HasMaxLength(50).IsRequired();
                e.HasIndex(x => x.SoCt).IsUnique();

                e.Property(x => x.NhaCungCapId).HasColumnName("nha_cung_cap_id");
                e.HasOne<NhaCungCap>().WithMany().HasForeignKey(x => x.NhaCungCapId);

                e.Property(x => x.KhoId).HasColumnName("kho_id");
                e.HasOne<Kho>().WithMany().HasForeignKey(x => x.KhoId);

                e.Property(x => x.NgayNhap).HasColumnName("ngay_nhap");

                e.Property(x => x.DonMuaId).HasColumnName("don_mua_id");
                e.HasOne<DonMua>().WithMany().HasForeignKey(x => x.DonMuaId);

                e.Property(x => x.GhiChu).HasColumnName("ghi_chu");
                e.Property(x => x.NgayTao).HasColumnName("ngay_tao");
                e.Property(x => x.NguoiTao).HasColumnName("nguoi_tao");
            });

            // ===== PHIẾU NHẬP DÒNG =====
            b.Entity<PhieuNhapDong>(e =>
            {
                e.ToTable("phieu_nhap_dong");
                e.HasKey(x => x.Id);

                e.Property(x => x.PhieuNhapId).HasColumnName("phieu_nhap_id");
                e.HasOne<PhieuNhap>()
                    .WithMany(p => p.Dong)
                    .HasForeignKey(x => x.PhieuNhapId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.Property(x => x.VatTuId).HasColumnName("vat_tu_id");
                e.HasOne<VatTu>().WithMany().HasForeignKey(x => x.VatTuId);

                e.Property(x => x.SoLo).HasColumnName("so_lo");
                e.Property(x => x.SoLuong).HasColumnName("so_luong").HasPrecision(18, 3);
                e.Property(x => x.DonGia).HasColumnName("don_gia").HasPrecision(18, 2);
                e.Property(x => x.GiaTri).HasColumnName("gia_tri").HasPrecision(18, 2);
            });
            // ===== PHIẾU XUẤT =====
            b.Entity<PhieuXuat>(e =>
            {
                e.ToTable("phieu_xuat");
                e.HasKey(x => x.Id);

                e.Property(x => x.SoCt).HasColumnName("so_ct").HasMaxLength(50).IsRequired();
                e.HasIndex(x => x.SoCt).IsUnique();

                e.Property(x => x.KhoId).HasColumnName("kho_id");
                e.HasOne<Kho>().WithMany().HasForeignKey(x => x.KhoId);

                e.Property(x => x.LenhSanXuatId).HasColumnName("lenh_san_xuat_id");
                e.HasOne<LenhSanXuat>().WithMany().HasForeignKey(x => x.LenhSanXuatId);

                e.Property(x => x.NgayXuat).HasColumnName("ngay_xuat");
                e.Property(x => x.GhiChu).HasColumnName("ghi_chu");

                e.Property(x => x.NgayTao).HasColumnName("ngay_tao");
                e.Property(x => x.NguoiTao).HasColumnName("nguoi_tao");
            });

            // ===== PHIẾU XUẤT DÒNG =====
            b.Entity<PhieuXuatDong>(e =>
            {
                e.ToTable("phieu_xuat_dong");
                e.HasKey(x => x.Id);

                e.Property(x => x.PhieuXuatId).HasColumnName("phieu_xuat_id");
                e.HasOne<PhieuXuat>()
                    .WithMany(p => p.Dong)
                    .HasForeignKey(x => x.PhieuXuatId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.Property(x => x.VatTuId).HasColumnName("vat_tu_id");
                e.HasOne<VatTu>().WithMany().HasForeignKey(x => x.VatTuId);

                e.Property(x => x.SoLo).HasColumnName("so_lo");
                e.Property(x => x.SoLuong).HasColumnName("so_luong").HasPrecision(18, 3);
                e.Property(x => x.DonGia).HasColumnName("don_gia").HasPrecision(18, 2);
                e.Property(x => x.GiaTri).HasColumnName("gia_tri").HasPrecision(18, 2);
            });

            // ===== HÓA ĐƠN MUA =====
            b.Entity<HoaDonMua>(e =>
            {
                e.ToTable("hoa_don_mua");
                e.HasKey(x => x.Id);

                e.Property(x => x.SoCt).HasColumnName("so_ct").HasMaxLength(50).IsRequired();
                e.HasIndex(x => x.SoCt).IsUnique();

                e.Property(x => x.NhaCungCapId).HasColumnName("nha_cung_cap_id");
                e.HasOne<NhaCungCap>().WithMany().HasForeignKey(x => x.NhaCungCapId);

                e.Property(x => x.SoTienDaThanhToan)
    .HasColumnName("so_tien_da_thanh_toan")
    .HasPrecision(18, 2)
    .HasDefaultValue(0m);

                e.Property(x => x.TrangThaiCongNo)
                    .HasColumnName("trang_thai_cong_no")
                    .HasMaxLength(20)
                    .HasDefaultValue("chua_tt");
                e.Property(x => x.NgayHoaDon).HasColumnName("ngay_hoa_don");
                e.Property(x => x.HanThanhToan).HasColumnName("han_thanh_toan");

                e.Property(x => x.DonMuaId).HasColumnName("don_mua_id");
                e.HasOne<DonMua>().WithMany().HasForeignKey(x => x.DonMuaId);

                e.Property(x => x.TienHang).HasColumnName("tien_hang").HasPrecision(18, 2);
                e.Property(x => x.TienThue).HasColumnName("tien_thue").HasPrecision(18, 2);
                e.Property(x => x.TongTien).HasColumnName("tong_tien").HasPrecision(18, 2);
                e.Property(x => x.TrangThai).HasColumnName("trang_thai").HasMaxLength(20).HasDefaultValue("con_no");
                e.Property(x => x.NgayTao).HasColumnName("ngay_tao");
                e.Property(x => x.NguoiTao).HasColumnName("nguoi_tao");
            });

            // ===== HÓA ĐƠN MUA DÒNG =====
            b.Entity<HoaDonMuaDong>(e =>
            {
                e.ToTable("hoa_don_mua_dong");
                e.HasKey(x => x.Id);

                e.Property(x => x.HoaDonMuaId).HasColumnName("hoa_don_mua_id");
                e.HasOne<HoaDonMua>()
                    .WithMany(h => h.Dong)
                    .HasForeignKey(x => x.HoaDonMuaId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.Property(x => x.VatTuId).HasColumnName("vat_tu_id");
                e.HasOne<VatTu>().WithMany().HasForeignKey(x => x.VatTuId);

                e.Property(x => x.SoLuong).HasColumnName("so_luong").HasPrecision(18, 3);
                e.Property(x => x.DonGia).HasColumnName("don_gia").HasPrecision(18, 2);

                e.Property(x => x.ThueSuatId).HasColumnName("thue_suat_id");
                e.HasOne<ThueSuat>().WithMany().HasForeignKey(x => x.ThueSuatId);

                e.Property(x => x.TienThue).HasColumnName("tien_thue").HasPrecision(18, 2);
                e.Property(x => x.ThanhTien).HasColumnName("thanh_tien").HasPrecision(18, 2);
            });
            b.Entity<LenhSanXuat>(e =>
            {
                e.ToTable("lenh_san_xuat");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("id");
                e.Property(x => x.Ma).HasColumnName("ma").HasMaxLength(50).IsRequired();
                e.HasIndex(x => x.Ma).IsUnique();

                e.Property(x => x.NgayLenh).HasColumnName("ngay_lenh");
                e.Property(x => x.SanPhamId).HasColumnName("san_pham_id");
                e.HasOne(x => x.SanPham).WithMany().HasForeignKey(x => x.SanPhamId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.Property(x => x.SoLuongKeHoach).HasColumnName("so_luong_ke_hoach").HasPrecision(18, 3);
                e.Property(x => x.SoLuongThucTe).HasColumnName("so_luong_thuc_te").HasPrecision(18, 3);

                // ===== ngành in
                e.Property(x => x.TenKhachHang).HasColumnName("ten_khach_hang").HasMaxLength(255);
                e.Property(x => x.TenBaiIn).HasColumnName("ten_bai_in").HasMaxLength(255);
                e.Property(x => x.TenGiayIn).HasColumnName("ten_giay_in").HasMaxLength(255);
                e.Property(x => x.KhoIn).HasColumnName("kho_in").HasMaxLength(50);
                e.Property(x => x.SoMauIn).HasColumnName("so_mau_in");
                e.Property(x => x.HinhThucIn).HasColumnName("hinh_thuc_in").HasMaxLength(50);
                e.Property(x => x.SoCon).HasColumnName("so_con");
                e.Property(x => x.MayIn).HasColumnName("may_in").HasMaxLength(100);
                e.Property(x => x.NgayIn).HasColumnName("ngay_in");
                e.Property(x => x.SoLuongThanhPham).HasColumnName("so_luong_thanh_pham").HasPrecision(18, 3);

                // meta
                e.Property(x => x.TrangThai).HasColumnName("trang_thai").HasMaxLength(20);
                e.Property(x => x.GhiChu).HasColumnName("ghi_chu").HasMaxLength(500);
                e.Property(x => x.NgayTao).HasColumnName("ngay_tao");
                e.Property(x => x.NgayCapNhat).HasColumnName("ngay_cap_nhat");
                e.Property(x => x.DaXoa).HasColumnName("da_xoa");

                e.HasMany(x => x.Dongs).WithOne(d => d.Lenh)
                    .HasForeignKey(d => d.LenhId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== LenhSanXuatDong
            b.Entity<LenhSanXuatDong>(e =>
            {
                e.ToTable("lenh_san_xuat_dong");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("id");

                e.Property(x => x.LenhId).HasColumnName("lenh_id");
                e.Property(x => x.VatTuId).HasColumnName("vat_tu_id");
                e.HasOne(x => x.VatTu)
                 .WithMany()
                 .HasForeignKey(x => x.VatTuId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.Property(x => x.LoaiDong).HasColumnName("loai_dong").HasMaxLength(20);
                e.Property(x => x.HeSo).HasColumnName("he_so").HasPrecision(18, 6);
                e.Property(x => x.SoLuong).HasColumnName("so_luong").HasPrecision(18, 3);
                e.Property(x => x.DonGia).HasColumnName("don_gia").HasPrecision(18, 3);
                e.Property(x => x.GiaTri).HasColumnName("gia_tri").HasPrecision(18, 3);

                e.Property(x => x.GhiChu).HasColumnName("ghi_chu").HasMaxLength(255);
                e.Property(x => x.DaXoa).HasColumnName("da_xoa");

                e.HasIndex(x => new { x.LenhId, x.LoaiDong });
                e.HasIndex(x => x.VatTuId);
            });

        }
    }
}
