using Microsoft.AspNetCore.Mvc;
using System.Linq;
using BudgetFinal.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using BudgetFinal.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
//App doesnt read the filter budget feature
public class BudgetController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IReportService _reportService;
    private readonly ILogger<BudgetController> _logger;

    public BudgetController(ApplicationDbContext context, 
                            UserManager<IdentityUser> userManager, 
                            IReportService reportService, 
                            ILogger<BudgetController> logger)    
    {
        _context = context;  
        _userManager = userManager;
        _reportService = reportService;
        _logger = logger;
    }

    // Private helper method to get the current user's ID
    private async Task<string> GetCurrentUserId()
    {
        var user = await _userManager.GetUserAsync(User); // Get logged-in user
        return user?.Id ?? string.Empty; // Return user ID or an empty string if not found
    }

    // Helper method to calculate total income
    private decimal CalculateTotalIncome()
    {
       return (decimal)_context.Transactions
        .Where(t => t.TransactionType == "Income")
        .Sum(t => (double)t.Amount); // Convert Amount to double for SQLite compatibility
    }

    // Helper method to calculate total expenses
    private decimal CalculateTotalExpenses()
    {
       return (decimal)_context.Transactions
        .Where(t => t.TransactionType == "Expense")
        .Sum(t => (double)t.Amount); // Convert Amount to double for SQLite compatibility
    }

    // Helper method to calculate balance (Income - Expenses)
    private decimal CalculateBalance()
    {
        return CalculateTotalIncome() - CalculateTotalExpenses();
    }

    // Index action to load the dashboard view
    public IActionResult Index()
    {
        //This is to compare the budget goal with the balance
        var budgetGoal = _context.BudgetGoals
            .Where(bg => bg.StartDate <= DateTime.Now && bg.EndDate >= DateTime.Now)
            .FirstOrDefault();

        var model = new BudgetViewModel
        {
            TotalIncome = CalculateTotalIncome(),
            TotalExpenses = CalculateTotalExpenses(),
            Balance = CalculateBalance(),
            Transactions = _context.Transactions.ToList(),
            NewTransaction = new Transaction(), // Initialize NewTransaction for form binding
            MinimumBudgetThreshold = budgetGoal?.MinimumBudgetThreshold ?? 0 // Initialize MinimumBudgetThreshold
        };

        return View(model);
    }

    // POST: AddTransaction
    [HttpPost]
    public async Task<IActionResult> AddTransaction(BudgetViewModel model)
    {
        // Log incoming model to debug issues
        _logger.LogInformation("AddTransaction called with transaction ID: {Id}, Amount: {Amount}, Category: {Category}", 
            model.NewTransaction.Id, model.NewTransaction.Amount, model.NewTransaction.Category);

        // Check ModelState validity
        if (!ModelState.IsValid)
        {
            // Log errors if any
            Console.WriteLine("ModelState is invalid:");
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine($" - {error.ErrorMessage}");
            }
            return View("Index", model); // Return to Index if ModelState is invalid
        }

        // Retrieve new transaction and set its type based on the amount
        var transaction = model.NewTransaction;
        if (transaction.Amount > 0)
        {
            transaction.TransactionType = "Income";
        }
        else if (transaction.Amount < 0)
        {
            transaction.TransactionType = "Expense";
        }
        else
        {
            ModelState.AddModelError("NewTransaction.Amount", "Amount must not be zero.");
            return View("Index", model); // Show error if amount is zero
        }

        // Handle UserId (authenticated users or "Anonymous")
        if (User.Identity.IsAuthenticated)
        {
            transaction.UserId = await GetCurrentUserId();
        }
        else
        {
            transaction.UserId ??= "Anonymous"; // For unauthenticated users, set as "Anonymous"
        }

        // Save the new transaction
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Redirect back to Index after adding transaction
        return RedirectToAction("Index");
    }

    // GET: UpdateTransaction
    [HttpGet]   
    public IActionResult UpdateTransaction(int id)
    {
        var existingTransaction = _context.Transactions.Find(id);
        if (existingTransaction == null)
        {
            return NotFound($"Transaction with ID {id} not found.");
        }

        var model = new BudgetViewModel
        {
            NewTransaction = existingTransaction
        };

        return View(model);
    }

    // POST: UpdateTransaction
    [HttpPost]
    public IActionResult UpdateTransaction(BudgetViewModel model)
    {
        if (!ModelState.IsValid)
        {
            // Log ModelState errors
            Console.WriteLine("ModelState is invalid:");
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine($" - {error.ErrorMessage}");
            }
            return View(model);
        }

        var existingTransaction = _context.Transactions.Find(model.NewTransaction.Id);
        if (existingTransaction == null)
        {
            return NotFound($"Transaction with ID {model.NewTransaction.Id} not found.");
        }

        existingTransaction.Description = model.NewTransaction.Description;
        existingTransaction.Amount = model.NewTransaction.Amount;
        existingTransaction.Date = model.NewTransaction.Date;
        existingTransaction.Category = model.NewTransaction.Category;

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

        if (User.Identity.IsAuthenticated)
        {
            existingTransaction.UserId = GetCurrentUserId().Result;
        }
        else
        {
            existingTransaction.UserId ??= "Anonymous";
        }

        _context.SaveChanges(); // Save updated transaction
        return RedirectToAction("Index"); // Redirect to Index after update
    }

    // POST: DeleteTransaction
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

    public IActionResult ManageBudgetOld()
    {
        // Retrieve budget data
        var budgetLimit = _context.Budgets.FirstOrDefault(); // Or however you retrieve the budget data

        if (budgetLimit == null)
        {
            budgetLimit = new BudgetLimit(); // Create a new budget limit object if none exists
        }
        // Calculate total spending for this month and year
        var totalSpentThisMonth = _context.Transactions
            .Where(t => t.Date.Month == DateTime.Now.Month && t.Date.Year == DateTime.Now.Year)
            .Sum(t => (double)t.Amount);  // Convert to double

        var totalSpentThisYear = _context.Transactions
            .Where(t => t.Date.Year == DateTime.Now.Year)
            .Sum(t => (double)t.Amount);  // Convert to double

        // Pass data to the view
        ViewBag.TotalSpentThisMonth = totalSpentThisMonth;
        ViewBag.TotalSpentThisYear = totalSpentThisYear;

        return View(budgetLimit); // Pass the budget limit model to the view
    }

    // GET: Budget/Create
    [HttpGet]
    public IActionResult CreateBudget()
    {
        return View();
    }

    // POST: Budget/Create
    [HttpPost]
    public async Task<IActionResult> CreateBudget(BudgetGoal model)
    {
        if (ModelState.IsValid)
        {
            _context.BudgetGoals.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction("ManageBudget");
        }
        return View(model);
    }

    // GET: Budget/Edit/5
    [HttpGet]
    public async Task<IActionResult> EditBudget(int id)
    {
        var budgetGoal = await _context.BudgetGoals.FindAsync(id);
        if (budgetGoal == null)
        {
            return NotFound();
        }
        return View(budgetGoal);
    }

    // POST: Budget/Edit/5
    [HttpPost]
    public async Task<IActionResult> EditBudget(BudgetGoal model)
    {
        if (ModelState.IsValid)
        {
            _context.Update(model);
            await _context.SaveChangesAsync();
            return RedirectToAction("ManageBudget");
        }
        return View(model);
    }

    // GET: Budget/Delete/5
    [HttpGet]
    public async Task<IActionResult> DeleteBudget(int id)
    {
        var budgetGoal = await _context.BudgetGoals.FindAsync(id);
        if (budgetGoal == null)
        {
            return NotFound();
        }
        return View(budgetGoal);
    }

    // POST: Budget/Delete/5
    [HttpPost, ActionName("DeleteBudget")]
    public async Task<IActionResult> DeleteBudgetConfirmed(int id)
    {
        var budgetGoal = await _context.BudgetGoals.FindAsync(id);
        if (budgetGoal != null)
        {
            _context.BudgetGoals.Remove(budgetGoal);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("ManageBudget");
    }

    // GET: Budget/Details/5
    [HttpGet]
    public async Task<IActionResult> DetailsBudget(int id)
    {
        var budgetGoal = await _context.BudgetGoals.FindAsync(id);
        if (budgetGoal == null)
        {
            return NotFound();
        }
        return View(budgetGoal);
    }

   
   // GET: Budget/Manage
public async Task<IActionResult> ManageBudget()
{
    var budgetGoal = await _context.BudgetGoals
        .Where(bg => bg.StartDate <= DateTime.Now && bg.EndDate >= DateTime.Now)
        .FirstOrDefaultAsync();

    if (budgetGoal != null)
    {
        // Calculate total income and expenses for the active budget goal period
        var totalIncome = _context.Transactions
            .Where(t => t.TransactionType == "Income" &&
                        t.Date >= budgetGoal.StartDate &&
                        t.Date <= budgetGoal.EndDate)
            .Sum(t => t.Amount);

        var totalExpenses = _context.Transactions
            .Where(t => t.TransactionType == "Expense" &&
                        t.Date >= budgetGoal.StartDate &&
                        t.Date <= budgetGoal.EndDate)
            .Sum(t => t.Amount);

        //This variable is here because its needed to deteermine when to trigger the alert
        var balance = totalIncome - totalExpenses;

        // Trigger an alert if the balance falls below the minimum threshold
        if (balance < budgetGoal.MinimumBudgetThreshold)
        {
            TempData["BudgetAlert"] = "Warning: Your balance has fallen below the minimum budget threshold!";
        }

        ViewBag.TotalIncome = totalIncome;
        ViewBag.TotalExpenses = totalExpenses;
    }
    else
    {
        // Initialize ViewBag to avoid null reference
        ViewBag.TotalIncome = 0;
        ViewBag.TotalExpenses = 0;
    }

    return View(budgetGoal);
}


// POST: Budget/SetMinimumThreshold
[HttpPost]
public async Task<IActionResult> SetMinimumThreshold(decimal MinimumBudgetThreshold)
{
    // Retrieve the current budget goal (you can modify this logic as needed)
    var budgetGoal = await _context.BudgetGoals
        .Where(bg => bg.StartDate <= DateTime.Now && bg.EndDate >= DateTime.Now)
        .FirstOrDefaultAsync();

    if (budgetGoal == null)
    {
        // Create a new budget goal if none exists
        budgetGoal = new BudgetGoal
        {
            MinimumBudgetThreshold = MinimumBudgetThreshold,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddMonths(1) // Example: 1-month budget period
        };
        _context.BudgetGoals.Add(budgetGoal);
    }
    else
    {
        // Update the existing budget goal
        budgetGoal.MinimumBudgetThreshold = MinimumBudgetThreshold;
        _context.BudgetGoals.Update(budgetGoal);
    }

    await _context.SaveChangesAsync();

    TempData["SuccessMessage"] = "Minimum budget threshold has been set successfully!";
    return RedirectToAction("Index");
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
 
}