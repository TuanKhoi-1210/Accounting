using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accounting.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "da_xoa",
                schema: "acc",
                table: "khach_hang",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "dia_chi",
                schema: "acc",
                table: "khach_hang",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "email",
                schema: "acc",
                table: "khach_hang",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ma_so_thue",
                schema: "acc",
                table: "khach_hang",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ngay_cap_nhat",
                schema: "acc",
                table: "khach_hang",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ngay_tao",
                schema: "acc",
                table: "khach_hang",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "nguoi_cap_nhat",
                schema: "acc",
                table: "khach_hang",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "nguoi_tao",
                schema: "acc",
                table: "khach_hang",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "so_dien_thoai",
                schema: "acc",
                table: "khach_hang",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "don_vi_tinh",
                schema: "acc",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ten = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_don_vi_tinh", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "kho",
                schema: "acc",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    nguoi_tao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    nguoi_cap_nhat = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    da_xoa = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kho", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "nha_cung_cap",
                schema: "acc",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ma_so_thue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dia_chi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    so_dien_thoai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    nguoi_tao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    nguoi_cap_nhat = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    da_xoa = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nha_cung_cap", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tai_khoan_ngan_hang",
                schema: "acc",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ten_ngan_hang = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    so_tai_khoan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    tien_te = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "VND")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tai_khoan_ngan_hang", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "thue_suat",
                schema: "acc",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ten = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ty_le = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false),
                    dang_hoat_dong = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_thue_suat", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "vat_tu",
                schema: "acc",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    kich_thuoc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    loai_giay = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dinh_luong_gsm = table.Column<int>(type: "int", nullable: true),
                    mau_in = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    gia_cong = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    don_vi_tinh_id = table.Column<long>(type: "bigint", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    nguoi_tao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    nguoi_cap_nhat = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    da_xoa = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vat_tu", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vat_tu_don_vi_tinh_don_vi_tinh_id",
                        column: x => x.don_vi_tinh_id,
                        principalSchema: "acc",
                        principalTable: "don_vi_tinh",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_khach_hang_ma",
                schema: "acc",
                table: "khach_hang",
                column: "ma",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_don_vi_tinh_ma",
                schema: "acc",
                table: "don_vi_tinh",
                column: "ma",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_kho_ma",
                schema: "acc",
                table: "kho",
                column: "ma",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_nha_cung_cap_ma",
                schema: "acc",
                table: "nha_cung_cap",
                column: "ma",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tai_khoan_ngan_hang_ma",
                schema: "acc",
                table: "tai_khoan_ngan_hang",
                column: "ma",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vat_tu_don_vi_tinh_id",
                schema: "acc",
                table: "vat_tu",
                column: "don_vi_tinh_id");

            migrationBuilder.CreateIndex(
                name: "IX_vat_tu_ma",
                schema: "acc",
                table: "vat_tu",
                column: "ma",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "kho",
                schema: "acc");

            migrationBuilder.DropTable(
                name: "nha_cung_cap",
                schema: "acc");

            migrationBuilder.DropTable(
                name: "tai_khoan_ngan_hang",
                schema: "acc");

            migrationBuilder.DropTable(
                name: "thue_suat",
                schema: "acc");

            migrationBuilder.DropTable(
                name: "vat_tu",
                schema: "acc");

            migrationBuilder.DropTable(
                name: "don_vi_tinh",
                schema: "acc");

            migrationBuilder.DropIndex(
                name: "IX_khach_hang_ma",
                schema: "acc",
                table: "khach_hang");

            migrationBuilder.DropColumn(
                name: "da_xoa",
                schema: "acc",
                table: "khach_hang");

            migrationBuilder.DropColumn(
                name: "dia_chi",
                schema: "acc",
                table: "khach_hang");

            migrationBuilder.DropColumn(
                name: "email",
                schema: "acc",
                table: "khach_hang");

            migrationBuilder.DropColumn(
                name: "ma_so_thue",
                schema: "acc",
                table: "khach_hang");

            migrationBuilder.DropColumn(
                name: "ngay_cap_nhat",
                schema: "acc",
                table: "khach_hang");

            migrationBuilder.DropColumn(
                name: "ngay_tao",
                schema: "acc",
                table: "khach_hang");

            migrationBuilder.DropColumn(
                name: "nguoi_cap_nhat",
                schema: "acc",
                table: "khach_hang");

            migrationBuilder.DropColumn(
                name: "nguoi_tao",
                schema: "acc",
                table: "khach_hang");

            migrationBuilder.DropColumn(
                name: "so_dien_thoai",
                schema: "acc",
                table: "khach_hang");
        }
    }
}
