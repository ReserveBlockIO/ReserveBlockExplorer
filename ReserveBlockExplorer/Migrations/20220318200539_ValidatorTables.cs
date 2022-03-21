using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReserveBlockExplorer.Migrations
{
    public partial class ValidatorTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Validator",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UniqueName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Position = table.Column<long>(type: "bigint", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EligibleBlockStart = table.Column<long>(type: "bigint", nullable: false),
                    Signature = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    FailCount = table.Column<int>(type: "int", nullable: false),
                    NodeIP = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NodeReferenceId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Validator", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Validator");
        }
    }
}
