namespace BrazilStockManagementTests
{
    public class StockTestContext : DbContext
    {
        public StockTestContext(DbContextOptions<StockTestContext> options) : base(options)
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

            modelBuilder.SeedDb();
        }
    }
}
