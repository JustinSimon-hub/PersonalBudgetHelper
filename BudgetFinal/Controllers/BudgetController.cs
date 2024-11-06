using Microsoft.AspNetCore.Mvc;
using System.Linq;
using BudgetFinal.Models;
using Microsoft.EntityFrameworkCore;



public class BudgetController : Controller
{
    private readonly ApplicationDbContext _context;

    public BudgetController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
{
    // Fetch all transactions from the database
    var transactions = _context.Transactions.ToList();
    
    // Calculate total income and expenses
    var totalIncome = transactions.Where(t => t.Amount > 0).Sum(t => t.Amount);
    var totalExpenses = transactions.Where(t => t.Amount < 0).Sum(t => t.Amount);
    var balance = totalIncome + totalExpenses;

    // Create a list of view models for displaying transactions with type labels
    var transactionViewModels = transactions.Select(t => new Transaction
    {
        Id = t.Id,
        Description = t.Description,
        Amount = t.Amount,
        Date = t.Date
    }).ToList();

    // Prepare the view model with the transactions and totals
    var viewModel = new BudgetViewModel
    {
        Transactions = transactionViewModels, // Refactored to use TransactionViewModel list
        TotalIncome = totalIncome,
        TotalExpenses = totalExpenses,
        Balance = balance
    };

    return View(viewModel);
}


    [HttpPost]
    public IActionResult AddTransaction(Transaction transaction)
    {
        if (ModelState.IsValid)
        {
            
            _context.Transactions.Add(transaction);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        return RedirectToAction("Index");
    }


        //Update actions requires both a get and post method
        //one for retrieving the obj and one for submitting the changes to the server 
    //Loads the transaction to be updated/view
    [HttpGet]   
    public IActionResult UpdateTransaction(int id)
    {
        var transaction = _context.Transactions.Find(id);
        if(transaction == null)
        {
            return NotFound();
        }
        return View(transaction);
    }
    //Adding methods to update and delete transactions
    [HttpPost]
    public IActionResult UpdateTransaction(Transaction transaction)
    {
        if (ModelState.IsValid)
        {
            _context.Transactions.Update(transaction);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        return View(transaction);
    }




   [HttpPost]
public IActionResult DeleteTransaction(int id)
{
    var transaction = _context.Transactions.Find(id);
    if (transaction == null)
    {
        return NotFound();
    }

    _context.Transactions.Remove(transaction);
    _context.SaveChanges();

    return RedirectToAction("Index");
}

//Actions for filtering transactions
public async Task<ReportViewModel> GetMonthlyReport(int userId, int month, int year)
{
    var report = new ReportViewModel();
    //Get income and expenses for the specified month and year
    report.TotalIncome = await _context.Transactions
        .Where(t => t.UserId == userId && t.TransactionType == "Income" &&
                    t.Date.Month == month && t.Date.Year == year)
        .SumAsync(t => t.Amount);

    report.TotalExpenses = await _context.Transactions
        .Where(t => t.UserId == userId && t.TransactionType == "Expense" &&
                    t.Date.Month == month && t.Date.Year == year)
        .SumAsync(t => t.Amount);

    report.Balance = report.TotalIncome - report.TotalExpenses;

    // Breakdown by Category
    report.CategoryBreakdown = await _context.Transactions
        .Where(t => t.UserId == userId && t.Date.Month == month && t.Date.Year == year)
        .GroupBy(t => t.Category)
        .Select(g => new CategorySummary
        {
            Category = g.Key,
            TotalAmount = g.Sum(t => t.Amount)
        }).ToListAsync();

    return report;
}

    

[HttpGet]
public async Task<IActionResult> GenerateReport(string reportType, int month = 0, int year = 0)
{
    var userId = GetCurrentUserId(); // Assume a method to get the logged-in user's ID
    ReportViewModel report = null;

    if (reportType == "Monthly" && month > 0 && year > 0)
    {
        report = await _reportService.GetMonthlyReport(userId, month, year);
    }
    else if (reportType == "Quarterly" && year > 0)
    {
        report = await _reportService.GetQuarterlyReport(userId, year);
    }
    else if (reportType == "Yearly" && year > 0)
    {
        report = await _reportService.GetYearlyReport(userId, year);
    }

    return View("Report", report);
}






//Category filtering actions 
//  public IActionResult TransactionsByCategory(int categoryId)
//     {
//         var category = _context.Categories.Include(c => c.Transactions)
//                                           .FirstOrDefault(c => c.Id == categoryId);
//         if (category == null)
//         {
//             return NotFound();
//         }

//         var viewModel = new BudgetViewModel
//         {
//             Category = category,
//             Transactions = category.Transactions.ToList()
//         };

//         return View(viewModel);
//     }

}