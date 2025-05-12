using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcfrApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Agencies",
                columns: table => new
                {
                    Slug = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ShortName = table.Column<string>(type: "TEXT", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: false),
                    SortableName = table.Column<string>(type: "TEXT", nullable: false),
                    ParentSlug = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agencies", x => x.Slug);
                    table.ForeignKey(
                        name: "FK_Agencies_Agencies_ParentSlug",
                        column: x => x.ParentSlug,
                        principalTable: "Agencies",
                        principalColumn: "Slug");
                });

            migrationBuilder.CreateTable(
                name: "CorrectionCounts",
                columns: table => new
                {
                    AgencySlug = table.Column<string>(type: "TEXT", nullable: false),
                    Count = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorrectionCounts", x => x.AgencySlug);
                });

            migrationBuilder.CreateTable(
                name: "Corrections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<int>(type: "INTEGER", nullable: false),
                    CorrectiveAction = table.Column<string>(type: "TEXT", nullable: false),
                    ErrorCorrected = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ErrorOccurred = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FrCitation = table.Column<string>(type: "TEXT", nullable: false),
                    Position = table.Column<int>(type: "INTEGER", nullable: false),
                    DisplayInToc = table.Column<bool>(type: "INTEGER", nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Corrections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DescendantRanges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<int>(type: "INTEGER", nullable: false),
                    Chapter = table.Column<string>(type: "TEXT", nullable: false),
                    Subchapter = table.Column<string>(type: "TEXT", nullable: false),
                    Part = table.Column<string>(type: "TEXT", nullable: false),
                    Subpart = table.Column<string>(type: "TEXT", nullable: false),
                    Section = table.Column<string>(type: "TEXT", nullable: false),
                    RangeStart = table.Column<string>(type: "TEXT", nullable: false),
                    RangeEnd = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DescendantRanges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TitleStructures",
                columns: table => new
                {
                    Title = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Identifier = table.Column<string>(type: "TEXT", nullable: false),
                    Label = table.Column<string>(type: "TEXT", nullable: false),
                    LabelLevel = table.Column<string>(type: "TEXT", nullable: false),
                    LabelDescription = table.Column<string>(type: "TEXT", nullable: false),
                    Reserved = table.Column<bool>(type: "INTEGER", nullable: false),
                    Size = table.Column<long>(type: "INTEGER", nullable: false),
                    Volumes = table.Column<string>(type: "TEXT", nullable: false),
                    DescendantRange = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TitleStructures", x => x.Title);
                });

            migrationBuilder.CreateTable(
                name: "CfrReferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AgencySlug = table.Column<string>(type: "TEXT", nullable: true),
                    Title = table.Column<int>(type: "INTEGER", nullable: false),
                    Chapter = table.Column<string>(type: "TEXT", nullable: true),
                    Subchapter = table.Column<string>(type: "TEXT", nullable: true),
                    Subpart = table.Column<string>(type: "TEXT", nullable: true),
                    Part = table.Column<string>(type: "TEXT", nullable: true),
                    Section = table.Column<string>(type: "TEXT", nullable: true),
                    Appendix = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CfrReferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CfrReferences_Agencies_AgencySlug",
                        column: x => x.AgencySlug,
                        principalTable: "Agencies",
                        principalColumn: "Slug");
                });

            migrationBuilder.CreateTable(
                name: "CorrectionReferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CorrectionId = table.Column<int>(type: "INTEGER", nullable: false),
                    CfrReference = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    Subtitle = table.Column<string>(type: "TEXT", nullable: true),
                    Chapter = table.Column<string>(type: "TEXT", nullable: true),
                    SubChapter = table.Column<string>(type: "TEXT", nullable: true),
                    Part = table.Column<string>(type: "TEXT", nullable: true),
                    Subpart = table.Column<string>(type: "TEXT", nullable: true),
                    Section = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorrectionReferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CorrectionReferences_Corrections_CorrectionId",
                        column: x => x.CorrectionId,
                        principalTable: "Corrections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StructureNodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TitleStructureId = table.Column<int>(type: "INTEGER", nullable: true),
                    Identifier = table.Column<string>(type: "TEXT", nullable: false),
                    Label = table.Column<string>(type: "TEXT", nullable: false),
                    LabelLevel = table.Column<string>(type: "TEXT", nullable: false),
                    LabelDescription = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    Reserved = table.Column<bool>(type: "INTEGER", nullable: false),
                    Size = table.Column<long>(type: "INTEGER", nullable: false),
                    Volumes = table.Column<string>(type: "TEXT", nullable: false),
                    ReceivedOn = table.Column<string>(type: "TEXT", nullable: true),
                    DescendantRange = table.Column<string>(type: "TEXT", nullable: false),
                    GeneratedId = table.Column<bool>(type: "INTEGER", nullable: false),
                    StructureNodeId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StructureNodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StructureNodes_StructureNodes_StructureNodeId",
                        column: x => x.StructureNodeId,
                        principalTable: "StructureNodes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StructureNodes_TitleStructures_TitleStructureId",
                        column: x => x.TitleStructureId,
                        principalTable: "TitleStructures",
                        principalColumn: "Title",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Agencies_ParentSlug",
                table: "Agencies",
                column: "ParentSlug");

            migrationBuilder.CreateIndex(
                name: "IX_CfrReferences_AgencySlug",
                table: "CfrReferences",
                column: "AgencySlug");

            migrationBuilder.CreateIndex(
                name: "IX_CorrectionReferences_CorrectionId",
                table: "CorrectionReferences",
                column: "CorrectionId");

            migrationBuilder.CreateIndex(
                name: "IX_StructureNodes_StructureNodeId",
                table: "StructureNodes",
                column: "StructureNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_StructureNodes_TitleStructureId",
                table: "StructureNodes",
                column: "TitleStructureId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CfrReferences");

            migrationBuilder.DropTable(
                name: "CorrectionCounts");

            migrationBuilder.DropTable(
                name: "CorrectionReferences");

            migrationBuilder.DropTable(
                name: "DescendantRanges");

            migrationBuilder.DropTable(
                name: "StructureNodes");

            migrationBuilder.DropTable(
                name: "Agencies");

            migrationBuilder.DropTable(
                name: "Corrections");

            migrationBuilder.DropTable(
                name: "TitleStructures");
        }
    }
}
