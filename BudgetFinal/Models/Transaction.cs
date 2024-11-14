using System;
using System.ComponentModel.DataAnnotations;
namespace BudgetFinal.Models{
public class Transaction
    {
        public int Id { get; set; }
        [Required]
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }

        //New properties for filtering transactions
        public string UserId { get; set; } // To link each transaction to a specific user
        public string TransactionType { get; set; } // e.g., "Income" or "Expense"
        public string Category { get; set; } // Optional, for categorizing transactions

    }
}