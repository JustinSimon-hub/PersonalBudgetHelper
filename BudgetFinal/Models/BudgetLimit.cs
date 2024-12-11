namespace BudgetFinal.Models
{

    public class BudgetLimit
    {
        public int Id { get; set; }
        public decimal MonthlyLimit { get; set; } // Monthly spending limit
        public decimal YearlyLimit { get; set; }  // Yearly spending limit
    }
}