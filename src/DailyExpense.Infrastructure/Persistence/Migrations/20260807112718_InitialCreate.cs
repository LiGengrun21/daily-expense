using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DailyExpense.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    icon = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "expenses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    expense_date = table.Column<DateOnly>(type: "date", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_method = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expenses", x => x.id);
                    table.CheckConstraint("ck_expenses_amount", "amount > 0");
                    table.ForeignKey(
                        name: "FK_expenses_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "monthly_budgets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    month = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_monthly_budgets", x => x.id);
                    table.CheckConstraint("ck_monthly_budgets_amount", "amount > 0");
                    table.CheckConstraint("ck_monthly_budgets_month", "\"month\" >= 1 AND \"month\" <= 12");
                    table.ForeignKey(
                        name: "FK_monthly_budgets_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "categories",
                columns: new[] { "id", "color", "created_at", "description", "icon", "is_default", "name", "updated_at" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "#2F855A", new DateTimeOffset(new DateTime(2026, 8, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Food and groceries", "utensils", true, "Food", null },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "#2B6CB0", new DateTimeOffset(new DateTime(2026, 8, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Public transport, fuel, rides", "car", true, "Transport", null },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "#805AD5", new DateTimeOffset(new DateTime(2026, 8, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Rent, utilities, home costs", "home", true, "Housing", null },
                    { new Guid("44444444-4444-4444-4444-444444444444"), "#D69E2E", new DateTimeOffset(new DateTime(2026, 8, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Clothes, household items, online shopping", "shopping-bag", true, "Shopping", null },
                    { new Guid("55555555-5555-5555-5555-555555555555"), "#C53030", new DateTimeOffset(new DateTime(2026, 8, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Movies, games, subscriptions", "ticket", true, "Entertainment", null },
                    { new Guid("66666666-6666-6666-6666-666666666666"), "#319795", new DateTimeOffset(new DateTime(2026, 8, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Medical, pharmacy, fitness", "heart-pulse", true, "Health", null },
                    { new Guid("77777777-7777-7777-7777-777777777777"), "#4C51BF", new DateTimeOffset(new DateTime(2026, 8, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Books, courses, learning", "graduation-cap", true, "Education", null },
                    { new Guid("88888888-8888-8888-8888-888888888888"), "#718096", new DateTimeOffset(new DateTime(2026, 8, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Uncategorized expenses", "circle-help", true, "Other", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_categories_name",
                table: "categories",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_expenses_category_id",
                table: "expenses",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_expenses_category_id_expense_date",
                table: "expenses",
                columns: new[] { "category_id", "expense_date" });

            migrationBuilder.CreateIndex(
                name: "IX_expenses_expense_date",
                table: "expenses",
                column: "expense_date");

            migrationBuilder.CreateIndex(
                name: "IX_monthly_budgets_category_id",
                table: "monthly_budgets",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_monthly_budgets_year_month",
                table: "monthly_budgets",
                columns: new[] { "year", "month" },
                unique: true,
                filter: "category_id IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_monthly_budgets_year_month_category_id",
                table: "monthly_budgets",
                columns: new[] { "year", "month", "category_id" },
                unique: true,
                filter: "category_id IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "expenses");

            migrationBuilder.DropTable(
                name: "monthly_budgets");

            migrationBuilder.DropTable(
                name: "categories");
        }
    }
}
