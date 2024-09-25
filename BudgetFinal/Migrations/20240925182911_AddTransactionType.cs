using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetFinal.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TransactionType",
                table: "Transactions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

                // SQL commands to update existing records
        migrationBuilder.Sql("UPDATE Transactions SET TransactionType = 'Income' WHERE Amount > 0");
        migrationBuilder.Sql("UPDATE Transactions SET TransactionType = 'Expense' WHERE Amount <= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TransactionType",
                table: "Transactions");
        }
    }
}
