using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcfrApp.Migrations
{
    /// <inheritdoc />
    public partial class AgencySlug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AgencySlug",
                table: "CorrectionReferences",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgencySlug",
                table: "CorrectionReferences");
        }
    }
}
