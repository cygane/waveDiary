using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace project.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePostModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "userId",
                table: "Posts",
                newName: "username");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "username",
                table: "Posts",
                newName: "userId");
        }
    }
}
