using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace BudgetFinal.Models
{
    public class BudgetGoal
    {
        public int Id { get; set; }
        //This is the property for alerting the user when the budget has been exceeded
        public decimal BudgetLimit { get; set; }
        //these two properties are for the start and end date of the budget
        public DateTime StartDate { get; set; } 
        public DateTime EndDate { get; set; }   


        

    }
}

