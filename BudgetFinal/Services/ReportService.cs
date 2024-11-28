using System.Linq;
using System.Threading.Tasks;
using BudgetFinal.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetFinal.Services
{
        public class ReportService : IReportService
        {
            private readonly ApplicationDbContext _context;

            public ReportService(ApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<ReportViewModel> GetMonthlyReport(string userId, int month, int year)
            {
                var transactions = await _context.Transactions
                    .Where(t => t.UserId == userId && t.Date.Month == month && t.Date.Year == year)
                    .ToListAsync();

                var totalIncome = transactions.Where(t => t.TransactionType == "Income").Sum(t => t.Amount);
                var totalExpenses = transactions.Where(t => t.TransactionType == "Expense").Sum(t => t.Amount);

                return new ReportViewModel
                {
                    TotalIncome = totalIncome,
                    TotalExpenses = totalExpenses,
                    ReportTitle = $"Monthly Report for {month}/{year}",
                    UserId = userId
                };
            }

            public async Task<ReportViewModel> GetQuarterlyReport(string userId, int year)
            {
                var transactions = await _context.Transactions
                    .Where(t => t.UserId == userId && t.Date.Year == year)
                    .ToListAsync();

                // For simplicity, let's assume you split the quarters by months
                var totalIncome = transactions.Where(t => t.TransactionType == "Income").Sum(t => t.Amount);
                var totalExpenses = transactions.Where(t => t.TransactionType == "Expense").Sum(t => t.Amount);

                return new ReportViewModel
                {
                    TotalIncome = totalIncome,
                    TotalExpenses = totalExpenses,
                    ReportTitle = $"Quarterly Report for {year}",
                    UserId = userId
                };
            }

            public async Task<ReportViewModel> GetYearlyReport(string userId, int year)
            {
                var transactions = await _context.Transactions
                    .Where(t => t.UserId == userId && t.Date.Year == year)
                    .ToListAsync();

                var totalIncome = transactions.Where(t => t.TransactionType == "Income").Sum(t => t.Amount);
                var totalExpenses = transactions.Where(t => t.TransactionType == "Expense").Sum(t => t.Amount);

                return new ReportViewModel
                {
                    TotalIncome = totalIncome,
                    TotalExpenses = totalExpenses,
                    ReportTitle = $"Yearly Report for {year}",
                    UserId = userId
                };
            }

                public async Task<decimal> GetTotalIncome(string userId, int month, int year)
            {
                return await _context.Transactions
                    .Where(t => t.UserId == userId.ToString() && t.TransactionType == "Income" && t.Date.Month == month && t.Date.Year == year)
                    .SumAsync(t => t.Amount);
            }

            public async Task<decimal> GetTotalExpenses(string userId, int month, int year)
            {
                return await _context.Transactions
                    .Where(t => t.UserId == userId.ToString() && t.TransactionType == "Expense" && t.Date.Month == month && t.Date.Year == year)
                    .SumAsync(t => t.Amount);
            }

                public async Task<decimal> GetTotalIncomeForQuarter(string userId, int year)
        {
            // Example logic for a quarter (e.g., Q1: January, February, March)
            return await _context.Transactions
                .Where(t => t.UserId ==  userId.ToString() && t.TransactionType == "Income" && 
                            t.Date.Year == year && t.Date.Month >= 1 && t.Date.Month <= 3)
                .SumAsync(t => t.Amount);
        }

        public async Task<decimal> GetTotalExpensesForQuarter(string userId, int year)
        {
            return await _context.Transactions
                .Where(t => t.UserId == userId.ToString() && t.TransactionType == "Expense" && 
                            t.Date.Year == year && t.Date.Month >= 1 && t.Date.Month <= 3)
                .SumAsync(t => t.Amount);
        }

        public async Task<decimal> GetTotalIncomeForYear(string userId, int year)
        {
            return await _context.Transactions
                .Where(t => t.UserId ==  userId.ToString() && t.TransactionType == "Income" && 
                            t.Date.Year == year)
                .SumAsync(t => t.Amount);
        }

        public async Task<decimal> GetTotalExpensesForYear(string userId, int year)
        {
            return await _context.Transactions
                .Where(t => t.UserId ==  userId.ToString() && t.TransactionType == "Expense" && 
                            t.Date.Year == year)
                .SumAsync(t => t.Amount);
        }
    }

}
    


