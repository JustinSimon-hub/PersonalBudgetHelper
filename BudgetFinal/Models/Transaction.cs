using System;
using System.ComponentModel.DataAnnotations;
namespace BudgetFinal.Models{
public class Transaction
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; }

    [Required]
    public string Description { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public string Category { get; set; }

    [Required]
    public string TransactionType { get; set; }
}

}