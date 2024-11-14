using System.ComponentModel.DataAnnotations;

namespace BudgetFinal.Models
{
    public class BudgetViewModel
    {   
        //represents the data that I want to display in the view
    [Required]
    public List<Transaction> Transactions { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal Balance { get; set; }

    


    }
}





