using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReserveBlockExplorer.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Block",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Height = table.Column<long>(type: "bigint", nullable: false),
                    ChainRefId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<long>(type: "bigint", nullable: false),
                    Hash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrevHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MerkleRoot = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StateRoot = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Validator = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ValidatorSignature = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NextValidators = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalReward = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    NumOfTx = table.Column<int>(type: "int", nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    BCraftTime = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Block", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Transaction",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Hash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ToAddress = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    FromAddress = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Nonce = table.Column<long>(type: "bigint", nullable: false),
                    Fee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Timestamp = table.Column<long>(type: "bigint", nullable: false),
                    NFTData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Signature = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Height = table.Column<long>(type: "bigint", nullable: false),
                    BlockId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transaction_Block_BlockId",
                        column: x => x.BlockId,
                        principalTable: "Block",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_BlockId",
                table: "Transaction",
                column: "BlockId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Transaction");

            migrationBuilder.DropTable(
                name: "Block");
        }
    }
}
