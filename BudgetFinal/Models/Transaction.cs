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
    }
}