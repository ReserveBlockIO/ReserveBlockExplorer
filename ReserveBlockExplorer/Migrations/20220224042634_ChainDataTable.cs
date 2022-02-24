using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReserveBlockExplorer.Migrations
{
    public partial class ChainDataTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlockChainData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Height = table.Column<long>(type: "bigint", nullable: false),
                    TotalRBX = table.Column<long>(type: "bigint", nullable: false),
                    RBXMinted = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockChainData", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlockChainData");
        }
    }
}
