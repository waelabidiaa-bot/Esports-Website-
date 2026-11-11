using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EsportPlatform.API.Migrations
{
    /// <inheritdoc />
    public partial class updatescorebinmatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "scoreB",
                table: "Matches",
                newName: "ScoreB");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ScoreB",
                table: "Matches",
                newName: "scoreB");
        }
    }
}
