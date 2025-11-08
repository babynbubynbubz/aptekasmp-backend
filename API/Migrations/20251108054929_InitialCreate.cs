using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Boxes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GId = table.Column<string>(type: "text", nullable: false),
                    SerialNumber = table.Column<string>(type: "text", nullable: false),
                    InBoxRemaining = table.Column<double>(type: "double precision", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Boxes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Medkits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CrewId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medkits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReceivingLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BoxId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceivingLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReceivingLogs_Boxes_BoxId",
                        column: x => x.BoxId,
                        principalTable: "Boxes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DispensingLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BoxId = table.Column<int>(type: "integer", nullable: false),
                    MedkitId = table.Column<int>(type: "integer", nullable: false),
                    DispensingAmount = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DispensingLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DispensingLogs_Boxes_BoxId",
                        column: x => x.BoxId,
                        principalTable: "Boxes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DispensingLogs_Medkits_MedkitId",
                        column: x => x.MedkitId,
                        principalTable: "Medkits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Boxes_GId_SerialNumber",
                table: "Boxes",
                columns: new[] { "GId", "SerialNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DispensingLogs_BoxId",
                table: "DispensingLogs",
                column: "BoxId");

            migrationBuilder.CreateIndex(
                name: "IX_DispensingLogs_MedkitId",
                table: "DispensingLogs",
                column: "MedkitId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceivingLogs_BoxId",
                table: "ReceivingLogs",
                column: "BoxId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DispensingLogs");

            migrationBuilder.DropTable(
                name: "ReceivingLogs");

            migrationBuilder.DropTable(
                name: "Medkits");

            migrationBuilder.DropTable(
                name: "Boxes");
        }
    }
}
