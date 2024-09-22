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
        var transactions = _context.Transactions.ToList();
        var totalIncome = transactions.Where(t => t.Amount > 0).Sum(t => t.Amount);
        var totalExpenses = transactions.Where(t => t.Amount < 0).Sum(t => t.Amount);
        var balance = totalIncome + totalExpenses;

        var viewModel = new BudgetViewModel
        {
            Transactions = transactions,
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
}
