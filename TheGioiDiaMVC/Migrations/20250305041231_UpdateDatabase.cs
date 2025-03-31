using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheGioiDiaMVC.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            

            

            

            

            migrationBuilder.CreateIndex(
                name: "IX_BanBe_MaHH",
                table: "BanBe",
                column: "MaHH");

            migrationBuilder.CreateIndex(
                name: "IX_BanBe_MaKH",
                table: "BanBe",
                column: "MaKH");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietHD_MaHD",
                table: "ChiTietHD",
                column: "MaHD");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietHD_MaHH",
                table: "ChiTietHD",
                column: "MaHH");

            migrationBuilder.CreateIndex(
                name: "IX_ChuDe_MaNV",
                table: "ChuDe",
                column: "MaNV");

            migrationBuilder.CreateIndex(
                name: "IX_GopY_MaCD",
                table: "GopY",
                column: "MaCD");

            migrationBuilder.CreateIndex(
                name: "IX_HangHoa_MaLoai",
                table: "HangHoa",
                column: "MaLoai");

            migrationBuilder.CreateIndex(
                name: "IX_HangHoa_MaNCC",
                table: "HangHoa",
                column: "MaNCC");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDon_MaKH",
                table: "HoaDon",
                column: "MaKH");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDon_MaNV",
                table: "HoaDon",
                column: "MaNV");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDon_MaTrangThai",
                table: "HoaDon",
                column: "MaTrangThai");

            migrationBuilder.CreateIndex(
                name: "IX_HoiDap_MaNV",
                table: "HoiDap",
                column: "MaNV");

            migrationBuilder.CreateIndex(
                name: "IX_PhanCong_MaNV",
                table: "PhanCong",
                column: "MaNV");

            migrationBuilder.CreateIndex(
                name: "IX_PhanCong_MaPB",
                table: "PhanCong",
                column: "MaPB");

            migrationBuilder.CreateIndex(
                name: "IX_PhanQuyen_MaPB",
                table: "PhanQuyen",
                column: "MaPB");

            migrationBuilder.CreateIndex(
                name: "IX_PhanQuyen_MaTrang",
                table: "PhanQuyen",
                column: "MaTrang");

            migrationBuilder.CreateIndex(
                name: "IX_YeuThich_MaHH",
                table: "YeuThich",
                column: "MaHH");

            migrationBuilder.CreateIndex(
                name: "IX_YeuThich_MaKH",
                table: "YeuThich",
                column: "MaKH");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BanBe");

            migrationBuilder.DropTable(
                name: "ChiTietHD");

            migrationBuilder.DropTable(
                name: "GopY");

            migrationBuilder.DropTable(
                name: "HoiDap");

            migrationBuilder.DropTable(
                name: "PhanCong");

            migrationBuilder.DropTable(
                name: "PhanQuyen");

            migrationBuilder.DropTable(
                name: "YeuThich");

            migrationBuilder.DropTable(
                name: "HoaDon");

            migrationBuilder.DropTable(
                name: "ChuDe");

            migrationBuilder.DropTable(
                name: "PhongBan");

            migrationBuilder.DropTable(
                name: "TrangWeb");

            migrationBuilder.DropTable(
                name: "HangHoa");

            migrationBuilder.DropTable(
                name: "TrangThai");

            migrationBuilder.DropTable(
                name: "KhachHang");

            migrationBuilder.DropTable(
                name: "NhanVien");

            migrationBuilder.DropTable(
                name: "Loai");

            migrationBuilder.DropTable(
                name: "NhaCungCap");
        }
    }
}
