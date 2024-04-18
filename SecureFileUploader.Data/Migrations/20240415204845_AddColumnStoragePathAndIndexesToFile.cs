using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecureFileUploader.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnStoragePathAndIndexesToFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StoragePath",
                table: "Files",
                type: "TEXT",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Files_FileName_UserId",
                table: "Files",
                columns: new[] { "FileName", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Files_StoragePath",
                table: "Files",
                column: "StoragePath",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Files_FileName_UserId",
                table: "Files");

            migrationBuilder.DropIndex(
                name: "IX_Files_StoragePath",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "StoragePath",
                table: "Files");
        }
    }
}
