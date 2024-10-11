namespace BudgetFinal.Models
{
    public class Category
    {
        public int id { get; set; }
        public string Name { get; set; }

        //Navigation property
        //Navigation property is a property that is defined on the principal entity that allows you to navigate or access the related entity
        public ICollection<Transaction> Transactions { get; set; }

    }
}