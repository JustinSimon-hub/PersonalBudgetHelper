using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetFinal.Migrations
{
    /// <inheritdoc />
   public partial class AddBudgetGoalIdToTransaction : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Add the nullable BudgetGoalId column
        migrationBuilder.AddColumn<int>(
            name: "BudgetGoalId",
            table: "Transactions",
            type: "INTEGER",
            nullable: true);

        // Create an index on the BudgetGoalId column
        migrationBuilder.CreateIndex(
            name: "IX_Transactions_BudgetGoalId",
            table: "Transactions",
            column: "BudgetGoalId");

        // Remove the foreign key temporarily (for data migration)
        // Add the constraint back later
        // migrationBuilder.AddForeignKey(
        //     name: "FK_Transactions_BudgetGoals_BudgetGoalId",
        //     table: "Transactions",
        //     column: "BudgetGoalId",
        //     principalTable: "BudgetGoals",
        //     principalColumn: "Id",
        //     onDelete: ReferentialAction.Cascade);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Transactions_BudgetGoals_BudgetGoalId",
            table: "Transactions");

        migrationBuilder.DropIndex(
            name: "IX_Transactions_BudgetGoalId",
            table: "Transactions");

        migrationBuilder.DropColumn(
            name: "BudgetGoalId",
            table: "Transactions");
    }
}

}
