using BrazilStockManagement.Enums;
using BrazilStockManagement.Service.Implementation;

namespace BrazilStockManagementTests.Business
{
    public class StockTest
    {
        public StockContext Context { get; set; }
        public StockBusiness StockBusiness { get; set; }
        public IStockManagerRepository Repository { get; set; }

        [TearDown]
        public void TearDown()
        {
            try
            {
                Context.Database.EnsureDeleted();
            }
            catch { }

            Repository = null;
            StockBusiness = null;
            Context = null;
        }

        [SetUp]
        public async Task SetUp()
        {
            Context = await TestsSettings.CreateDb(DatabaseOptions.SqlServer);

            Repository = new StockManagerRepository(Context);
            StockBusiness = new StockBusiness(Repository);
        }

        [Test]
        public async Task Stock_CreateStock_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            Stock Stock = TestsSettings.GetStock(financialInstitution);
            #endregion

            #region Act
            bool added = await StockBusiness.AddAsync(Stock);
            #endregion

            #region Assert
            Assert.That(added, Is.True);
            #endregion
        }

        [Test]
        public async Task Stock_BuyThanSell_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            Stock Stock = TestsSettings.GetStock(financialInstitution);
            bool added = await StockBusiness.AddAsync(Stock);
            bool saved = await StockBusiness.Commit(added);
            #endregion

            #region Act
            #endregion

            #region Assert
            Assert.That(saved, Is.True);
            #endregion
        }

        [Test]
        public async Task Stock_GetStock_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            Stock Stock = TestsSettings.GetStock(financialInstitution);
            bool added = await StockBusiness.AddAsync(Stock);
            #endregion

            #region Act
            Stock StockFromDb = await StockBusiness.GetAsync<Stock>(x => x.Id == Stock.Id);
            #endregion

            #region Assert
            Assert.That(added, Is.True);
            Assert.That(StockFromDb, Is.Not.Null);
            Assert.That(StockFromDb.Id, Is.EqualTo(Stock.Id));
            #endregion
        }

        [Test]
        public async Task Stock_ListStock_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            Stock Stock1 = TestsSettings.GetStock(financialInstitution, "ABCD S.A");
            Stock Stock2 = TestsSettings.GetStock(financialInstitution, "ABCD S.A");
            await SeedStock(financialInstitution);
            bool added1 = await StockBusiness.AddAsync(Stock1);
            bool added2 = await StockBusiness.AddAsync(Stock2);
            #endregion

            #region Act
            IReadOnlyCollection<Stock> stocks = await StockBusiness.ListAsync<Stock>();
            #endregion

            #region Assert
            Assert.That(added1, Is.True);
            Assert.That(added2, Is.True);
            Assert.That(stocks, Is.Not.Null);
            Assert.That(stocks, Has.Count.EqualTo(2));
            #endregion
        }

        [Test]
        public async Task Stock_UpdateStock_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            Stock Stock = TestsSettings.GetStock(financialInstitution);
            bool added = await StockBusiness.AddAsync(Stock);
            Stock.Company = "Company Updated Name";
            #endregion

            #region Act
            bool updated = StockBusiness.Update<Stock>(Stock);
            Stock StockFromDb = await StockBusiness.GetAsync<Stock>(x => x.Id == Stock.Id);
            #endregion

            #region Assert
            Assert.That(added, Is.True);
            Assert.That(updated, Is.True);
            Assert.That(StockFromDb, Is.Not.Null);
            Assert.That(StockFromDb.Id, Is.EqualTo(Stock.Id));
            #endregion
        }

        [Test]
        public async Task Stock_DeleteStock_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            Stock Stock = TestsSettings.GetStock(financialInstitution);
            bool added = await StockBusiness.AddAsync(Stock);
            bool saved = await StockBusiness.Commit(added);
            #endregion

            #region Act
            object deleted = await StockBusiness.DeleteAsync<Stock>(Stock.Id);
            bool deletedSaved = await StockBusiness.Commit(added);
            Stock StockFromDb = await StockBusiness.GetAsync<Stock>(x => x.Id == Stock.Id);
            #endregion

            #region Assert
            Assert.That(added && deletedSaved, Is.True);
            Assert.That(deleted, Is.True);
            Assert.That(StockFromDb, Is.Null);
            #endregion
        }

        private async Task SeedStock(FinancialInstitution financialInstitution)
        {
            int amount = 100;
            decimal price = 13m;
            DateTime now = DateTime.Now;

            Stock stock3 = new()
            {
                Code = "ABCD4",
                Total = (amount + 50) * (price + 0.15m),

                Amount = amount + 50,
                Price = price + 0.15m,
                Company = "ABCD S.A",

                OperationDate = now,
                OperationType = OperationType.Buy,
                InvestmentType = InvestmentType.Stock,

                FinancialInstitutionId = financialInstitution.Id,
                Id = Guid.NewGuid()
            };

            Stock stock4 = new()
            {
                Code = "XPTO3",
                Total = (amount + 50) * (price + 0.15m),

                Amount = amount,
                Price = price,
                Company = "XPTO S.A",

                OperationDate = now,
                OperationType = OperationType.Buy,
                InvestmentType = InvestmentType.Stock,

                FinancialInstitutionId = financialInstitution.Id,
                Id = Guid.NewGuid()
            };

            Stock stock5 = new()
            {
                Code = "XPTO3",
                Total = (amount + 50) * (price + 0.15m),

                Amount = amount,
                Price = price,
                Company = "XPTO S.A",

                OperationDate = now,
                OperationType = OperationType.Sell,
                InvestmentType = InvestmentType.Stock,

                FinancialInstitutionId = financialInstitution.Id,
                Id = Guid.NewGuid()
            };

            await StockBusiness.AddAsync(stock3);
            await StockBusiness.AddAsync(stock4);
            await StockBusiness.AddAsync(stock5);
        }
    }
}
