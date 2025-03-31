using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheGioiDiaMVC.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGuiDiaHiem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GuiDiaHiems",
                columns: table => new
                {
                    MaGuiDia = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDia = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Gia = table.Column<float>(type: "real", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HinhAnh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TinhTrang = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MaKH = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    NgayGui = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuiDiaHiems", x => x.MaGuiDia);
                    table.ForeignKey(
                        name: "FK_GuiDiaHiems_KhachHang_MaKH",
                        column: x => x.MaKH,
                        principalTable: "KhachHang",
                        principalColumn: "MaKH",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GuiDiaHiems_MaKH",
                table: "GuiDiaHiems",
                column: "MaKH");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GuiDiaHiems");
        }
    }
}
