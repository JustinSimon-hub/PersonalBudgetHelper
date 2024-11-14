using System.Transactions;
using Microsoft.EntityFrameworkCore;
using BudgetFinal.Models;

    public class ApplicationDbContext : DbContext
    {
        //This property is a DbSet of Transaction objects

            public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<BudgetFinal.Models.Transaction> Transactions { get; set; }

       
    }




