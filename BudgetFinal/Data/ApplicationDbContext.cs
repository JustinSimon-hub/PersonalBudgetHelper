using System.Transactions;
using Microsoft.EntityFrameworkCore;

namespace BudgetFinal.Data
{
    public class ApplicationDbContext : DbContext
    {
        //This property is a DbSet of Transaction objects
        public DbSet<Transaction> Transactions { get; set; }

    }
}