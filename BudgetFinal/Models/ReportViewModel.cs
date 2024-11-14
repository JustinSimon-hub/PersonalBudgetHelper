
namespace BudgetFinal.Models
{
    public class ReportViewModel
    {
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal Balance { get; set; }
    public List<CategorySummary> CategoryBreakdown { get; set; }

    public string ReportTitle { get; set; } // For example, "Monthly Report", "Quarterly Report"
    public string UserId { get; set; }
    }
}