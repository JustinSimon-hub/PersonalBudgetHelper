using BudgetFinal.Models;

namespace BudgetFinal.Services
{
    public interface IReportService
    {
        Task<ReportViewModel> GetMonthlyReport(string userId, int month, int year);
        Task<ReportViewModel> GetQuarterlyReport(string userId, int year);
        Task<decimal> GetTotalIncome(string userId, int month, int year);
        Task<decimal> GetTotalExpenses(string userId, int month, int year);
        Task<decimal> GetTotalIncomeForQuarter(string userId, int year);
        Task<decimal> GetTotalExpensesForQuarter(string userId, int year);
        Task<decimal> GetTotalIncomeForYear(string userId, int year);
        Task<decimal> GetTotalExpensesForYear(string userId, int year);
    }
}

