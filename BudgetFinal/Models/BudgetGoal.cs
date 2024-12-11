using System.ComponentModel.DataAnnotations;

namespace BudgetFinal.Models
{
    public class BudgetGoal
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal LimitAmount { get; set; }
        public string UserId { get; set; } // Optional, for user-specific goals
    }
}