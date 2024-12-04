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
    private readonly ILogger<BudgetController> _logger;


 public BudgetController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IReportService reportService, ILogger<BudgetController> logger)    
    {
        _context = context;  
        _userManager = userManager;
        _logger = logger;
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
    //Logs incoming model to check if its populated correctly
 _logger.LogInformation("UpdateTransaction called with transaction ID: {Id}, Amount: {Amount}, Category: {Category}", 
        model.NewTransaction.Id, model.NewTransaction.Amount, model.NewTransaction.Category);

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
    if (transaction.Amount > 0)
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
    // Attempt to find the existing transaction in the database
    var existingTransaction = _context.Transactions.Find(id);
    if (existingTransaction == null)
    {
        // If no matching transaction is found, return a NotFound result
        return NotFound($"Transaction with ID {id} not found.");
    }

    // Create a new BudgetViewModel with the existing transaction
    var model = new BudgetViewModel
    {
        NewTransaction = existingTransaction
    };

    // Return the UpdateTransaction view with the model
    return View(model);
}

//Post method for updating the transaction after changes have been made
[HttpPost]
public IActionResult UpdateTransaction(BudgetViewModel model)
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
        // Return the UpdateTransaction view with the model to show validation errors
        return View(model);
    }

    // Attempt to find the existing transaction in the database
    var existingTransaction = _context.Transactions.Find(model.NewTransaction.Id);
    if (existingTransaction == null)
    {
        // If no matching transaction is found, return a NotFound result
        return NotFound($"Transaction with ID {model.NewTransaction.Id} not found.");
    }

    // Update the existing transaction's properties
    existingTransaction.Description = model.NewTransaction.Description;
    existingTransaction.Amount = model.NewTransaction.Amount;
    existingTransaction.Date = model.NewTransaction.Date;
    existingTransaction.Category = model.NewTransaction.Category;

    // Set TransactionType based on Amount (Income if positive, Expense if negative)
    if (existingTransaction.Amount > 0)
    {
        existingTransaction.TransactionType = "Income";
    }
    else if (existingTransaction.Amount < 0)
    {
        existingTransaction.TransactionType = "Expense";
    }
    else
    {
        ModelState.AddModelError("NewTransaction.Amount", "Amount must not be zero.");
        return View(model);
    }

    // Handle UserId (if User is authenticated, assign their ID, else assign "Anonymous")
    if (User.Identity.IsAuthenticated)
    {
        existingTransaction.UserId = GetCurrentUserId().Result; // Ensure this method returns the correct user ID
    }
    else
    {
        existingTransaction.UserId ??= "Anonymous"; // For non-authenticated users, set the UserId to "Anonymous"
    }

    // Save changes to the database
    _context.SaveChanges();

    // Redirect to the Index page after successfully updating the transaction
    return RedirectToAction("Index");
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


public IActionResult FilterTransactions(TransactionFilterViewModel model)
{
    // Start with all transactions from the database
    var transactions = _context.Transactions.AsQueryable();

    // Filter by category if provided
    if (!string.IsNullOrEmpty(model.Category))
    {
        transactions = transactions.Where(t => t.Category.Contains(model.Category));
    }

    // Filter by date range if provided
    if (model.StartDate.HasValue)
    {
        transactions = transactions.Where(t => t.Date >= model.StartDate.Value);
    }

    if (model.EndDate.HasValue)
    {
        transactions = transactions.Where(t => t.Date <= model.EndDate.Value);
    }

    // Assign the filtered results to the model
    model.FilteredTransactions = transactions.ToList();

    // Return the filtered view
    return View(model);
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