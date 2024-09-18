namespace BudgetFinal.Models
{
    public class Transaction
    {
    public int Id { get; set; }
    public string Description { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }//going to be pos for income and neg for expense 

    

    }
}