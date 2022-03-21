using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReserveBlockExplorer.Migrations
{
    public partial class ValInfoBanreport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BanReport",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BannedAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReporterAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReportDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BanReport", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ValidatorInfo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsBanned = table.Column<bool>(type: "bit", nullable: false),
                    IsOnline = table.Column<bool>(type: "bit", nullable: false),
                    LastChecked = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FailChecks = table.Column<int>(type: "int", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ValidatorId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValidatorInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ValidatorInfo_Validator_ValidatorId",
                        column: x => x.ValidatorId,
                        principalTable: "Validator",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ValidatorInfo_ValidatorId",
                table: "ValidatorInfo",
                column: "ValidatorId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BanReport");

            migrationBuilder.DropTable(
                name: "ValidatorInfo");
        }
    }
}
