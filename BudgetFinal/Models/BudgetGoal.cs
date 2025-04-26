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

        //This property is being used to enable the alert system used in the frontend
        public decimal MinimumBudgetThreshold { get; set; }
        

    }
}
