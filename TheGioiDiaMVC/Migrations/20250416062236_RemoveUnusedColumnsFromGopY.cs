using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheGioiDiaMVC.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedColumnsFromGopY : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanTraLoi",
                table: "GopY");

            // 👇 Thêm dòng này để gỡ bỏ index trước
            migrationBuilder.DropIndex(
                name: "IX_GopY_MaCD",
                table: "GopY");

            migrationBuilder.DropColumn(
                name: "MaCD",
                table: "GopY");

            migrationBuilder.DropColumn(
                name: "NgayTL",
                table: "GopY");

            migrationBuilder.DropColumn(
                name: "TraLoi",
                table: "GopY");
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CanTraLoi",
                table: "GopY",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaCD",
                table: "GopY",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateOnly>(
                name: "NgayTL",
                table: "GopY",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TraLoi",
                table: "GopY",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
