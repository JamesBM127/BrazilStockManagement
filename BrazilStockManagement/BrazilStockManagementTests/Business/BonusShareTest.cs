using BrazilStockManagementTests.ConfigTests.ConfigServices;

namespace BrazilStockManagementTests.Business
{
    public class BonusShareTest
    {
        public StockContext Context { get; set; }
        public BonusShareBusiness BonusShareBusiness { get; set; }
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
            BonusShareBusiness = null;
            Context = null;
        }

        [SetUp]
        public async Task SetUp()
        {
            Context = await TestsSettings.CreateDb(DatabaseOptions.InMemoryDatabase);

            Repository = new StockManagerRepository(Context);
            BonusShareBusiness = new BonusShareBusiness(Repository);

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
        public async Task BonusShare_CreateBonusShare_Success(InvestmentType investmentType)
        {
            #region Arrange
            BonusShare bonusShare;

            if (investmentType == InvestmentType.Stock)
                bonusShare = TestsSettings.GetBonusShare(PortfolioStock.Id);
            else
                bonusShare = TestsSettings.GetBonusShare(PortfolioReit.Id);
            #endregion

            #region Act
            bool added = await BonusShareBusiness.AddAsync(bonusShare);
            #endregion

            #region Assert
            Assert.That(added, Is.True);
            #endregion
        }

        [TestCase(InvestmentType.Stock)]
        [TestCase(InvestmentType.Reit)]
        public async Task BonusShare_GetBonusShare_Success(InvestmentType investmentType)
        {
            #region Arrange
            BonusShare bonusShare;

            if (investmentType == InvestmentType.Stock)
                bonusShare = TestsSettings.GetBonusShare(PortfolioStock.Id);
            else
                bonusShare = TestsSettings.GetBonusShare(PortfolioReit.Id);

            bool added = await BonusShareBusiness.AddAsync(bonusShare);
            bool saved = await BonusShareBusiness.Commit(added);
            #endregion

            #region Act
            BonusShare BonusShareFromDb = await BonusShareBusiness.GetAsync<BonusShare>(x => x.Id == bonusShare.Id);
            #endregion

            #region Assert
            Assert.That(added && saved, Is.True);
            Assert.That(BonusShareFromDb, Is.Not.Null);
            Assert.That(BonusShareFromDb.Id, Is.EqualTo(bonusShare.Id));
            #endregion
        }

        [TestCase(InvestmentType.Stock)]
        [TestCase(InvestmentType.Reit)]
        public async Task BonusShare_ListBonusShare_Success(InvestmentType investmentType)
        {
            #region Arrange
            BonusShare bonusShare1;
            BonusShare bonusShare2;

            if (investmentType == InvestmentType.Stock)
            {
                bonusShare1 = TestsSettings.GetBonusShare(PortfolioStock.Id);
                bonusShare2 = TestsSettings.GetBonusShare(PortfolioStock.Id);
            }
            else
            {
                bonusShare1 = TestsSettings.GetBonusShare(PortfolioReit.Id);
                bonusShare2 = TestsSettings.GetBonusShare(PortfolioReit.Id);
            }

            bool added1 = await BonusShareBusiness.AddAsync(bonusShare1);
            bool added2 = await BonusShareBusiness.AddAsync(bonusShare2);
            bool saved = await BonusShareBusiness.Commit(added1 && added2);
            #endregion

            #region Act
            IReadOnlyCollection<BonusShare> holders = await BonusShareBusiness.ListAsync<BonusShare>();
            #endregion

            #region Assert
            Assert.That(added1 && added2 && saved, Is.True);
            Assert.That(holders, Is.Not.Null);
            Assert.That(holders, Has.Count.EqualTo(2));
            #endregion
        }

        [TestCase(InvestmentType.Stock)]
        [TestCase(InvestmentType.Reit)]
        public async Task BonusShare_UpdateBonusShare_Success(InvestmentType investmentType)
        {
            #region Arrange
            BonusShare bonusShare;

            if (investmentType == InvestmentType.Stock)
                bonusShare = TestsSettings.GetBonusShare(PortfolioStock.Id);
            else
                bonusShare = TestsSettings.GetBonusShare(PortfolioReit.Id);
            
            bool added = await BonusShareBusiness.AddAsync(bonusShare);
            bool saved = await BonusShareBusiness.Commit(true);
            bonusShare.Amount = 9999;
            #endregion

            #region Act
            bool updated = BonusShareBusiness.Update<BonusShare>(bonusShare);
            #endregion

            #region Assert
            Assert.That(added && saved && updated, Is.True);
            #endregion
        }

        [TestCase(InvestmentType.Stock)]
        [TestCase(InvestmentType.Reit)]
        public async Task BonusShare_DeleteBonusShareReturnBool_Success(InvestmentType investmentType)
        {
            #region Arrange
            BonusShare bonusShare;

            if (investmentType == InvestmentType.Stock)
                bonusShare = TestsSettings.GetBonusShare(PortfolioStock.Id);
            else
                bonusShare = TestsSettings.GetBonusShare(PortfolioReit.Id);

            bool added = await BonusShareBusiness.AddAsync(bonusShare);
            bool saved = await BonusShareBusiness.Commit(true);
            #endregion

            #region Act
            object deleted = await BonusShareBusiness.DeleteAsync<BonusShare>(bonusShare.Id);
            #endregion

            #region Assert
            Assert.That(added && saved, Is.True);
            Assert.That(deleted, Is.True);
            #endregion
        }

        [TestCase(InvestmentType.Stock)]
        [TestCase(InvestmentType.Reit)]
        public async Task BonusShare_DeleteBonusShareReturnObject_Success(InvestmentType investmentType)
        {
            #region Arrange
            BonusShare bonusShare;

            if (investmentType == InvestmentType.Stock)
                bonusShare = TestsSettings.GetBonusShare(PortfolioStock.Id);
            else
                bonusShare = TestsSettings.GetBonusShare(PortfolioReit.Id);

            bonusShare.PortfolioId = Guid.NewGuid();
            bool added = await BonusShareBusiness.AddAsync(bonusShare);
            bool saved = await BonusShareBusiness.Commit(true);
            #endregion

            #region Act
            object deleted = await BonusShareBusiness.DeleteAsync<BonusShare>(bonusShare.Id, true);
            BonusShare deletedBonusShare = (BonusShare)deleted;
            #endregion

            #region Assert
            Assert.That(added && saved, Is.True);
            Assert.That(deletedBonusShare, Is.Not.Null);
            #endregion
        }
    }
}
