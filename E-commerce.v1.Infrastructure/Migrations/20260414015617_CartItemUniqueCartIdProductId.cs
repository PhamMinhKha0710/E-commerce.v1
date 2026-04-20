using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_commerce.v1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CartItemUniqueCartIdProductId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE t SET Quantity = s.TotalQty
                FROM CartItems AS t
                INNER JOIN (
                    SELECT CartId, ProductId, MIN(Id) AS MinId, SUM(Quantity) AS TotalQty
                    FROM CartItems
                    GROUP BY CartId, ProductId
                    HAVING COUNT(*) > 1
                ) AS s ON t.CartId = s.CartId AND t.ProductId = s.ProductId AND t.Id = s.MinId;

                DELETE ci
                FROM CartItems AS ci
                WHERE ci.Id NOT IN (
                    SELECT MIN(Id) FROM CartItems AS c GROUP BY c.CartId, c.ProductId
                );
                """);

            migrationBuilder.DropIndex(
                name: "IX_CartItems_CartId",
                table: "CartItems");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId_ProductId",
                table: "CartItems",
                columns: new[] { "CartId", "ProductId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CartItems_CartId_ProductId",
                table: "CartItems");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId",
                table: "CartItems",
                column: "CartId");
        }
    }
}
