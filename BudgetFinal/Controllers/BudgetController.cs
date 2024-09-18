using Microsoft.AspNetCore.Mvc;
using System.Linq;
using BudgetFinal.Models;

namespace BudgetFinal.Controllers
{
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
        } 
                

        public IActionResult AddTransaction()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddTransaction(Transaction transaction)
        {
            return RedirectToAction("Index");
        }
    }
}