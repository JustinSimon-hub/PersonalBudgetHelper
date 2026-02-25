using BudgetFinal.Hub;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Testing.Areas.Identity.Data;
using BudgetFinal.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IReportService, ReportService>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//Pass along razor pages to live deployment
builder.Services.AddRazorPages();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();


//Code for telling program: when you get a request at the path '/budgethub', direct it to the BudgetHub class for handling."app.MapHub<BudgetHub>("/budgethub");    

app.UseRouting();
//middleware for authentication and authorization
app.UseAuthentication();

app.UseAuthorization();




app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Budget}/{action=Index}/{id?}");


//Pass along pages to live deployment   
app.MapRazorPages();

app.Run();



