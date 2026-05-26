using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OrderFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "currency",
                columns: table => new
                {
                    code = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                    symbol = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_currency", x => x.code);
                });

            migrationBuilder.CreateTable(
                name: "product",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    available_quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product", x => x.id);
                    table.CheckConstraint("chk_product_price_positive", "unit_price >= 0");
                    table.CheckConstraint("chk_product_stock_positive", "available_quantity >= 0");
                });

            migrationBuilder.CreateTable(
                name: "sales_order_status",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sales_order_status", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sales_order",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    customer_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    currency_code = table.Column<string>(type: "character(3)", nullable: false),
                    sales_order_status_id = table.Column<int>(type: "integer", nullable: false),
                    total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sales_order", x => x.id);
                    table.CheckConstraint("chk_sales_order_total_positive", "total >= 0");
                    table.ForeignKey(
                        name: "fk_sales_order_currency_currency_code",
                        column: x => x.currency_code,
                        principalTable: "currency",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_sales_order_sales_order_status_sales_order_status_id",
                        column: x => x.sales_order_status_id,
                        principalTable: "sales_order_status",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sales_order_items",
                columns: table => new
                {
                    sales_order_id = table.Column<long>(type: "bigint", nullable: false),
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sales_order_items", x => new { x.sales_order_id, x.product_id });
                    table.CheckConstraint("chk_price_positive", "unit_price >= 0");
                    table.CheckConstraint("chk_quantity_positive", "quantity > 0");
                    table.ForeignKey(
                        name: "fk_sales_order_items_products_product_id",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_sales_order_items_sales_orders_sales_order_id",
                        column: x => x.sales_order_id,
                        principalTable: "sales_order",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "currency",
                columns: new[] { "code", "name", "symbol" },
                values: new object[,]
                {
                    { "BRL", "Real Brasileiro", "R$" },
                    { "EUR", "Euro", "€" },
                    { "USD", "United States Dollar", "$" }
                });

            migrationBuilder.InsertData(
                table: "sales_order_status",
                columns: new[] { "id", "description", "name" },
                values: new object[,]
                {
                    { 1, "Pedido de venda recebido pelo sistema e aguardando processamento.", "Placed" },
                    { 2, "Pagamento aprovado e pedido de venda confirmado.", "Confirmed" },
                    { 3, "Pedido de venda cancelado.", "Canceled" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_sales_order_currency_code",
                table: "sales_order",
                column: "currency_code");

            migrationBuilder.CreateIndex(
                name: "ix_sales_order_sales_order_status_id",
                table: "sales_order",
                column: "sales_order_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_sales_order_items_product_id",
                table: "sales_order_items",
                column: "product_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sales_order_items");

            migrationBuilder.DropTable(
                name: "product");

            migrationBuilder.DropTable(
                name: "sales_order");

            migrationBuilder.DropTable(
                name: "currency");

            migrationBuilder.DropTable(
                name: "sales_order_status");
        }
    }
}
