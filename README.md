Features
Income and Expense Tracking
Add, edit, and delete transactions with categories, dates, and amounts.
Budget Management
Create monthly, quarterly, or yearly budgets and receive alerts when spending exceeds limits.
Transaction Filtering & Reporting
Filter transactions by category or date range and generate financial reports.
Real-Time Dashboard
Visual representation of total income, expenses, and balance using charts and graphs.
Category Management
Organize income and expenses into categories for better financial insights.
Authentication & Authorization
Optional user accounts via ASP.NET Identity for personalized tracking (can also operate anonymously).
Responsive UI
Modern, clean, and mobile-friendly interface using Razor views and CSS enhancements.
Technologies Used
Backend: ASP.NET Core MVC
Frontend: Razor Pages, HTML5, CSS3, JavaScript
Database: SQLite with Entity Framework Core
Authentication: ASP.NET Core Identity (optional)
Charts & Graphs: Chart.js or similar library for financial visualizations
Version Control: Git and GitHub
Project Structure
BudgetingApp/
│
├─ Controllers/         # Handles HTTP requests
│   └─ BudgetController.cs
│
├─ Models/              # Application data models
│   └─ Transaction.cs
│   └─ Budget.cs
│   └─ BudgetViewModel.cs
│
├─ Views/               # Razor views for UI
│   └─ Home/
│   └─ Budget/
│
├─ wwwroot/             # Static files (CSS, JS)
│
├─ Data/
│   └─ ApplicationDbContext.cs  # Database context
│
├─ Migrations/          # EF Core database migrations
│
└─ Program.cs           # App entry point and services configuration
Installation & Setup

Follow these steps to run the project locally:

Clone the repository
git clone https://github.com/yourusername/budgeting-app.git
cd budgeting-app
Install dependencies
Make sure you have .NET 8 SDK
 installed.
Apply database migrations
dotnet ef database update
Run the application
dotnet run
Access the application
Open your browser and navigate to https://localhost:5001 or http://localhost:5000.
Usage
Add income and expense transactions with relevant categories and dates.
Create budgets and set limits for specific periods.
Filter transactions or generate reports to analyze financial trends.
View the dashboard to track your balance in real-time.
