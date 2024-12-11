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
    
    //This property is a DbSet of BudgetLimit objects and iteracts with the Budgets table in the database
    public DbSet<BudgetLimit> Budgets { get; set; }
    //This property is a DbSet of BudgetGoal objects and iteracts with the BudgetGoals table in the database
    public DbSet<BudgetGoal> BudgetGoals { get; set; }





    }




