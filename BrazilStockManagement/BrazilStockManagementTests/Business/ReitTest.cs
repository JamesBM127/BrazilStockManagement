namespace BrazilStockManagementTests.Business
{
    public class ReitTest
    {
        public StockContext Context { get; set; }
        public ReitBusiness ReitBusiness { get; set; }
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
            ReitBusiness = null;
            Context = null;
        }

        [SetUp]
        public async Task SetUp()
        {
            Context = await TestsSettings.CreateDb(DatabaseOptions.SqlServer);

            Repository = new StockManagerRepository(Context);
            ReitBusiness = new ReitBusiness(Repository);
        }

        [Test]
        public async Task Reit_CreateReit_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            Reit Reit = TestsSettings.GetReit(financialInstitution);
            #endregion

            #region Act
            bool added = await ReitBusiness.AddAsync(Reit);
            #endregion

            #region Assert
            Assert.That(added, Is.True);
            #endregion
        }

        [Test]
        public async Task Reit_GetReit_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            Reit Reit = TestsSettings.GetReit(financialInstitution);
            bool added = await ReitBusiness.AddAsync(Reit);
            #endregion

            #region Act
            Reit ReitFromDb = await ReitBusiness.GetAsync<Reit>(x => x.Id == Reit.Id);
            #endregion

            #region Assert
            Assert.That(added, Is.True);
            Assert.That(ReitFromDb, Is.Not.Null);
            Assert.That(ReitFromDb.Id, Is.EqualTo(Reit.Id));
            #endregion
        }

        [Test]
        public async Task Reit_ListReit_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            Reit Reit1 = TestsSettings.GetReit(financialInstitution);
            Reit Reit2 = TestsSettings.GetReit(financialInstitution, "Company 2");
            bool added1 = await ReitBusiness.AddAsync(Reit1);
            bool added2 = await ReitBusiness.AddAsync(Reit2);
            #endregion

            #region Act
            IReadOnlyCollection<Reit> holders = await ReitBusiness.ListAsync<Reit>();
            #endregion

            #region Assert
            Assert.That(added1, Is.True);
            Assert.That(added2, Is.True);
            Assert.That(holders, Is.Not.Null);
            Assert.That(holders, Has.Count.EqualTo(2));
            #endregion
        }

        [Test]
        public async Task Reit_UpdateReit_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            Reit Reit = TestsSettings.GetReit(financialInstitution);
            bool added = await ReitBusiness.AddAsync(Reit);
            Reit.Company = "Company Updated Name";
            #endregion

            #region Act
            bool updated = ReitBusiness.Update<Reit>(Reit);
            Reit ReitFromDb = await ReitBusiness.GetAsync<Reit>(x => x.Id == Reit.Id);
            #endregion

            #region Assert
            Assert.That(added, Is.True);
            Assert.That(updated, Is.True);
            Assert.That(ReitFromDb, Is.Not.Null);
            Assert.That(ReitFromDb.Id, Is.EqualTo(Reit.Id));
            #endregion
        }

        [Test]
        public async Task Reit_DeleteReit_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            Reit Reit = TestsSettings.GetReit(financialInstitution);
            bool added = await ReitBusiness.AddAsync(Reit);
            #endregion

            #region Act
            object deleted = await ReitBusiness.DeleteAsync<Reit>(Reit.Id);
            Reit ReitFromDb = await ReitBusiness.GetAsync<Reit>(x => x.Id == Reit.Id);
            #endregion

            #region Assert
            Assert.That(added, Is.True);
            Assert.That(deleted, Is.True);
            Assert.That(ReitFromDb, Is.Null);
            #endregion
        }
    }
}
