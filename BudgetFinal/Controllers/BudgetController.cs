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
    var user = await _userManager.GetUserAsync(User); // _userManager is assumed to be injected via constructor
    return user?.Id ?? string.Empty; // Return user ID or an empty string if no user is found
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


//Debugging neccesary to remove modelstate errors when calling thios action
 
[HttpPost]
public async Task<IActionResult> AddTransaction(BudgetViewModel model)
{
    // Check if the ModelState is valid
    if (!ModelState.IsValid)
    {
        // Log ModelState errors for debugging purposes
        Console.WriteLine("ModelState is invalid:");
        foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
        {
            Console.WriteLine($" - {error.ErrorMessage}");
        }
        // Return the Index view with the model to show validation errors
        return View("Index", model); 
    }

    // Retrieve the new transaction from the model
    var transaction = model.NewTransaction;

    // Set TransactionType based on Amount (Income if positive, Expense if negative)
    if (transaction.Amount >= 0)
    {
        transaction.TransactionType = "Income";
    }
    else if (transaction.Amount < 0)
    {
        transaction.TransactionType = "Expense";
    }
    else if(transaction.Amount == 0)
    {
        transaction.TransactionType = "Zero";
    }
    else
    {
        ModelState.AddModelError("NewTransaction.Amount", "Amount must not be zero.");
        return View("Index", model);
    }



    // Handle UserId (if User is authenticated, assign their ID, else assign "Anonymous")
    if (User.Identity.IsAuthenticated)
    {
        transaction.UserId = await GetCurrentUserId(); // Ensure this method returns the correct user ID
    }
    else
    {
        transaction.UserId ??= "Anonymous"; // For non-authenticated users, set the UserId to "Anonymous"
    }

    // Add the transaction to the context and save changes
    _context.Transactions.Add(transaction);
    await _context.SaveChangesAsync();

    // Redirect to the Index page after successfully adding the transaction
    return RedirectToAction("Index");
}










    //Update actions requires both a get and post method
    //one for retrieving the obj and one for submitting the changes to the server 
    //Loads the transaction to be updated/view
    [HttpGet]   
public IActionResult UpdateTransaction(int id)
{
    var transaction = _context.Transactions.Find(id);
    if (transaction == null)
    {
        return NotFound();
    }

    var model = new BudgetViewModel
    {
        NewTransaction = transaction // Bind the existing transaction to the view
    };

    return View(model);
}

    //Adding methods to update and delete transactions
    [HttpPost]
public IActionResult UpdateTransaction(BudgetViewModel model)
{
    if (ModelState.IsValid)
    {
        var transaction = _context.Transactions.Find(model.NewTransaction.Id);
        if (transaction == null)
        {
            return NotFound();
        }

        // Update properties
        transaction.Description = model.NewTransaction.Description;
        transaction.Amount = model.NewTransaction.Amount;
        transaction.Date = model.NewTransaction.Date;
        transaction.Category = model.NewTransaction.Category;

        // Automatically set the TransactionType based on Amount
        transaction.TransactionType = model.NewTransaction.Amount >= 0 ? "Income" : "Expense";

        // Save the updated transaction
        _context.Update(transaction);
        _context.SaveChanges();

        return RedirectToAction("Index");
    }

    // If ModelState is not valid, return the same model to the view
    return View(model);
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