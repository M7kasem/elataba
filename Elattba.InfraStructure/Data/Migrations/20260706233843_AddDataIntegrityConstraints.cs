using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Elattba.InfraStructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDataIntegrityConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Stores_Rating_Range",
                table: "Stores",
                sql: "[Rating] >= 0 AND [Rating] <= 5");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ShippingRates_Cost_NonNegative",
                table: "ShippingRates",
                sql: "[Cost] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Reviews_Rating_Range",
                table: "Reviews",
                sql: "[Rating] BETWEEN 1 AND 5");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Products_BasePrice_Positive",
                table: "Products",
                sql: "[BasePrice] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Products_StockQuantity_NonNegative",
                table: "Products",
                sql: "[StockQuantity] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PricingTiers_MinQuantity_Positive",
                table: "PricingTiers",
                sql: "[MinQuantity] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PricingTiers_PricePerUnit_Positive",
                table: "PricingTiers",
                sql: "[PricePerUnit] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Orders_ShippingCost_NonNegative",
                table: "Orders",
                sql: "[ShippingCost] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Orders_TotalAmount_NonNegative",
                table: "Orders",
                sql: "[TotalAmount] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderItems_Quantity_Positive",
                table: "OrderItems",
                sql: "[Quantity] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderItems_Subtotal_NonNegative",
                table: "OrderItems",
                sql: "[Subtotal] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderItems_UnitPrice_Positive",
                table: "OrderItems",
                sql: "[UnitPrice] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Offers_Date_Range",
                table: "Offers",
                sql: "[StartDate] < [EndDate]");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Offers_DiscountPercentage_Range",
                table: "Offers",
                sql: "[DiscountPercentage] > 0 AND [DiscountPercentage] <= 100");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Stores_Rating_Range",
                table: "Stores");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ShippingRates_Cost_NonNegative",
                table: "ShippingRates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Reviews_Rating_Range",
                table: "Reviews");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Products_BasePrice_Positive",
                table: "Products");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Products_StockQuantity_NonNegative",
                table: "Products");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PricingTiers_MinQuantity_Positive",
                table: "PricingTiers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PricingTiers_PricePerUnit_Positive",
                table: "PricingTiers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Orders_ShippingCost_NonNegative",
                table: "Orders");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Orders_TotalAmount_NonNegative",
                table: "Orders");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderItems_Quantity_Positive",
                table: "OrderItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderItems_Subtotal_NonNegative",
                table: "OrderItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderItems_UnitPrice_Positive",
                table: "OrderItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Offers_Date_Range",
                table: "Offers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Offers_DiscountPercentage_Range",
                table: "Offers");
        }
    }
}
