using System;
using System.ComponentModel.DataAnnotations;
using BudgetFinal.Models;
public class Transaction
    {
     public int Id { get; set; }
     [Required]
    public string Description { get; set; }
    public decimal Amount { get; set; }

    public DateTime Date { get; set; }


    //Category model navigation properties
    public int CategoryId { get; set; }
    public Category Category { get; set; }//Nav proprety
    


    }
