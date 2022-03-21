using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReserveBlockExplorer.Migrations
{
    public partial class ValInfoBlockCounts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BlocksIn24Hours",
                table: "ValidatorInfo",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BlocksIn7Days",
                table: "ValidatorInfo",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlocksIn24Hours",
                table: "ValidatorInfo");

            migrationBuilder.DropColumn(
                name: "BlocksIn7Days",
                table: "ValidatorInfo");
        }
    }
}
