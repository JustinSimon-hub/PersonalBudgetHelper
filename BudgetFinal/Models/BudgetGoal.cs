using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace BudgetFinal.Models
{
    public class BudgetGoal
    {
        public int Id { get; set; }
        [Required]
        public DateTime StartDate { get; set; } 
        public DateTime EndDate { get; set; }   
        [Required]
        public decimal LimitAmount { get; set; }

        //This is the property for alerting the user when the budget has been exceeded
        public int BudgetLimit { get; set; }
        

    }
}
