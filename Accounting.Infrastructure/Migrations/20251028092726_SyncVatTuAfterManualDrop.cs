using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accounting.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncVatTuAfterManualDrop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_don_mua_nha_cung_cap_nha_cung_cap_id",
                schema: "acc",
                table: "don_mua");

            migrationBuilder.DropForeignKey(
                name: "FK_don_mua_dong_vat_tu_vat_tu_id",
                schema: "acc",
                table: "don_mua_dong");

            migrationBuilder.DropForeignKey(
                name: "FK_hoa_don_mua_nha_cung_cap_nha_cung_cap_id",
                schema: "acc",
                table: "hoa_don_mua");

            migrationBuilder.DropForeignKey(
                name: "FK_hoa_don_mua_dong_vat_tu_vat_tu_id",
                schema: "acc",
                table: "hoa_don_mua_dong");

            migrationBuilder.DropForeignKey(
                name: "FK_phieu_nhap_kho_kho_id",
                schema: "acc",
                table: "phieu_nhap");

            migrationBuilder.DropForeignKey(
                name: "FK_phieu_nhap_dong_vat_tu_vat_tu_id",
                schema: "acc",
                table: "phieu_nhap_dong");









            migrationBuilder.AlterColumn<string>(
                name: "tien_te",
                schema: "acc",
                table: "tai_khoan_ngan_hang",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                defaultValue: "VND",
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldDefaultValue: "VND");


            migrationBuilder.AlterColumn<int>(
                name: "vat_tu_id",
                schema: "acc",
                table: "phieu_nhap_dong",
                type: "int",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");




            migrationBuilder.AlterColumn<decimal>(
                name: "gia_tri",
                schema: "acc",
                table: "phieu_nhap_dong",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);




            migrationBuilder.AlterColumn<DateTime>(
                name: "ngay_tao",
                schema: "acc",
                table: "phieu_nhap",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<int>(
                name: "kho_id",
                schema: "acc",
                table: "phieu_nhap",
                type: "int",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");





            migrationBuilder.AlterColumn<DateTime>(
                name: "ngay_tao",
                schema: "acc",
                table: "nha_cung_cap",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");


            migrationBuilder.AlterColumn<DateTime>(
                name: "ngay_tao",
                schema: "acc",
                table: "kho",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");


            migrationBuilder.AlterColumn<DateTime>(
                name: "ngay_tao",
                schema: "acc",
                table: "khach_hang",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");


            migrationBuilder.AlterColumn<int>(
                name: "vat_tu_id",
                schema: "acc",
                table: "hoa_don_mua_dong",
                type: "int",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<decimal>(
                name: "tien_thue",
                schema: "acc",
                table: "hoa_don_mua_dong",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);


            migrationBuilder.AlterColumn<decimal>(
                name: "thanh_tien",
                schema: "acc",
                table: "hoa_don_mua_dong",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);




            migrationBuilder.AlterColumn<string>(
                name: "trang_thai",
                schema: "acc",
                table: "hoa_don_mua",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                defaultValue: "con_no",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "con_no");

            migrationBuilder.AlterColumn<decimal>(
                name: "tong_tien",
                schema: "acc",
                table: "hoa_don_mua",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "tien_thue",
                schema: "acc",
                table: "hoa_don_mua",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "tien_hang",
                schema: "acc",
                table: "hoa_don_mua",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<int>(
                name: "nha_cung_cap_id",
                schema: "acc",
                table: "hoa_don_mua",
                type: "int",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ngay_tao",
                schema: "acc",
                table: "hoa_don_mua",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");



            migrationBuilder.AlterColumn<int>(
                name: "vat_tu_id",
                schema: "acc",
                table: "don_mua_dong",
                type: "int",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<decimal>(
                name: "tien_thue",
                schema: "acc",
                table: "don_mua_dong",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);



            migrationBuilder.AlterColumn<decimal>(
                name: "thanh_tien",
                schema: "acc",
                table: "don_mua_dong",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);






            migrationBuilder.AlterColumn<string>(
                name: "trang_thai",
                schema: "acc",
                table: "don_mua",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                defaultValue: "nhap",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "nhap");

            migrationBuilder.AlterColumn<decimal>(
                name: "tong_tien",
                schema: "acc",
                table: "don_mua",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "tien_thue",
                schema: "acc",
                table: "don_mua",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "tien_te",
                schema: "acc",
                table: "don_mua",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<decimal>(
                name: "tien_hang",
                schema: "acc",
                table: "don_mua",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<int>(
                name: "nha_cung_cap_id",
                schema: "acc",
                table: "don_mua",
                type: "int",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");



            migrationBuilder.AddForeignKey(
                name: "FK_don_mua_dong_vat_tu_vat_tu_id",
                schema: "acc",
                table: "don_mua_dong",
                column: "vat_tu_id",
                principalSchema: "acc",
                principalTable: "vat_tu",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_hoa_don_mua_nha_cung_cap_nha_cung_cap_id",
                schema: "acc",
                table: "hoa_don_mua",
                column: "nha_cung_cap_id",
                principalSchema: "acc",
                principalTable: "nha_cung_cap",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_hoa_don_mua_dong_vat_tu_vat_tu_id",
                schema: "acc",
                table: "hoa_don_mua_dong",
                column: "vat_tu_id",
                principalSchema: "acc",
                principalTable: "vat_tu",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_phieu_nhap_kho_kho_id",
                schema: "acc",
                table: "phieu_nhap",
                column: "kho_id",
                principalSchema: "acc",
                principalTable: "kho",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_phieu_nhap_dong_vat_tu_vat_tu_id",
                schema: "acc",
                table: "phieu_nhap_dong",
                column: "vat_tu_id",
                principalSchema: "acc",
                principalTable: "vat_tu",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_don_mua_nha_cung_cap_nha_cung_cap_id",
                schema: "acc",
                table: "don_mua");

            migrationBuilder.DropForeignKey(
                name: "FK_don_mua_dong_vat_tu_vat_tu_id",
                schema: "acc",
                table: "don_mua_dong");

            migrationBuilder.DropForeignKey(
                name: "FK_hoa_don_mua_nha_cung_cap_nha_cung_cap_id",
                schema: "acc",
                table: "hoa_don_mua");

            migrationBuilder.DropForeignKey(
                name: "FK_hoa_don_mua_dong_vat_tu_vat_tu_id",
                schema: "acc",
                table: "hoa_don_mua_dong");

            migrationBuilder.DropForeignKey(
                name: "FK_phieu_nhap_kho_kho_id",
                schema: "acc",
                table: "phieu_nhap");

            migrationBuilder.DropForeignKey(
                name: "FK_phieu_nhap_dong_vat_tu_vat_tu_id",
                schema: "acc",
                table: "phieu_nhap_dong");





            migrationBuilder.AlterColumn<long>(
                name: "Id",
                schema: "acc",
                table: "vat_tu",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                schema: "acc",
                table: "thue_suat",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<string>(
                name: "tien_te",
                schema: "acc",
                table: "tai_khoan_ngan_hang",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "VND",
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true,
                oldDefaultValue: "VND");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                schema: "acc",
                table: "tai_khoan_ngan_hang",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<long>(
                name: "vat_tu_id",
                schema: "acc",
                table: "phieu_nhap_dong",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);




            migrationBuilder.AlterColumn<decimal>(
                name: "gia_tri",
                schema: "acc",
                table: "phieu_nhap_dong",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                schema: "acc",
                table: "phieu_nhap_dong",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<long>(
                name: "nha_cung_cap_id",
                schema: "acc",
                table: "phieu_nhap",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ngay_tao",
                schema: "acc",
                table: "phieu_nhap",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "kho_id",
                schema: "acc",
                table: "phieu_nhap",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "don_mua_id",
                schema: "acc",
                table: "phieu_nhap",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                schema: "acc",
                table: "phieu_nhap",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ngay_tao",
                schema: "acc",
                table: "nha_cung_cap",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                schema: "acc",
                table: "nha_cung_cap",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ngay_tao",
                schema: "acc",
                table: "kho",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                schema: "acc",
                table: "kho",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ngay_tao",
                schema: "acc",
                table: "khach_hang",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                schema: "acc",
                table: "khach_hang",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<long>(
                name: "vat_tu_id",
                schema: "acc",
                table: "hoa_don_mua_dong",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "tien_thue",
                schema: "acc",
                table: "hoa_don_mua_dong",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "thue_suat_id",
                schema: "acc",
                table: "hoa_don_mua_dong",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "thanh_tien",
                schema: "acc",
                table: "hoa_don_mua_dong",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "hoa_don_mua_id",
                schema: "acc",
                table: "hoa_don_mua_dong",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                schema: "acc",
                table: "hoa_don_mua_dong",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<string>(
                name: "trang_thai",
                schema: "acc",
                table: "hoa_don_mua",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "con_no",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true,
                oldDefaultValue: "con_no");

            migrationBuilder.AlterColumn<decimal>(
                name: "tong_tien",
                schema: "acc",
                table: "hoa_don_mua",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "tien_thue",
                schema: "acc",
                table: "hoa_don_mua",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "tien_hang",
                schema: "acc",
                table: "hoa_don_mua",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "nha_cung_cap_id",
                schema: "acc",
                table: "hoa_don_mua",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ngay_tao",
                schema: "acc",
                table: "hoa_don_mua",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
            migrationBuilder.Sql(@"
IF COL_LENGTH('acc.vat_tu','ngay_tao') IS NOT NULL
BEGIN
    DECLARE @dc sysname;
    SELECT @dc = d.name
    FROM sys.default_constraints d
    JOIN sys.columns c ON d.parent_column_id = c.column_id AND d.parent_object_id = c.object_id
    WHERE d.parent_object_id = OBJECT_ID(N'acc.vat_tu') AND c.name = N'ngay_tao';

    IF @dc IS NOT NULL EXEC(N'ALTER TABLE [acc].[vat_tu] DROP CONSTRAINT [' + @dc + ']');

    -- Bạn không muốn cột này => drop luôn (safe)
    ALTER TABLE [acc].[vat_tu] DROP COLUMN [ngay_tao];
END
");

            migrationBuilder.AlterColumn<long>(
                name: "don_mua_id",
                schema: "acc",
                table: "hoa_don_mua",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                schema: "acc",
                table: "hoa_don_mua",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                schema: "acc",
                table: "don_vi_tinh",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<long>(
                name: "vat_tu_id",
                schema: "acc",
                table: "don_mua_dong",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "tien_thue",
                schema: "acc",
                table: "don_mua_dong",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "thue_suat_id",
                schema: "acc",
                table: "don_mua_dong",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "thanh_tien",
                schema: "acc",
                table: "don_mua_dong",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "don_mua_id",
                schema: "acc",
                table: "don_mua_dong",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);


            migrationBuilder.AlterColumn<long>(
                name: "Id",
                schema: "acc",
                table: "don_mua_dong",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<string>(
                name: "trang_thai",
                schema: "acc",
                table: "don_mua",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "nhap",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true,
                oldDefaultValue: "nhap");

            migrationBuilder.AlterColumn<decimal>(
                name: "tong_tien",
                schema: "acc",
                table: "don_mua",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "tien_thue",
                schema: "acc",
                table: "don_mua",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "tien_te",
                schema: "acc",
                table: "don_mua",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "tien_hang",
                schema: "acc",
                table: "don_mua",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "nha_cung_cap_id",
                schema: "acc",
                table: "don_mua",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                schema: "acc",
                table: "don_mua",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddForeignKey(
                name: "FK_don_mua_nha_cung_cap_nha_cung_cap_id",
                schema: "acc",
                table: "don_mua",
                column: "nha_cung_cap_id",
                principalSchema: "acc",
                principalTable: "nha_cung_cap",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_don_mua_dong_vat_tu_vat_tu_id",
                schema: "acc",
                table: "don_mua_dong",
                column: "vat_tu_id",
                principalSchema: "acc",
                principalTable: "vat_tu",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hoa_don_mua_nha_cung_cap_nha_cung_cap_id",
                schema: "acc",
                table: "hoa_don_mua",
                column: "nha_cung_cap_id",
                principalSchema: "acc",
                principalTable: "nha_cung_cap",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hoa_don_mua_dong_vat_tu_vat_tu_id",
                schema: "acc",
                table: "hoa_don_mua_dong",
                column: "vat_tu_id",
                principalSchema: "acc",
                principalTable: "vat_tu",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_phieu_nhap_kho_kho_id",
                schema: "acc",
                table: "phieu_nhap",
                column: "kho_id",
                principalSchema: "acc",
                principalTable: "kho",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_phieu_nhap_dong_vat_tu_vat_tu_id",
                schema: "acc",
                table: "phieu_nhap_dong",
                column: "vat_tu_id",
                principalSchema: "acc",
                principalTable: "vat_tu",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
