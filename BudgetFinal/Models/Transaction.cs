using System;
using System.ComponentModel.DataAnnotations;
namespace BudgetFinal.Models{
public class Transaction
{
    public int Id { get; set; }

    //UserId is optional for now
    public string? UserId { get; set; }

    [Required]
    public string Description { get; set; }

    [Required]
    [Range(double.MinValue, double.MaxValue, ErrorMessage = "Amount must be a valid number.")]
public decimal Amount { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public string Category { get; set; }

   [StringLength(50)] 
      public string? TransactionType { get; set; }
}

}