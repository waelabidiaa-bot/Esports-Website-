using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EsportPlatform.API.Migrations
{
    /// <inheritdoc />
    public partial class AddBroadcastVideos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VideoUrl",
                table: "Broadcasts",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VideoUrl",
                table: "Broadcasts");
        }
    }
}
