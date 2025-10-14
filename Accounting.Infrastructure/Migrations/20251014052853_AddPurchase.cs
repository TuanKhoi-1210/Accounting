using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accounting.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "don_mua",
                schema: "acc",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    so_ct = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    nha_cung_cap_id = table.Column<long>(type: "bigint", nullable: false),
                    ngay_don = table.Column<DateTime>(type: "datetime2", nullable: false),
                    tien_te = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ty_gia = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false, defaultValue: 1.0000m),
                    co_hop_dong_lon = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ghi_chu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    trang_thai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "nhap"),
                    tien_hang = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    tien_thue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    tong_tien = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_don_mua", x => x.Id);
                    table.ForeignKey(
                        name: "FK_don_mua_nha_cung_cap_nha_cung_cap_id",
                        column: x => x.nha_cung_cap_id,
                        principalSchema: "acc",
                        principalTable: "nha_cung_cap",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "don_mua_dong",
                schema: "acc",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    don_mua_id = table.Column<long>(type: "bigint", nullable: false),
                    vat_tu_id = table.Column<long>(type: "bigint", nullable: false),
                    kich_thuoc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    loai_giay = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dinh_luong_gsm = table.Column<int>(type: "int", nullable: true),
                    mau_in = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    gia_cong = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    so_luong = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    don_gia = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    thue_suat_id = table.Column<long>(type: "bigint", nullable: true),
                    tien_thue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    thanh_tien = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_don_mua_dong", x => x.Id);
                    table.ForeignKey(
                        name: "FK_don_mua_dong_don_mua_don_mua_id",
                        column: x => x.don_mua_id,
                        principalSchema: "acc",
                        principalTable: "don_mua",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_don_mua_dong_thue_suat_thue_suat_id",
                        column: x => x.thue_suat_id,
                        principalSchema: "acc",
                        principalTable: "thue_suat",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_don_mua_dong_vat_tu_vat_tu_id",
                        column: x => x.vat_tu_id,
                        principalSchema: "acc",
                        principalTable: "vat_tu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hoa_don_mua",
                schema: "acc",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    so_ct = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    nha_cung_cap_id = table.Column<long>(type: "bigint", nullable: false),
                    ngay_hoa_don = table.Column<DateTime>(type: "datetime2", nullable: false),
                    han_thanh_toan = table.Column<DateTime>(type: "datetime2", nullable: true),
                    don_mua_id = table.Column<long>(type: "bigint", nullable: true),
                    tien_hang = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    tien_thue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    tong_tien = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    trang_thai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "con_no"),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    nguoi_tao = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hoa_don_mua", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hoa_don_mua_don_mua_don_mua_id",
                        column: x => x.don_mua_id,
                        principalSchema: "acc",
                        principalTable: "don_mua",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_hoa_don_mua_nha_cung_cap_nha_cung_cap_id",
                        column: x => x.nha_cung_cap_id,
                        principalSchema: "acc",
                        principalTable: "nha_cung_cap",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "phieu_nhap",
                schema: "acc",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    so_ct = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    nha_cung_cap_id = table.Column<long>(type: "bigint", nullable: true),
                    kho_id = table.Column<long>(type: "bigint", nullable: false),
                    ngay_nhap = table.Column<DateTime>(type: "datetime2", nullable: false),
                    don_mua_id = table.Column<long>(type: "bigint", nullable: true),
                    ghi_chu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    nguoi_tao = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_phieu_nhap", x => x.Id);
                    table.ForeignKey(
                        name: "FK_phieu_nhap_don_mua_don_mua_id",
                        column: x => x.don_mua_id,
                        principalSchema: "acc",
                        principalTable: "don_mua",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_phieu_nhap_kho_kho_id",
                        column: x => x.kho_id,
                        principalSchema: "acc",
                        principalTable: "kho",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_phieu_nhap_nha_cung_cap_nha_cung_cap_id",
                        column: x => x.nha_cung_cap_id,
                        principalSchema: "acc",
                        principalTable: "nha_cung_cap",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "hoa_don_mua_dong",
                schema: "acc",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    hoa_don_mua_id = table.Column<long>(type: "bigint", nullable: false),
                    vat_tu_id = table.Column<long>(type: "bigint", nullable: false),
                    so_luong = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    don_gia = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    thue_suat_id = table.Column<long>(type: "bigint", nullable: true),
                    tien_thue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    thanh_tien = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hoa_don_mua_dong", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hoa_don_mua_dong_hoa_don_mua_hoa_don_mua_id",
                        column: x => x.hoa_don_mua_id,
                        principalSchema: "acc",
                        principalTable: "hoa_don_mua",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_hoa_don_mua_dong_thue_suat_thue_suat_id",
                        column: x => x.thue_suat_id,
                        principalSchema: "acc",
                        principalTable: "thue_suat",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_hoa_don_mua_dong_vat_tu_vat_tu_id",
                        column: x => x.vat_tu_id,
                        principalSchema: "acc",
                        principalTable: "vat_tu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "phieu_nhap_dong",
                schema: "acc",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    phieu_nhap_id = table.Column<long>(type: "bigint", nullable: false),
                    vat_tu_id = table.Column<long>(type: "bigint", nullable: false),
                    so_lo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    so_luong = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    don_gia = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    gia_tri = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_phieu_nhap_dong", x => x.Id);
                    table.ForeignKey(
                        name: "FK_phieu_nhap_dong_phieu_nhap_phieu_nhap_id",
                        column: x => x.phieu_nhap_id,
                        principalSchema: "acc",
                        principalTable: "phieu_nhap",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_phieu_nhap_dong_vat_tu_vat_tu_id",
                        column: x => x.vat_tu_id,
                        principalSchema: "acc",
                        principalTable: "vat_tu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_don_mua_nha_cung_cap_id",
                schema: "acc",
                table: "don_mua",
                column: "nha_cung_cap_id");

            migrationBuilder.CreateIndex(
                name: "IX_don_mua_so_ct",
                schema: "acc",
                table: "don_mua",
                column: "so_ct",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_don_mua_dong_don_mua_id",
                schema: "acc",
                table: "don_mua_dong",
                column: "don_mua_id");

            migrationBuilder.CreateIndex(
                name: "IX_don_mua_dong_thue_suat_id",
                schema: "acc",
                table: "don_mua_dong",
                column: "thue_suat_id");

            migrationBuilder.CreateIndex(
                name: "IX_don_mua_dong_vat_tu_id",
                schema: "acc",
                table: "don_mua_dong",
                column: "vat_tu_id");

            migrationBuilder.CreateIndex(
                name: "IX_hoa_don_mua_don_mua_id",
                schema: "acc",
                table: "hoa_don_mua",
                column: "don_mua_id");

            migrationBuilder.CreateIndex(
                name: "IX_hoa_don_mua_nha_cung_cap_id",
                schema: "acc",
                table: "hoa_don_mua",
                column: "nha_cung_cap_id");

            migrationBuilder.CreateIndex(
                name: "IX_hoa_don_mua_so_ct",
                schema: "acc",
                table: "hoa_don_mua",
                column: "so_ct",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hoa_don_mua_dong_hoa_don_mua_id",
                schema: "acc",
                table: "hoa_don_mua_dong",
                column: "hoa_don_mua_id");

            migrationBuilder.CreateIndex(
                name: "IX_hoa_don_mua_dong_thue_suat_id",
                schema: "acc",
                table: "hoa_don_mua_dong",
                column: "thue_suat_id");

            migrationBuilder.CreateIndex(
                name: "IX_hoa_don_mua_dong_vat_tu_id",
                schema: "acc",
                table: "hoa_don_mua_dong",
                column: "vat_tu_id");

            migrationBuilder.CreateIndex(
                name: "IX_phieu_nhap_don_mua_id",
                schema: "acc",
                table: "phieu_nhap",
                column: "don_mua_id");

            migrationBuilder.CreateIndex(
                name: "IX_phieu_nhap_kho_id",
                schema: "acc",
                table: "phieu_nhap",
                column: "kho_id");

            migrationBuilder.CreateIndex(
                name: "IX_phieu_nhap_nha_cung_cap_id",
                schema: "acc",
                table: "phieu_nhap",
                column: "nha_cung_cap_id");

            migrationBuilder.CreateIndex(
                name: "IX_phieu_nhap_so_ct",
                schema: "acc",
                table: "phieu_nhap",
                column: "so_ct",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_phieu_nhap_dong_phieu_nhap_id",
                schema: "acc",
                table: "phieu_nhap_dong",
                column: "phieu_nhap_id");

            migrationBuilder.CreateIndex(
                name: "IX_phieu_nhap_dong_vat_tu_id",
                schema: "acc",
                table: "phieu_nhap_dong",
                column: "vat_tu_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "don_mua_dong",
                schema: "acc");

            migrationBuilder.DropTable(
                name: "hoa_don_mua_dong",
                schema: "acc");

            migrationBuilder.DropTable(
                name: "phieu_nhap_dong",
                schema: "acc");

            migrationBuilder.DropTable(
                name: "hoa_don_mua",
                schema: "acc");

            migrationBuilder.DropTable(
                name: "phieu_nhap",
                schema: "acc");

            migrationBuilder.DropTable(
                name: "don_mua",
                schema: "acc");
        }
    }
}
