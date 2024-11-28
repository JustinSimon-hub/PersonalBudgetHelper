using Microsoft.AspNetCore.Mvc;
using System.Linq;
using BudgetFinal.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using BudgetFinal.Services;



public class BudgetController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IReportService _reportService;

 public BudgetController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IReportService reportService)    
    {
        _context = context;  
        _userManager = userManager;
        _reportService = reportService;
    }

    // Private helper method to get the current user's ID
    private async Task<string> GetCurrentUserId()
    {
        var user = await _userManager.GetUserAsync(User);
        return user?.Id;
    }

   private decimal CalculateTotalIncome()
{
    // Get all transactions from the database
    var transactions = _context.Transactions.ToList();
    
    // Sum the amounts where the transaction type is Income
    return transactions.Where(t => t.TransactionType == "Income").Sum(t => (decimal)t.Amount);
}

private decimal CalculateTotalExpenses()
{
    // Get all transactions from the database
    var transactions = _context.Transactions.ToList();
    
    // Sum the amounts where the transaction type is Expense
    return transactions.Where(t => t.TransactionType == "Expense").Sum(t => (decimal)t.Amount);
}

private decimal CalculateBalance()
{
    // Get all transactions from the database
    var transactions = _context.Transactions.ToList();
    
    // Calculate the balance as Total Income - Total Expenses
    return CalculateTotalIncome() - CalculateTotalExpenses();
}


    // Index action to load the dashboard view
    public IActionResult Index()
    {
        var model = new BudgetViewModel
        {
            TotalIncome = CalculateTotalIncome(),
            TotalExpenses = CalculateTotalExpenses(),
            Balance = CalculateBalance(),
            Transactions = _context.Transactions.ToList(),
            NewTransaction = new Transaction() // Initialize NewTransaction for form binding
        };

        return View(model);
    }



[HttpPost]
public async Task<IActionResult> AddTransaction(BudgetViewModel model)
{
    if (!ModelState.IsValid)
    {
        // Log ModelState errors for debugging
        Console.WriteLine("ModelState is invalid:");
        foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
        {
            Console.WriteLine($" - {error.ErrorMessage}");
        }
        return View(model); // Ensure you return the same model to display errors in the view
    }

    var transaction = model.NewTransaction;

    // If UserId is required by your database schema, handle it conditionally
    if (User.Identity.IsAuthenticated)
    {
        transaction.UserId = await GetCurrentUserId();
    }
    else
    {
        transaction.UserId ??= "Anonymous"; // Optional value for non-authenticated users
    }

    // Add the transaction and save changes
    _context.Transactions.Add(transaction);
    await _context.SaveChangesAsync();

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
        .Where(t => t.UserId == userId.ToString() && t.TransactionType == "Income" &&
                    t.Date.Month == month && t.Date.Year == year)
        .SumAsync(t => t.Amount);

    report.TotalExpenses = await _context.Transactions
        .Where(t => t.UserId == userId.ToString() && t.TransactionType == "Expense" &&
                    t.Date.Month == month && t.Date.Year == year)
        .SumAsync(t => t.Amount);

    report.Balance = report.TotalIncome - report.TotalExpenses;

    // Breakdown by Category
    report.CategoryBreakdown = await _context.Transactions
        .Where(t => t.UserId == userId.ToString() && t.Date.Month == month && t.Date.Year == year)
        .GroupBy(t => t.Category)
        .Select(g => new CategorySummary
        {
            Category = g.Key,
            TotalAmount = g.Sum(t => t.Amount)
        }).ToListAsync();

    return report;
}

    

[HttpGet]
// In your BudgetController.cs
[HttpGet]
public async Task<IActionResult> GenerateReport(string reportType, int month = 0, int year = 0)
{
    var userId = await GetCurrentUserId(); // Assuming this returns the logged-in user's ID
    ReportViewModel reportViewModel = new ReportViewModel(); // Or you can use ReportViewModel if you're focusing on reports

    if (reportType == "Monthly" && month > 0 && year > 0)
    {
        reportViewModel.ReportTitle = $"Monthly Report for {month}/{year}";
        reportViewModel.TotalIncome = await _reportService.GetTotalIncome(userId, month, year);
        reportViewModel.TotalExpenses = await _reportService.GetTotalExpenses(userId, month, year);
    }
    else if (reportType == "Quarterly" && year > 0)
    {
        reportViewModel.ReportTitle = $"Quarterly Report for {year}";
        reportViewModel.TotalIncome = await _reportService.GetTotalIncomeForQuarter(userId, year);
        reportViewModel.TotalExpenses = await _reportService.GetTotalExpensesForQuarter(userId, year);
    }
    else if (reportType == "Yearly" && year > 0)
    {
        reportViewModel.ReportTitle = $"Yearly Report for {year}";
        reportViewModel.TotalIncome = await _reportService.GetTotalIncomeForYear(userId, year);
        reportViewModel.TotalExpenses = await _reportService.GetTotalExpensesForYear(userId, year);
    }

    return View("Report", reportViewModel); // Use your report view to display this data
}










}