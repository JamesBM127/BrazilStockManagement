namespace BrazilStockManagementTests.Services
{
    public class ReceivedServiceTests
    {
        public StockContext Context { get; set; }
        public IStockManagerRepository Repository;
        public ReceivedService ReceivedService { get; set; }
        public Portfolio PortfolioStock { get; set; }
        public Portfolio PortfolioReit { get; set; }


        [TearDown]
        public void TearDown()
        {
            try
            {
                Context.Database.EnsureDeleted();
            }
            catch { }

            Context = null;
        }

        [SetUp]
        public async Task SetUp()
        {
            Context = await TestsSettings.CreateDb(DatabaseOptions.InMemoryDatabase);

            Repository = new StockManagerRepository(Context);
            
            await Arrange();
        }

        #region Dividend
        [TestCase(DividendType.Dividendo)]
        [TestCase(DividendType.JCP)]
        public async Task ReceivedService_AddDividendAndJCP_Success(DividendType dividendType)
        {
            #region Arrange
            Dividend dividendStock = TestsSettings.GetDividend(PortfolioStock.Id, dividendType);
            Dividend dividendReit = TestsSettings.GetDividend(PortfolioReit.Id, dividendType);
            #endregion

            #region Act
            bool addedStock = await ReceivedService.AddReceivedAsync<Dividend>(dividendStock);
            bool addedReit = await ReceivedService.AddReceivedAsync<Dividend>(dividendReit);
            #endregion

            #region Assert
            Assert.IsTrue(addedStock && addedReit);
            #endregion
        }
        
        [TestCase(DividendType.Dividendo)]
        [TestCase(DividendType.JCP)]
        public async Task ReceivedService_UpdateDividend_Success(DividendType dividendType)
        {
            #region Arrange
            Dividend dividend = TestsSettings.GetDividend(PortfolioStock.Id, dividendType);
            bool added = await ReceivedService.AddReceivedAsync<Dividend>(dividend);

            dividend.Amount--;
            #endregion

            #region Act
            bool updated = await ReceivedService.UpdateReceivedAsync<Dividend>(dividend);
            #endregion

            #region Assert
            Assert.IsTrue(added && updated);
            #endregion
        }
        
        [TestCase(DividendType.Dividendo)]
        [TestCase(DividendType.JCP)]
        public async Task ReceivedService_DeleteDividend_Success(DividendType dividendType)
        {
            #region Arrange
            Dividend dividend = TestsSettings.GetDividend(PortfolioStock.Id, dividendType);
            bool added = await ReceivedService.AddReceivedAsync<Dividend>(dividend);
            #endregion

            #region Act
            bool deleted = await ReceivedService.DeleteReceivedAsync<Dividend>(dividend);
            #endregion

            #region Assert
            Assert.IsTrue(added && deleted);
            #endregion
        }
        
        [TestCase(DividendType.Dividendo)]
        [TestCase(DividendType.JCP)]
        public async Task ReceivedService_ListAllDividend_Success(DividendType dividendType)
        {
            #region Arrange
            Dividend dividend1 = TestsSettings.GetDividend(PortfolioStock.Id, dividendType);
            Dividend dividend2 = TestsSettings.GetDividend(PortfolioStock.Id, dividendType);
            bool added1 = await ReceivedService.AddReceivedAsync<Dividend>(dividend1);
            bool added2 = await ReceivedService.AddReceivedAsync<Dividend>(dividend2);
            #endregion

            #region Act
            IReadOnlyCollection<Dividend> dividends = await ReceivedService.ListReceivedAsync<Dividend>();
            #endregion

            #region Assert
            Assert.IsTrue(added1 && added2);
            Assert.That(dividends.Count, Is.EqualTo(2));
            #endregion
        }
        
        [TestCase(DividendType.Dividendo)]
        [TestCase(DividendType.JCP)]
        public async Task ReceivedService_ListDividendOfOnlyEspecificPortfolio_Success(DividendType dividendType)
        {
            #region Arrange
            Dividend dividend1 = TestsSettings.GetDividend(PortfolioStock.Id, dividendType);
            Dividend dividend2 = TestsSettings.GetDividend(PortfolioReit.Id, dividendType);
            bool added1 = await ReceivedService.AddReceivedAsync<Dividend>(dividend1);
            bool added2 = await ReceivedService.AddReceivedAsync<Dividend>(dividend2);
            #endregion

            #region Act
            IReadOnlyCollection<Dividend> dividends = await ReceivedService.ListReceivedAsync<Dividend>(x => x.PortfolioId == PortfolioStock.Id);
            #endregion

            #region Assert
            Assert.IsTrue(added1 && added2);
            Assert.That(dividends.Count, Is.EqualTo(1));
            #endregion
        }

        #endregion

        #region Bonus Share

        [TestCase(0.04)]
        [TestCase(0.25)]
        public async Task ReceivedService_AddBonusShare_Success(decimal proporcion)
        {
            #region Arrange
            BonusShare bonusShare = TestsSettings.GetBonusShare(PortfolioStock.Id);
            bonusShare.Proportion = proporcion;
            #endregion

            #region Act
            bool addBonuShare = await ReceivedService.AddReceivedAsync<BonusShare>(bonusShare);
            #endregion

            #region Assert
            Assert.IsTrue(addBonuShare);
            #endregion
        }
        
        [TestCase(0.04)]
        [TestCase(0.25)]
        public async Task ReceivedService_UpdateBonusShare_Success(decimal proporcion)
        {
            #region Arrange
            BonusShare bonusShare = TestsSettings.GetBonusShare(PortfolioStock.Id);
            bonusShare.Proportion = proporcion;
            bool addedStock = await ReceivedService.AddReceivedAsync<BonusShare>(bonusShare);

            IReadOnlyCollection<BonusShare> bonusShares = await ReceivedService.ListReceivedAsync<BonusShare>();
            bonusShare = bonusShares.First();
            bonusShare.Proportion = proporcion + 0.05m;
            #endregion

            #region Act
            bool updatedBonusShare = await ReceivedService.UpdateReceivedAsync<BonusShare>(bonusShare);
            #endregion

            #region Assert
            Assert.IsTrue(addedStock && updatedBonusShare);
            #endregion
        }

        [Test]
        public async Task ReceivedService_DeleteBonusShare_Success()
        {
            #region Arrange
            BonusShare bonusShare = TestsSettings.GetBonusShare(PortfolioStock.Id);
            bool addedStock = await ReceivedService.AddReceivedAsync<BonusShare>(bonusShare);

            IReadOnlyCollection<BonusShare> bonusShares = await ReceivedService.ListReceivedAsync<BonusShare>();
            bonusShare = bonusShares.First();
            #endregion

            #region Act
            bool deletedBonusShare = await ReceivedService.DeleteReceivedAsync<BonusShare>(bonusShare);
            #endregion

            #region Assert
            Assert.IsTrue(addedStock && deletedBonusShare);
            #endregion
        }

        #endregion
        private async Task Arrange()
        {
            StockBusiness stockBusiness = new StockBusiness(Repository);
            PortfolioBusiness portfolioBusiness = new PortfolioBusiness(Repository);
            DividendBusiness dividendBusiness = new DividendBusiness(Repository);
            BonusShareBusiness bonusShareBusiness = new BonusShareBusiness(Repository);

            ReceivedService = new ReceivedService(dividendBusiness, bonusShareBusiness, portfolioBusiness);

            VariableIncomeService VariableIncomeService = new VariableIncomeServiceTests(stockBusiness, portfolioBusiness: portfolioBusiness);

            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);

            Stock Stock = TestsSettings.GetStock(financialInstitution, amount: 10000);
            Reit Reit = TestsSettings.GetReit(financialInstitution, amount: 10000);
            await VariableIncomeService.AddNewVariableIncome<Stock>(Stock);
            await VariableIncomeService.AddNewVariableIncome<Reit>(Reit);

            PortfolioStock = await portfolioBusiness.GetAsync<Portfolio>(x => x.Code == Stock.Code);
            PortfolioReit = await portfolioBusiness.GetAsync<Portfolio>(x => x.Code == Reit.Code);
        }
    }
}
