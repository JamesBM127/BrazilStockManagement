using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace BrazilStockManagementTests.Business
{
    public class PortfolioTest
    {
        public StockContext Context { get; set; }
        public StockBusiness StockBusiness { get; set; }
        public PortfolioBusiness PortfolioBusiness { get; set; }
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
            //PortfolioBusiness = null;
            Context = null;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            PortfolioBusiness = null;
        }

        [SetUp]
        public async Task SetUp()
        {
            Context = await TestsSettings.CreateDb(DatabaseOptions.SqlServer);

            Repository = new StockManagerRepository(Context);
            PortfolioBusiness = new PortfolioBusiness(Repository);
            StockBusiness = new StockBusiness(Repository);
        }

        #region Stock
        [Test]
        public async Task Portfolio_CreatePortfolioStock_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            Stock stock = TestsSettings.GetStock(financialInstitution);
            #endregion

            #region Act
            bool addedStock = await PortfolioBusiness.BuyOrSellPortfolioAsync(stock);
            #endregion

            #region Assert
            Assert.That(addedStock, Is.True);
            #endregion
        }

        [Test]
        public async Task Portfolio_CreatePortfolioSameCompanyDiferenteCodeStock_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            Stock stock1 = TestsSettings.GetStock(financialInstitution);
            bool addedStock1 = await PortfolioBusiness.BuyOrSellPortfolioAsync(stock1);
            Stock stock2 = TestsSettings.GetStock(financialInstitution);
            stock2.Code = "ABCD4";
            #endregion

            #region Act
            bool addedStock2 = await PortfolioBusiness.BuyOrSellPortfolioAsync(stock2);
            IReadOnlyCollection<Portfolio> portfolios = await PortfolioBusiness.ListAsync<Portfolio>();
            #endregion

            #region Assert
            Assert.That(addedStock1, Is.True);
            Assert.That(addedStock2, Is.True);
            Assert.That(portfolios.Count, Is.EqualTo(2));
            #endregion
        }

        [Test]
        public async Task Portfolio_AddStockToExistingPortfolio_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            Stock stock1 = TestsSettings.GetStock(financialInstitution);
            Stock stock2 = TestsSettings.GetStock(financialInstitution);
            Stock stock3 = TestsSettings.GetStock(financialInstitution);
            #endregion

            #region Act
            bool addedStock1 = await PortfolioBusiness.BuyOrSellPortfolioAsync(stock1);
            bool addedStock2 = await PortfolioBusiness.BuyOrSellPortfolioAsync(stock2);
            bool addedStock3 = await PortfolioBusiness.BuyOrSellPortfolioAsync(stock3);
            #endregion

            #region Assert
            Assert.That(addedStock1, Is.True);
            Assert.That(addedStock2, Is.True);
            Assert.That(addedStock3, Is.True);
            #endregion
        }

        [Test]
        public async Task Portfolio_BuyStockThenSellThenDeletePortfolio_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            Stock stock1 = TestsSettings.GetStock(financialInstitution);
            Stock stock2 = TestsSettings.GetStock(financialInstitution);
            Stock stock3 = TestsSettings.GetStock(financialInstitution);

            int amount = stock1.Amount + stock2.Amount + stock3.Amount;
            Stock stock4 = TestsSettings.GetStock(financialInstitution, amount: amount);
            stock4.OperationType = OperationType.Sell;
            #endregion

            #region Act
            bool addedStock1 = await PortfolioBusiness.BuyOrSellPortfolioAsync(stock1);
            await PortfolioBusiness.Commit(addedStock1);
            
            bool addedStock2 = await PortfolioBusiness.BuyOrSellPortfolioAsync(stock2);
            await PortfolioBusiness.Commit(addedStock2);

            bool addedStock3 = await PortfolioBusiness.BuyOrSellPortfolioAsync(stock3);
            await PortfolioBusiness.Commit(addedStock3);

            bool addedStock4 = await PortfolioBusiness.BuyOrSellPortfolioAsync(stock4);
            await PortfolioBusiness.Commit(addedStock4);

            Portfolio? portfolio = await PortfolioBusiness.GetAsync<Portfolio>(x => x.Code == stock1.Code);
            #endregion

            #region Assert
            Assert.That(addedStock1 && addedStock2 && addedStock3 && addedStock4, Is.True);
            Assert.That(portfolio, Is.Null);
            #endregion
        }
        
        [Test]
        public async Task Portfolio_SellStockToExistingPortfolio_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            
            Stock stock1 = TestsSettings.GetStock(financialInstitution);
            stock1.Amount = 100;
            stock1.Total = stock1.Price * stock1.Amount;
            bool addedStock1 = await PortfolioBusiness.BuyOrSellPortfolioAsync(stock1);

            Stock stock2 = TestsSettings.GetStock(financialInstitution);
            stock2.Amount--;
            stock2.Price = 11m;
            stock2.Total = stock2.Amount * stock2.Price;
            stock2.OperationType = OperationType.Sell;
            #endregion

            #region Act
            bool addedStock2 = await PortfolioBusiness.BuyOrSellPortfolioAsync(stock2);
            #endregion

            #region Assert
            Assert.That(addedStock1, Is.True);
            Assert.That(addedStock2, Is.True);
            #endregion
        }
        
        [Test]
        public async Task Portfolio_SellStockToRent_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);

            Stock stock = TestsSettings.GetStock(financialInstitution);
            stock.Amount--;
            stock.Price = 11m;
            stock.Total = stock.Amount * stock.Price;
            stock.OperationType = OperationType.Sell;
            #endregion

            #region Act
            bool addedStock = await PortfolioBusiness.BuyOrSellPortfolioAsync(stock);
            #endregion

            #region Assert
            Assert.That(addedStock, Is.True);
            #endregion
        }
        
        [Test]
        public async Task Portfolio_SellStockToRentThenBuy_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);

            Stock stock = TestsSettings.GetStock(financialInstitution);
            Stock stock2 = TestsSettings.GetStock(financialInstitution);
            stock.Amount--;
            stock.Price = 11m;
            stock.Total = stock.Amount * stock.Price;
            stock.OperationType = OperationType.Sell;
            #endregion

            #region Act
            bool addedStock = await PortfolioBusiness.BuyOrSellPortfolioAsync(stock);
            await PortfolioBusiness.Commit(addedStock);
            bool addedStock2 = await PortfolioBusiness.BuyOrSellPortfolioAsync(stock2);
            await PortfolioBusiness.Commit(addedStock2);
            #endregion

            #region Assert
            Assert.That(addedStock, Is.True);
            Assert.That(addedStock2, Is.True);
            #endregion
        }
        #endregion

        #region Reit
        [Test]
        public async Task Portfolio_CreatePortfolioReit_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            Reit reit = TestsSettings.GetReit(financialInstitution);
            #endregion

            #region Act
            bool addedReit = await PortfolioBusiness.BuyOrSellPortfolioAsync(reit);
            #endregion

            #region Assert
            Assert.That(addedReit, Is.True);
            #endregion
        }

        [Test]
        public async Task Portfolio_CreatePortfolioSameCompanyDiferenteCodeReit_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            Reit reit1 = TestsSettings.GetReit(financialInstitution);
            bool addedReit1 = await PortfolioBusiness.BuyOrSellPortfolioAsync(reit1);
            Reit reit2 = TestsSettings.GetReit(financialInstitution);
            reit2.Code = "ABCD4";
            #endregion

            #region Act
            bool addedReit2 = await PortfolioBusiness.BuyOrSellPortfolioAsync(reit2);
            IReadOnlyCollection<Portfolio> portfolios = await PortfolioBusiness.ListAsync<Portfolio>();
            #endregion

            #region Assert
            Assert.That(addedReit1, Is.True);
            Assert.That(addedReit2, Is.True);
            Assert.That(portfolios.Count, Is.EqualTo(2));
            #endregion
        }

        [Test]
        public async Task Portfolio_AddReitToExistingPortfolio_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            Reit reit1 = TestsSettings.GetReit(financialInstitution);
            bool addedReit1 = await PortfolioBusiness.BuyOrSellPortfolioAsync(reit1);
            Reit reit2 = TestsSettings.GetReit(financialInstitution);
            #endregion

            #region Act
            bool addedReit2 = await PortfolioBusiness.BuyOrSellPortfolioAsync(reit2);
            #endregion

            #region Assert
            Assert.That(addedReit1, Is.True);
            Assert.That(addedReit2, Is.True);
            #endregion
        }

        [Test]
        public async Task Portfolio_SellReitToExistingPortfolio_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);

            Reit reit1 = TestsSettings.GetReit(financialInstitution);
            reit1.Amount = 100;
            reit1.Total = reit1.Price * reit1.Amount;
            bool addedReit1 = await PortfolioBusiness.BuyOrSellPortfolioAsync(reit1);

            Reit reit2 = TestsSettings.GetReit(financialInstitution);
            reit2.Amount--;
            reit2.Price = 11m;
            reit2.Total = reit2.Amount * reit2.Price;
            reit2.OperationType = OperationType.Sell;
            #endregion

            #region Act
            bool addedReit2 = await PortfolioBusiness.BuyOrSellPortfolioAsync(reit2);
            #endregion

            #region Assert
            Assert.That(addedReit1, Is.True);
            Assert.That(addedReit2, Is.True);
            #endregion
        }

        [Test]
        public async Task Portfolio_SellReitToRent_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);

            Reit reit = TestsSettings.GetReit(financialInstitution);
            reit.Amount--;
            reit.Price = 11m;
            reit.Total = reit.Amount * reit.Price;
            reit.OperationType = OperationType.Sell;
            #endregion

            #region Act
            bool addedReit = await PortfolioBusiness.BuyOrSellPortfolioAsync(reit);
            #endregion

            #region Assert
            Assert.That(addedReit, Is.True);
            #endregion
        }

        [Test]
        public async Task Portfolio_SellReitToRentThenBuy_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);

            Reit reit = TestsSettings.GetReit(financialInstitution);
            Reit reit2 = TestsSettings.GetReit(financialInstitution);
            reit.Amount--;
            reit.Price = 11m;
            reit.Total = reit.Amount * reit.Price;
            reit.OperationType = OperationType.Sell;
            #endregion

            #region Act
            bool addedReit = await PortfolioBusiness.BuyOrSellPortfolioAsync(reit);
            bool addedReit2 = await PortfolioBusiness.BuyOrSellPortfolioAsync(reit2);
            #endregion

            #region Assert
            Assert.That(addedReit, Is.True);
            Assert.That(addedReit2, Is.True);
            #endregion
        }
        #endregion
    }
}
