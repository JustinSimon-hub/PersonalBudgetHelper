
namespace BudgetFinal.Models
{
    public class ReportViewModel
    {
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal Balance { get; set; }
    public List<CategorySummary> CategoryBreakdown { get; set; }
    }
}