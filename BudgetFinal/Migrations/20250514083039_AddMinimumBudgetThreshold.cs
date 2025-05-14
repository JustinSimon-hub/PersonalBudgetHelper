using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetFinal.Migrations
{
    /// <inheritdoc />
    public partial class AddMinimumBudgetThreshold : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MinimumBudgetThreshold",
                table: "BudgetGoals",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinimumBudgetThreshold",
                table: "BudgetGoals");
        }
    }
}
