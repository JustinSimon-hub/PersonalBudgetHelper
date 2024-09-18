using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;   



    public class BudgetHub : Hub
    {
        public async Task SendTransactions(string message)
        {
            await Clients.All.SendAsync("ReceiveTransactionUpdate", message);
        }
    }
