using BudgetFinal.Models;

public class TransactionFilterViewModel
{
    public string Category { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<Transaction> FilteredTransactions { get; set; }
}
