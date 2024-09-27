using Microsoft.AspNetCore.Mvc;
using System.Linq;
using BudgetFinal.Models;



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
        return View(transaction);
    }

        //Adding methods to update and delete transactions
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

        public IActionResult DeleteTransaction(int id)
        {
            var transaction = _context.Transactions.Find(id);
            _context.Transactions.Remove(transaction);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
}
