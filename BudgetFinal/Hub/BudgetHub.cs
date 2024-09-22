namespace BudgetFinal.Hub
{
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;   



    public class BudgetHub : Hub
    {              
        //Send message to all connected clients
    public async Task SendTransactions(string message)
        {
            await Clients.All.SendAsync("ReceiveTransactionUpdate", message);
        }
    }
}
