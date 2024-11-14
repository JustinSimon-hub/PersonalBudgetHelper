using BudgetFinal.Models;

namespace BudgetFinal.Services
{
    public interface IReportService
    {
        Task<ReportViewModel> GetMonthlyReport(string userId, int month, int year);
        Task<ReportViewModel> GetQuarterlyReport(string userId, int year);
        Task<ReportViewModel> GetYearlyReport(string userId, int year);
    }
}

