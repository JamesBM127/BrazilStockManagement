using BrazilStockManagement.Data.DataSettings;
using BrazilStockManagement.Entity;
using Microsoft.EntityFrameworkCore;

namespace BrazilStockManagement.Data
{
    public class StockContext : DbContext
    {
        public StockContext(DbContextOptions<StockContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.PortfolioModelBuilder();
            modelBuilder.ShareHolderModelBuider();
            modelBuilder.FinancialInstitutionModelBuider();
            modelBuilder.FixedDepositModelBuider();
            modelBuilder.StockModelBuider();
            modelBuilder.ReitModelBuider();
            modelBuilder.OptionsModelBuider();
            modelBuilder.ProfitModelBuider();
            modelBuilder.BonusShareModelBuider();
            modelBuilder.DividendModelBuider();
        }
    }
}
