using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_commerce.v1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAhamoveOrderFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AhamoveLastStatus",
                table: "Orders",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AhamoveOrderId",
                table: "Orders",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_AhamoveOrderId",
                table: "Orders",
                column: "AhamoveOrderId",
                unique: true,
                filter: "[AhamoveOrderId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_AhamoveOrderId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AhamoveLastStatus",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AhamoveOrderId",
                table: "Orders");
        }
    }
}
