using Microsoft.AspNetCore.Mvc;
using System.Linq;
using BudgetFinal.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using BudgetFinal.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Testing.Areas.Identity.Data;
//App doesnt read the filter budget feature
[Authorize]
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
    private async Task<decimal> CalculateTotalIncome()
    {
       var userId = await GetCurrentUserId();
       return (decimal)_context.Transactions
        .Where(t => t.UserId == userId && t.TransactionType == "Income")
        .Sum(t => (double)t.Amount); // Convert Amount to double for SQLite compatibility
    }

    // Helper method to calculate total expenses
    private async Task<decimal> CalculateTotalExpenses()
    {
       var userId = await GetCurrentUserId();
       return (decimal)_context.Transactions
        .Where(t => t.UserId == userId && t.TransactionType == "Expense")
        .Sum(t => Math.Abs((double)t.Amount)); // Ensure positive sum
    }

    // Helper method to calculate balance (Income - Expenses)
    private async Task<decimal> CalculateBalance()
    {
        var totalIncome = await CalculateTotalIncome();
        var totalExpenses = await CalculateTotalExpenses();
        return totalIncome - totalExpenses; // Return the balance
    }

    // Index action to load the dashboard view
    public async Task<IActionResult> Index()
    {
        var userId = await GetCurrentUserId();

        //Retrieves the current budget goal
        var budgetGoal = _context.BudgetGoals
            .Where(bg => bg.UserId == userId && bg.StartDate <= DateTime.Now && bg.EndDate >= DateTime.Now)
            .FirstOrDefault();

        //calculates the total income, expenses, and balance
        var totalIncome = await CalculateTotalIncome();
        var totalExpenses = await CalculateTotalExpenses();
        var balance = await CalculateBalance();

    //Check if the expense exceed the income
    if (totalExpenses > totalIncome)
        {
                    TempData["ExpenseAlert"] = true; // Set a flag for the popup
                    TempData.Keep("ExpenseAlert");
                    _logger.LogInformation("ExpenseAlert triggered: TotalIncome = {TotalIncome}, TotalExpenses = {TotalExpenses}", totalIncome, totalExpenses);

        }

    //System for checking balance falling below the min
    if(budgetGoal != null && balance < budgetGoal.MinimumBudgetThreshold)
        {
           TempData["BudgetAlert"] = "Warning: Your balance has fallen below the minimum budget threshold!";
        }

        var model = new BudgetViewModel
        {
            TotalIncome = totalIncome,
            TotalExpenses = totalExpenses,
            Balance = balance,
            Transactions = _context.Transactions.Where(t => t.UserId == userId).ToList(),
            NewTransaction = new Transaction(), // Initialize NewTransaction for form binding
            MinimumBudgetThreshold = budgetGoal?.MinimumBudgetThreshold ?? 0 // Set the minimum budget threshold
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

        // Set UserId for authenticated user
        transaction.UserId = await GetCurrentUserId();

        // Save the new transaction
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Redirect back to Index after adding transaction
        return RedirectToAction("Index");
    }

    // GET: UpdateTransaction
    [HttpGet]   
    public async Task<IActionResult> UpdateTransaction(int id)
    {
        var userId = await GetCurrentUserId();
        var existingTransaction = _context.Transactions.FirstOrDefault(t => t.Id == id && t.UserId == userId);
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
    public async Task<IActionResult> UpdateTransaction(BudgetViewModel model)
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

        var userId = await GetCurrentUserId();
        var existingTransaction = _context.Transactions.FirstOrDefault(t => t.Id == model.NewTransaction.Id && t.UserId == userId);
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

        existingTransaction.UserId = userId;

        _context.SaveChanges(); // Save updated transaction
        return RedirectToAction("Index"); // Redirect to Index after update
    }

    // POST: DeleteTransaction
    [HttpPost]
    public async Task<IActionResult> DeleteTransaction(int id)
    {
        var userId = await GetCurrentUserId();
        var transaction = _context.Transactions.FirstOrDefault(t => t.Id == id && t.UserId == userId);
        if (transaction == null)
        {
            return NotFound();
        }

        _context.Transactions.Remove(transaction);
        _context.SaveChanges();

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> ManageBudgetOld()
    {
        var userId = await GetCurrentUserId();

        // Retrieve budget data
        var budgetLimit = _context.Budgets.FirstOrDefault(b => b.UserId == userId); // Or however you retrieve the budget data

        if (budgetLimit == null)
        {
            budgetLimit = new BudgetLimit(); // Create a new budget limit object if none exists
        }
        // Calculate total spending for this month and year
        var totalSpentThisMonth = _context.Transactions
            .Where(t => t.UserId == userId && t.Date.Month == DateTime.Now.Month && t.Date.Year == DateTime.Now.Year)
            .Sum(t => (double)t.Amount);  // Convert to double

        var totalSpentThisYear = _context.Transactions
            .Where(t => t.UserId == userId && t.Date.Year == DateTime.Now.Year)
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
            model.UserId = await GetCurrentUserId();
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
        var userId = await GetCurrentUserId();
        var budgetGoal = await _context.BudgetGoals.FirstOrDefaultAsync(bg => bg.Id == id && bg.UserId == userId);
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
            var userId = await GetCurrentUserId();
            var existingBudget = await _context.BudgetGoals.FirstOrDefaultAsync(bg => bg.Id == model.Id && bg.UserId == userId);
            if (existingBudget == null)
            {
                return NotFound();
            }
            model.UserId = userId;
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
        var userId = await GetCurrentUserId();
        var budgetGoal = await _context.BudgetGoals.FirstOrDefaultAsync(bg => bg.Id == id && bg.UserId == userId);
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
        var userId = await GetCurrentUserId();
        var budgetGoal = await _context.BudgetGoals.FirstOrDefaultAsync(bg => bg.Id == id && bg.UserId == userId);
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
        var userId = await GetCurrentUserId();
        var budgetGoal = await _context.BudgetGoals.FirstOrDefaultAsync(bg => bg.Id == id && bg.UserId == userId);
        if (budgetGoal == null)
        {
            return NotFound();
        }
        return View(budgetGoal);
    }


    public async Task<IActionResult> FilterTransactions(TransactionFilterViewModel model)
 {
     var userId = await GetCurrentUserId();

     // Start with all transactions from the database for current user
     var transactions = _context.Transactions.Where(t => t.UserId == userId).AsQueryable();
 
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


 [HttpPost]
public async Task<IActionResult> SetMinimumThreshold(decimal MinimumBudgetThreshold)
{
    var userId = await GetCurrentUserId();

    // Retrieve the active budget goal
    var budgetGoal = await _context.BudgetGoals
        .Where(bg => bg.UserId == userId && bg.StartDate <= DateTime.Now && bg.EndDate >= DateTime.Now)
        .FirstOrDefaultAsync();

    if (budgetGoal == null)
    {
        // Create a new budget goal if none exists
        budgetGoal = new BudgetGoal
        {
            UserId = userId,
            MinimumBudgetThreshold = MinimumBudgetThreshold,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddMonths(1), // Example: 1-month budget period
            LimitAmount = 1000 // Default limit amount
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
 
}