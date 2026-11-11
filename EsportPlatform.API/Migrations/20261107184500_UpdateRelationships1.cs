using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EsportPlatform.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRelationships1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Teams_teamsId",
                table: "Matches");

            migrationBuilder.DropIndex(
                name: "IX_Matches_teamsId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "teamsId",
                table: "Matches");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "teamsId",
                table: "Matches",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Matches_teamsId",
                table: "Matches",
                column: "teamsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Teams_teamsId",
                table: "Matches",
                column: "teamsId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
