namespace BrazilStockManagementTests.Business
{
    public class FixedDepositTest
    {
        public StockContext Context { get; set; }
        public FixedDepositBusiness FixedDepositBusiness { get; set; }
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
            FixedDepositBusiness = null;
            Context = null;
        }

        [SetUp]
        public async Task SetUp()
        {
            Context = await TestsSettings.CreateDb(DatabaseOptions.InMemoryDatabase);

            Repository = new StockManagerRepository(Context);
            FixedDepositBusiness = new FixedDepositBusiness(Repository);
        }

        [Test]
        public async Task FixedDeposit_CreateFixedDeposit_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            FixedDeposit FixedDeposit = TestsSettings.GetFixedDeposit(financialInstitution.Id);
            #endregion

            #region Act
            bool added = await FixedDepositBusiness.AddAsync(FixedDeposit);
            #endregion

            #region Assert
            Assert.That(added, Is.True);
            #endregion
        }

        [Test]
        public async Task FixedDeposit_GetFixedDeposit_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            FixedDeposit FixedDeposit = TestsSettings.GetFixedDeposit(financialInstitution.Id);
            bool added = await FixedDepositBusiness.AddAsync(FixedDeposit);
            #endregion

            #region Act
            FixedDeposit FixedDepositFromDb = await FixedDepositBusiness.GetAsync<FixedDeposit>(x => x.Id == FixedDeposit.Id);
            #endregion

            #region Assert
            Assert.That(added, Is.True);
            Assert.That(FixedDepositFromDb, Is.Not.Null);
            Assert.That(FixedDepositFromDb.Id, Is.EqualTo(FixedDeposit.Id));
            #endregion
        }

        [Test]
        public async Task FixedDeposit_ListFixedDeposit_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            FixedDeposit FixedDeposit1 = TestsSettings.GetFixedDeposit(financialInstitution.Id);
            FixedDeposit FixedDeposit2 = TestsSettings.GetFixedDeposit(financialInstitution.Id, "Company 2");
            bool added1 = await FixedDepositBusiness.AddAsync(FixedDeposit1);
            bool added2 = await FixedDepositBusiness.AddAsync(FixedDeposit2);
            #endregion

            #region Act
            IReadOnlyCollection<FixedDeposit> holders = await FixedDepositBusiness.ListAsync<FixedDeposit>();
            #endregion

            #region Assert
            Assert.That(added1, Is.True);
            Assert.That(added2, Is.True);
            Assert.That(holders, Is.Not.Null);
            Assert.That(holders, Has.Count.EqualTo(2));
            #endregion
        }

        [Test]
        public async Task FixedDeposit_UpdateFixedDeposit_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            FixedDeposit FixedDeposit = TestsSettings.GetFixedDeposit(financialInstitution.Id);
            bool added = await FixedDepositBusiness.AddAsync(FixedDeposit);
            FixedDeposit.Company = "Company Updated Name";
            #endregion

            #region Act
            bool updated = FixedDepositBusiness.Update<FixedDeposit>(FixedDeposit);
            FixedDeposit FixedDepositFromDb = await FixedDepositBusiness.GetAsync<FixedDeposit>(x => x.Id == FixedDeposit.Id);
            #endregion

            #region Assert
            Assert.That(added, Is.True);
            Assert.That(updated, Is.True);
            Assert.That(FixedDepositFromDb, Is.Not.Null);
            Assert.That(FixedDepositFromDb.Id, Is.EqualTo(FixedDeposit.Id));
            #endregion
        }

        [Test]
        public async Task FixedDeposit_DeleteFixedDeposit_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            FixedDeposit FixedDeposit = TestsSettings.GetFixedDeposit(financialInstitution.Id);
            bool added = await FixedDepositBusiness.AddAsync(FixedDeposit);
            #endregion

            #region Act
            object deleted = await FixedDepositBusiness.DeleteAsync<FixedDeposit>(FixedDeposit.Id);
            FixedDeposit FixedDepositFromDb = await FixedDepositBusiness.GetAsync<FixedDeposit>(x => x.Id == FixedDeposit.Id);
            #endregion

            #region Assert
            Assert.That(added, Is.True);
            Assert.That(deleted, Is.True);
            Assert.That(FixedDepositFromDb, Is.Null);
            #endregion
        }
    }
}
