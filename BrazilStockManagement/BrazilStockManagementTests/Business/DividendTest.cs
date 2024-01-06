using BrazilStockManagementTests.ConfigTests.ConfigServices;

namespace BrazilStockManagementTests.Business
{
    public class DividendTest
    {
        public StockContext Context { get; set; }
        public DividendBusiness DividendBusiness { get; set; }
        public IStockManagerRepository Repository { get; set; }
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

            Repository = null;
            DividendBusiness = null;
            Context = null;
        }

        [SetUp]
        public async Task SetUp()
        {
            Context = await TestsSettings.CreateDb(DatabaseOptions.InMemoryDatabase);

            Repository = new StockManagerRepository(Context);
            DividendBusiness = new DividendBusiness(Repository);

            VariableIncomeServiceTests variableIncomeService = new VariableIncomeServiceTests(new StockBusiness(Repository), new ReitBusiness(Repository), null, new PortfolioBusiness(Repository));

            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            Stock stock = TestsSettings.GetStock(financialInstitution);
            await variableIncomeService.AddNewVariableIncome(stock);

            Reit reit = TestsSettings.GetReit(financialInstitution);
            await variableIncomeService.AddNewVariableIncome(reit);

            PortfolioStock = await Repository.GetAsync<Portfolio>(x => x.Code == stock.Code);
            PortfolioReit = await Repository.GetAsync<Portfolio>(x => x.Code == reit.Code);
        }


        [TestCase(InvestmentType.Stock)]
        [TestCase(InvestmentType.Reit)]
        public async Task Dividend_CreateDividend_Success(InvestmentType investmentType)
        {
            #region Arrange
            Dividend dividend;

            if (investmentType == InvestmentType.Stock)
                dividend = TestsSettings.GetDividend(PortfolioStock.Id);
            else
                dividend = TestsSettings.GetDividend(PortfolioReit.Id);
            #endregion

            #region Act
            bool added = await DividendBusiness.AddAsync(dividend);
            #endregion

            #region Assert
            Assert.That(added, Is.True);
            #endregion
        }

        [TestCase(InvestmentType.Stock)]
        [TestCase(InvestmentType.Reit)]
        public async Task Dividend_GetDividend_Success(InvestmentType investmentType)
        {
            #region Arrange
            Dividend dividend;

            if (investmentType == InvestmentType.Stock)
                dividend = TestsSettings.GetDividend(PortfolioStock.Id);
            else
                dividend = TestsSettings.GetDividend(PortfolioReit.Id);

            bool added = await DividendBusiness.AddAsync(dividend);
            bool saved = await DividendBusiness.Commit(true);
            #endregion

            #region Act
            Dividend DividendFromDb = await DividendBusiness.GetAsync<Dividend>(x => x.Id == dividend.Id);
            #endregion

            #region Assert
            Assert.That(added && saved, Is.True);
            Assert.That(DividendFromDb, Is.Not.Null);
            Assert.That(DividendFromDb.Id, Is.EqualTo(dividend.Id));
            #endregion
        }

        [TestCase(InvestmentType.Stock)]
        [TestCase(InvestmentType.Reit)]
        public async Task Dividend_ListDividend_Success(InvestmentType investmentType)
        {
            #region Arrange
            Dividend dividend1;
            Dividend dividend2;

            if (investmentType == InvestmentType.Stock)
            {
                dividend1 = TestsSettings.GetDividend(PortfolioStock.Id);
                dividend2 = TestsSettings.GetDividend(PortfolioStock.Id);
            }
            else
            {
                dividend1 = TestsSettings.GetDividend(PortfolioReit.Id);
                dividend2 = TestsSettings.GetDividend(PortfolioReit.Id);
            }

            bool added1 = await DividendBusiness.AddAsync(dividend1);
            bool added2 = await DividendBusiness.AddAsync(dividend2);
            bool saved = await DividendBusiness.Commit(true);
            #endregion

            #region Act
            IReadOnlyCollection<Dividend> holders = await DividendBusiness.ListAsync<Dividend>();
            #endregion

            #region Assert
            Assert.That(added1 && added2 && saved, Is.True);
            Assert.That(holders, Is.Not.Null);
            Assert.That(holders, Has.Count.EqualTo(2));
            #endregion
        }

        [TestCase(InvestmentType.Stock)]
        [TestCase(InvestmentType.Reit)]
        public async Task Dividend_UpdateDividend_Success(InvestmentType investmentType)
        {
            #region Arrange
            Dividend dividend;

            if (investmentType == InvestmentType.Stock)
                dividend = TestsSettings.GetDividend(PortfolioStock.Id);
            else
                dividend = TestsSettings.GetDividend(PortfolioReit.Id);

            bool added = await DividendBusiness.AddAsync(dividend);
            bool saved = await DividendBusiness.Commit(true);
            dividend.Amount = 9999;
            #endregion

            #region Act
            bool updated = DividendBusiness.Update<Dividend>(dividend);
            #endregion

            #region Assert
            Assert.That(added && saved && updated, Is.True);
            #endregion
        }


        [TestCase(InvestmentType.Stock)]
        [TestCase(InvestmentType.Reit)]
        public async Task Dividend_DeleteDividendReturnBool_Success(InvestmentType investmentType)
        {
            #region Arrange
            Dividend dividend;

            if (investmentType == InvestmentType.Stock)
                dividend = TestsSettings.GetDividend(PortfolioStock.Id);
            else
                dividend = TestsSettings.GetDividend(PortfolioReit.Id);

            bool added = await DividendBusiness.AddAsync(dividend);
            bool saved = await DividendBusiness.Commit(true);
            #endregion

            #region Act
            object deleted = await DividendBusiness.DeleteAsync<Dividend>(dividend.Id);
            #endregion

            #region Assert
            Assert.That(added && saved, Is.True);
            Assert.That(deleted, Is.True);
            #endregion
        }

        [TestCase(InvestmentType.Stock)]
        [TestCase(InvestmentType.Reit)]
        public async Task Dividend_DeleteDividendReturnObject_Success(InvestmentType investmentType)
        {
            #region Arrange
            Dividend dividend;

            if (investmentType == InvestmentType.Stock)
                dividend = TestsSettings.GetDividend(PortfolioStock.Id);
            else
                dividend = TestsSettings.GetDividend(PortfolioReit.Id);

            dividend.PortfolioId = Guid.NewGuid();
            bool added = await DividendBusiness.AddAsync(dividend);
            bool saved = await DividendBusiness.Commit(true);
            #endregion

            #region Act
            object deleted = await DividendBusiness.DeleteAsync<Dividend>(dividend.Id, true);
            Dividend deletedDividend = (Dividend)deleted;
            #endregion

            #region Assert
            Assert.That(added && saved, Is.True);
            Assert.That(deletedDividend, Is.Not.Null);
            #endregion
        }
    }
}
