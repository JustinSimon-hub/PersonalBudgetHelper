using System.ComponentModel.DataAnnotations;

namespace BudgetFinal.Models
{
    public class BudgetViewModel
    {
        //Proprties used in the views
  
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal Balance { get; set; }
    public List<Transaction> Transactions { get; set; } = new List<Transaction>();
    public Transaction NewTransaction { get; set; } = new Transaction();


     public decimal MinimumBudgetThreshold { get; set; }

    
    }
}





