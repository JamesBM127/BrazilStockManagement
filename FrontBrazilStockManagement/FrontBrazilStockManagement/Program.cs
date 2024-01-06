using BrazilStockManagement.Business.Implementation;
using BrazilStockManagement.Data;
using BrazilStockManagement.Service;
using BrazilStockManagement.Service.Implementation;
using BrazilStockManagement.StockRepository.Implementation;
using BrazilStockManagement.StockRepository.Interface;
using JBM.DeserializeJson;
using JBMDatabase;
using JBMDatabase.ConnectionString.Model;
using JBMDatabase.Enum;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

//ConnectionStringModel connectionStringModel = builder.Configuration.ToCSharp<ConnectionStringModel>("ConnectionStringTestFrontEnd");
string connectionStringModel = "Server=James;Database=JBM_DB;Trusted_Connection=True;";

await builder.Services.EnsureCreateAsync<StockContext>(connectionStringModel, DatabaseOptions.InMemoryDatabase, QueryTrackingBehavior.TrackAll);

builder.Services.AddScoped<IStockManagerRepository, StockManagerRepository>();

builder.Services.AddScoped<BonusShareBusiness>();
builder.Services.AddScoped<DividendBusiness>();
builder.Services.AddScoped<FinancialInstitutionBusiness>();
builder.Services.AddScoped<FixedDepositBusiness>();
builder.Services.AddScoped<OptionsBusiness>();
builder.Services.AddScoped<PortfolioBusiness>();
builder.Services.AddScoped<ProfitBusiness>();
builder.Services.AddScoped<ReitBusiness>();
builder.Services.AddScoped<ShareHolderBusiness>();
builder.Services.AddScoped<StockBusiness>();

builder.Services.AddScoped<BaseService>();
builder.Services.AddScoped<PortfolioFrontService>();
builder.Services.AddScoped<FixedDepositService>();
builder.Services.AddScoped<FinancialInstitutionService>();
builder.Services.AddScoped<VariableIncomeService>();
builder.Services.AddScoped<HistoricService>();
builder.Services.AddScoped<EditVariableIncomeService>();
builder.Services.AddScoped<ShareHolderService>();
builder.Services.AddScoped<EditFixedDepositService>();
builder.Services.AddScoped<ReceivedService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
