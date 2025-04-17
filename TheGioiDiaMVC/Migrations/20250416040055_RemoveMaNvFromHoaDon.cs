using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheGioiDiaMVC.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMaNvFromHoaDon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Xoá index trước
            migrationBuilder.DropIndex(
                name: "IX_HoaDon_MaNV",
                table: "HoaDon");

            // (Nếu có constraint default thì đoạn dưới giữ nguyên)
            migrationBuilder.Sql(@"
        DECLARE @var sysname;
        SELECT @var = [d].[name]
        FROM [sys].[default_constraints] [d]
        INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
        WHERE ([d].[parent_object_id] = OBJECT_ID(N'[HoaDon]') AND [c].[name] = N'MaNV');
        IF @var IS NOT NULL EXEC(N'ALTER TABLE [HoaDon] DROP CONSTRAINT [' + @var + ']');
    ");

            // Rồi mới xoá cột
            migrationBuilder.DropColumn(
                name: "MaNV",
                table: "HoaDon");
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MaNV",
                table: "HoaDon",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
