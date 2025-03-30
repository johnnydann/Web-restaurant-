using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChieuT4_Nhom05_WebQLCF.Migrations
{
    /// <inheritdoc />
    public partial class updateQlDH : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QLDHId",
                table: "OrderDetails",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_QLDHId",
                table: "OrderDetails",
                column: "QLDHId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_QLDHs_QLDHId",
                table: "OrderDetails",
                column: "QLDHId",
                principalTable: "QLDHs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_QLDHs_QLDHId",
                table: "OrderDetails");

            migrationBuilder.DropIndex(
                name: "IX_OrderDetails_QLDHId",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "QLDHId",
                table: "OrderDetails");
        }
    }
}
