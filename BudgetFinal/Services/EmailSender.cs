using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

namespace BudgetFinal.Services
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // For now, just log that we would send an email
            // In production, implement actual email sending logic
            return Task.CompletedTask;
        }
    }
}
