using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_commerce.v1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdvancedPromotionRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PromotionDiscount",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "PromotionRuleId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PromotionSummary",
                table: "Orders",
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AppliedPromotionRuleId",
                table: "Carts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PromotionDiscountPreview",
                table: "Carts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PromotionSummary",
                table: "Carts",
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PromotionRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotionRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PromotionBuyXGetYActions",
                columns: table => new
                {
                    PromotionRuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuyProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BuyCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BuyQty = table.Column<int>(type: "int", nullable: false),
                    GetProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GetCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GetQty = table.Column<int>(type: "int", nullable: false),
                    LimitPerOrder = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotionBuyXGetYActions", x => x.PromotionRuleId);
                    table.ForeignKey(
                        name: "FK_PromotionBuyXGetYActions_Categories_BuyCategoryId",
                        column: x => x.BuyCategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PromotionBuyXGetYActions_Categories_GetCategoryId",
                        column: x => x.GetCategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PromotionBuyXGetYActions_Products_BuyProductId",
                        column: x => x.BuyProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PromotionBuyXGetYActions_Products_GetProductId",
                        column: x => x.GetProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PromotionBuyXGetYActions_PromotionRules_PromotionRuleId",
                        column: x => x.PromotionRuleId,
                        principalTable: "PromotionRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PromotionPercentageActions",
                columns: table => new
                {
                    PromotionRuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Percent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Target = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotionPercentageActions", x => x.PromotionRuleId);
                    table.ForeignKey(
                        name: "FK_PromotionPercentageActions_PromotionRules_PromotionRuleId",
                        column: x => x.PromotionRuleId,
                        principalTable: "PromotionRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PromotionRuleCategories",
                columns: table => new
                {
                    PromotionRuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotionRuleCategories", x => new { x.PromotionRuleId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_PromotionRuleCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PromotionRuleCategories_PromotionRules_PromotionRuleId",
                        column: x => x.PromotionRuleId,
                        principalTable: "PromotionRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PromotionRuleProducts",
                columns: table => new
                {
                    PromotionRuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotionRuleProducts", x => new { x.PromotionRuleId, x.ProductId });
                    table.ForeignKey(
                        name: "FK_PromotionRuleProducts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PromotionRuleProducts_PromotionRules_PromotionRuleId",
                        column: x => x.PromotionRuleId,
                        principalTable: "PromotionRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PromotionRuleId",
                table: "Orders",
                column: "PromotionRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_AppliedPromotionRuleId",
                table: "Carts",
                column: "AppliedPromotionRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionBuyXGetYActions_BuyCategoryId",
                table: "PromotionBuyXGetYActions",
                column: "BuyCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionBuyXGetYActions_BuyProductId",
                table: "PromotionBuyXGetYActions",
                column: "BuyProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionBuyXGetYActions_GetCategoryId",
                table: "PromotionBuyXGetYActions",
                column: "GetCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionBuyXGetYActions_GetProductId",
                table: "PromotionBuyXGetYActions",
                column: "GetProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionRuleCategories_CategoryId",
                table: "PromotionRuleCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionRuleProducts_ProductId",
                table: "PromotionRuleProducts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionRules_IsActive_StartDate_EndDate_Priority",
                table: "PromotionRules",
                columns: new[] { "IsActive", "StartDate", "EndDate", "Priority" });

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_PromotionRules_AppliedPromotionRuleId",
                table: "Carts",
                column: "AppliedPromotionRuleId",
                principalTable: "PromotionRules",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_PromotionRules_PromotionRuleId",
                table: "Orders",
                column: "PromotionRuleId",
                principalTable: "PromotionRules",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carts_PromotionRules_AppliedPromotionRuleId",
                table: "Carts");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_PromotionRules_PromotionRuleId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "PromotionBuyXGetYActions");

            migrationBuilder.DropTable(
                name: "PromotionPercentageActions");

            migrationBuilder.DropTable(
                name: "PromotionRuleCategories");

            migrationBuilder.DropTable(
                name: "PromotionRuleProducts");

            migrationBuilder.DropTable(
                name: "PromotionRules");

            migrationBuilder.DropIndex(
                name: "IX_Orders_PromotionRuleId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Carts_AppliedPromotionRuleId",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "PromotionDiscount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PromotionRuleId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PromotionSummary",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AppliedPromotionRuleId",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "PromotionDiscountPreview",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "PromotionSummary",
                table: "Carts");
        }
    }
}
