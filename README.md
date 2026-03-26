# PersonalBudgetHelper

A multi-user personal finance web application built with ASP.NET Core MVC (.NET 8). Users can track income and expenses, set budget goals, monitor spending thresholds, filter transactions, and view financial reports — all within a secure, authenticated environment.

---

## Features

- **Dashboard** — Overview of total income, total expenses, current balance, and active budget goals
- **Transaction Management** — Add, edit, and delete income/expense transactions with category, date, and description; transaction type (Income/Expense) is automatically determined by amount sign
- **Budget Goals** — Create time-bound budget goals with spending limits and minimum balance thresholds
- **Budget Limits** — Set monthly and yearly spending limits
- **Alerts** — Automatic warnings when expenses exceed income or balance falls below a configured minimum threshold
- **Transaction Filtering** — Filter transactions by category and date range
- **Financial Reports** — Monthly, quarterly, and yearly income/expense summaries via the report service
- **Real-time Updates** — SignalR hub infrastructure for broadcasting transaction updates to connected clients
- **User Isolation** — All data is scoped to the authenticated user; no cross-user data leakage

---

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core MVC (.NET 8.0) |
| ORM | Entity Framework Core 8.0 |
| Authentication | ASP.NET Core Identity |
| Real-time | SignalR 1.1.0 |
| Database (prod) | Azure SQL Server (Azure AD auth) |
| Database (dev) | SQL Server LocalDB |
| Frontend | Bootstrap, jQuery, Chart.js |

---

## Project Structure

```
BudgetFinal/
├── Controllers/
│   ├── BudgetController.cs         # Core budget, transaction, and goal operations
│   └── HomeController.cs           # Landing page; redirects authenticated users
├── Models/
│   ├── Transaction.cs              # Income/expense record
│   ├── BudgetGoal.cs               # Time-bound budget period with limits
│   ├── BudgetLimit.cs              # Monthly/yearly spending limits
│   ├── BudgetViewModel.cs          # Dashboard view data
│   ├── ReportViewModel.cs          # Report view data
│   ├── CategorySummary.cs          # Per-category aggregation
│   └── TransactionFilterViewModel.cs
├── Services/
│   ├── IReportService.cs           # Report service interface
│   ├── ReportService.cs            # Monthly, quarterly, yearly report logic
│   └── EmailSender.cs              # Email notification stub
├── Hub/
│   └── BudgetHub.cs                # SignalR hub for real-time updates
├── Views/Budget/
│   ├── Index.cshtml                # Main dashboard
│   ├── CreateBudget.cshtml         # Create a budget goal
│   ├── ManageBudget.cshtml         # Budget management
│   ├── UpdateTransaction.cshtml    # Edit a transaction
│   ├── FilterTransactions.cshtml   # Filter/search transactions
│   └── ChartView.cshtml            # Financial charts
├── Areas/Identity/                 # Scaffolded ASP.NET Identity pages
│   └── Pages/Account/              # Login, Register, Logout, ForgotPassword
├── Migrations/                     # EF Core database migrations
├── Program.cs                      # App configuration and startup
├── appsettings.json                # Production settings (Azure SQL)
└── appsettings.Development.json    # Development settings (LocalDB)
```

---

## Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (LocalDB is sufficient for development) or an Azure SQL database

### Setup

1. **Clone the repository**

   ```bash
   git clone https://github.com/JustinSimon-hub/PersonalBudgetHelper.git
   cd PersonalBudgetHelper
   ```

2. **Configure the database connection**

   Update `BudgetFinal/appsettings.Development.json` with your local SQL Server connection string:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BudgetDb;Trusted_Connection=True;MultipleActiveResultSets=true"
     }
   }
   ```

3. **Apply database migrations**

   The application runs `db.Database.Migrate()` automatically on startup, so the schema will be created/updated when you first run the app. Alternatively, apply migrations manually:

   ```bash
   cd BudgetFinal
   dotnet ef database update
   ```

4. **Run the application**

   ```bash
   dotnet run --project BudgetFinal
   ```

   The app will be available at `https://localhost:7250` (HTTPS) or `http://localhost:5221` (HTTP).

---

## Database Schema

| Table | Description |
|---|---|
| `Transactions` | Income and expense records (UserId, Amount, Category, Date, Type) |
| `BudgetGoals` | Time-bound goals with limit and minimum threshold (UserId, StartDate, EndDate, LimitAmount) |
| `Budgets` | Monthly and yearly spending limits (UserId, MonthlyLimit, YearlyLimit) |
| `AspNetUsers` / Identity tables | User accounts and roles managed by ASP.NET Identity |

---

## Configuration

| Setting | File | Description |
|---|---|---|
| Production DB | `appsettings.json` | Azure SQL Server with Azure AD authentication |
| Development DB | `appsettings.Development.json` | SQL Server LocalDB |
| Login path | `Program.cs` | `/Identity/Account/Login` |
| Account confirmation | `Program.cs` | Disabled (`RequireConfirmedAccount = false`) |

---

## License

This project is for personal/portfolio use.
