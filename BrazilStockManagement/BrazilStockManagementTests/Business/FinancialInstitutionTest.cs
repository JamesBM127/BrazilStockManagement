namespace BrazilStockManagementTests.Business
{
    public class FinancialInstitutionTest
    {
        public StockContext Context { get; set; }
        public FinancialInstitutionBusiness FinancialInstitutionBusiness { get; set; }
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
            FinancialInstitutionBusiness = null;
            Context = null;
        }

        [SetUp]
        public async Task SetUp()
        {
            Context = await TestsSettings.CreateDb(DatabaseOptions.InMemoryDatabase);

            Repository = new StockManagerRepository(Context);
            FinancialInstitutionBusiness = new FinancialInstitutionBusiness(Repository);
        }

        [Test]
        public async Task FinancialInstitution_CreateFinancialInstitution_Success()
        {
            #region Arrange
            ShareHolder shareHolder = await TestsSettings.CreateAndGetShareHolderInDb(Repository);
            FinancialInstitution FinancialInstitution = TestsSettings.GetFinancialInstitution(shareHolder.Id);
            #endregion

            #region Act
            bool added = await FinancialInstitutionBusiness.AddAsync(FinancialInstitution);
            #endregion

            #region Assert
            Assert.That(added, Is.True);
            #endregion
        }

        [Test]
        public async Task FinancialInstitution_GetFinancialInstitution_Success()
        {
            #region Arrange
            ShareHolder shareHolder = await TestsSettings.CreateAndGetShareHolderInDb(Repository);
            FinancialInstitution FinancialInstitution = TestsSettings.GetFinancialInstitution(shareHolder.Id);
            bool added = await FinancialInstitutionBusiness.AddAsync(FinancialInstitution);
            await FinancialInstitutionBusiness.Commit(true);
            #endregion

            #region Act
            FinancialInstitution FinancialInstitutionFromDb = await FinancialInstitutionBusiness.GetAsync<FinancialInstitution>(x => x.Id == FinancialInstitution.Id);
            #endregion

            #region Assert
            Assert.That(added, Is.True);
            Assert.That(FinancialInstitutionFromDb, Is.Not.Null);
            Assert.That(FinancialInstitutionFromDb.Id, Is.EqualTo(FinancialInstitution.Id));
            Assert.That(FinancialInstitutionFromDb.Name, Is.EqualTo(FinancialInstitution.Name));
            #endregion
        }

        [Test]
        public async Task FinancialInstitution_ListFinancialInstitution_Success()
        {
            #region Arrange
            ShareHolder shareHolder = await TestsSettings.CreateAndGetShareHolderInDb(Repository);
            FinancialInstitution FinancialInstitution1 = TestsSettings.GetFinancialInstitution(shareHolder.Id);
            FinancialInstitution FinancialInstitution2 = TestsSettings.GetFinancialInstitution(shareHolder.Id, "Financial Institution 2");
            bool added1 = await FinancialInstitutionBusiness.AddAsync(FinancialInstitution1);
            bool added2 = await FinancialInstitutionBusiness.AddAsync(FinancialInstitution2);
            await FinancialInstitutionBusiness.Commit(true);
            #endregion

            #region Act
            IReadOnlyCollection<FinancialInstitution> holders = await FinancialInstitutionBusiness.ListAsync<FinancialInstitution>();
            #endregion

            #region Assert
            Assert.That(added1, Is.True);
            Assert.That(added2, Is.True);
            Assert.That(holders, Is.Not.Null);
            Assert.That(holders, Has.Count.EqualTo(2));
            #endregion
        }

        [Test]
        public async Task FinancialInstitution_UpdateFinancialInstitution_Success()
        {
            #region Arrange
            ShareHolder shareHolder = await TestsSettings.CreateAndGetShareHolderInDb(Repository);
            FinancialInstitution FinancialInstitution = TestsSettings.GetFinancialInstitution(shareHolder.Id);
            bool added = await FinancialInstitutionBusiness.AddAsync(FinancialInstitution);
            await FinancialInstitutionBusiness.Commit(true);

            FinancialInstitution.Name = "Financial Institution Updated Name";
            #endregion

            #region Act
            bool updated = FinancialInstitutionBusiness.Update<FinancialInstitution>(FinancialInstitution);
            #endregion

            #region Assert
            Assert.That(added, Is.True);
            Assert.That(updated, Is.True);
            #endregion
        }

        [Test]
        public async Task FinancialInstitution_DeleteFinancialInstitution_Success()
        {
            #region Arrange
            ShareHolder shareHolder = await TestsSettings.CreateAndGetShareHolderInDb(Repository);
            FinancialInstitution FinancialInstitution = TestsSettings.GetFinancialInstitution(shareHolder.Id);
            bool added = await FinancialInstitutionBusiness.AddAsync(FinancialInstitution);
            await FinancialInstitutionBusiness.Commit(true);
            #endregion

            #region Act
            object deleted = await FinancialInstitutionBusiness.DeleteAsync<FinancialInstitution>(FinancialInstitution.Id);
            #endregion

            #region Assert
            Assert.That(added, Is.True);
            Assert.That(deleted, Is.True);
            #endregion
        }
    }
}
