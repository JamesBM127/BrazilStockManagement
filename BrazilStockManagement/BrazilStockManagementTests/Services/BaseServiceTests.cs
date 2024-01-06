using BrazilStockManagement.Business.Implementation;
using BrazilStockManagement.Entity;
using JBMDatabase.Repos;

namespace BrazilStockManagementTests.Services
{
    public class BaseServiceTests
    {
        public BaseServiceConfigTest BaseServiceConfigTest { get; set; }
        public StockContext Context { get; set; }
        public IStockManagerRepository Repository;

        [SetUp]
        public async Task SetUp()
        {
            Context = await TestsSettings.CreateDb(DatabaseOptions.InMemoryDatabase);

            Repository = new StockManagerRepository(Context);
            BonusShareBusiness bonusShareBusiness = new BonusShareBusiness(Repository);
            DividendBusiness dividendBusiness = new DividendBusiness(Repository);
            FinancialInstitutionBusiness financialInstitutionBusiness = new FinancialInstitutionBusiness(Repository);
            FixedDepositBusiness fixedDepositBusiness = new FixedDepositBusiness(Repository);
            OptionsBusiness optionsBusiness = new OptionsBusiness(Repository);
            PortfolioBusiness portfolioBusiness = new PortfolioBusiness(Repository);
            ProfitBusiness profitBusiness = new ProfitBusiness(Repository);
            ReitBusiness reitBusiness = new ReitBusiness(Repository);
            ShareHolderBusiness shareHolderBusiness = new ShareHolderBusiness(Repository);
            StockBusiness stockBusiness = new StockBusiness(Repository);

            BaseServiceConfigTest = new BaseServiceConfigTest(bonusShareBusiness,
                                                               dividendBusiness,
                                                               financialInstitutionBusiness,
                                                               fixedDepositBusiness,
                                                               optionsBusiness,
                                                               portfolioBusiness,
                                                               profitBusiness,
                                                               reitBusiness,
                                                               shareHolderBusiness,
                                                               stockBusiness);
        }

        [Test]
        public async Task GetSomethingOfBaseService()
        {
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            Stock stockBuy = TestsSettings.GetStock(financialInstitution);
            bool addedBuy = await BaseServiceConfigTest.StockBusiness.AddAsync(stockBuy);
            bool commited = await BaseServiceConfigTest.StockBusiness.Commit(addedBuy);

            IReadOnlyCollection<Stock> stockList = await BaseServiceConfigTest.StockBusiness.ListAsync<Stock>();

            Assert.NotNull(stockList);
            Assert.AreEqual(1, stockList.Count);
        }
    }
}





