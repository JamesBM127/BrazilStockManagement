namespace BrazilStockManagementTests.Business
{
    public class ShareHolderTest
    {
        public StockContext Context { get; set; }
        public ShareHolderBusiness ShareHolderBusiness { get; set; }

        [TearDown]
        public void TearDown()
        {
            try
            {
                Context.Database.EnsureDeleted();
            }
            catch { }

            ShareHolderBusiness = null;
            Context = null;
        }

        [SetUp]
        public async Task SetUp()
        {
            Context = await TestsSettings.CreateDb(DatabaseOptions.SqlServer);
            //TearDown();
            IStockManagerRepository Repository = new StockManagerRepository(Context);
            ShareHolderBusiness = new ShareHolderBusiness(Repository);
        }

        [Test]
        public async Task ShareHolder_CreateABunchOfShareHolder_Success()
        {
            #region Arrange
            ShareHolder shareHolder = null;
            List<ShareHolder> holdersList = new List<ShareHolder>();
            List<bool> addedList = new List<bool>();

            for (int i = 1; i <= 50; i++)
            {
                shareHolder = TestsSettings.GetShareHolder("Jameson " + i);
                holdersList.Add(shareHolder);
            }
            #endregion

            #region Act
            foreach (ShareHolder holder in holdersList)
            {
                addedList.Add(await ShareHolderBusiness.AddAsync(holder));
            }
            //await ShareHolderBusiness.
            #endregion

            #region Assert
            Assert.That(addedList, Does.Not.Contain(false));
            #endregion
        }

        [Test]
        public async Task ShareHolder_CreateShareHolder_Success()
        {
            #region Arrange
            ShareHolder shareHolder = TestsSettings.GetShareHolder();
            #endregion

            #region Act
            bool added = await ShareHolderBusiness.AddAsync(shareHolder);
            #endregion

            #region Assert
            Assert.That(added, Is.True);
            #endregion
        }

        [Test]
        public async Task ShareHolder_GetShareHolder_Success()
        {
            #region Arrange
            ShareHolder shareHolder = TestsSettings.GetShareHolder();
            bool added = await ShareHolderBusiness.AddAsync(shareHolder);
            #endregion

            #region Act
            ShareHolder shareHolderFromDb = await ShareHolderBusiness.GetAsync<ShareHolder>(x => x.Id == shareHolder.Id);
            #endregion

            #region Assert
            Assert.That(added, Is.True);
            Assert.That(shareHolderFromDb, Is.Not.Null);
            Assert.That(shareHolderFromDb.Id, Is.EqualTo(shareHolder.Id));
            Assert.That(shareHolderFromDb.Name, Is.EqualTo(shareHolder.Name));
            #endregion
        }

        [Test]
        public async Task ShareHolder_ListShareHolder_Success()
        {
            #region Arrange
            ShareHolder shareHolder1 = TestsSettings.GetShareHolder();
            ShareHolder shareHolder2 = TestsSettings.GetShareHolder("Share Holder 2");
            bool added1 = await ShareHolderBusiness.AddAsync(shareHolder1);
            bool added2 = await ShareHolderBusiness.AddAsync(shareHolder2);
            #endregion

            #region Act
            IReadOnlyCollection<ShareHolder> holders = await ShareHolderBusiness.ListAsync<ShareHolder>();
            #endregion

            #region Assert
            Assert.That(added1, Is.True);
            Assert.That(added2, Is.True);
            Assert.That(holders, Is.Not.Null);
            Assert.That(holders, Has.Count.EqualTo(2));
            #endregion
        }

        [Test]
        public async Task ShareHolder_UpdateShareHolder_Success()
        {
            #region Arrange
            ShareHolder shareHolder = TestsSettings.GetShareHolder();
            bool added = await ShareHolderBusiness.AddAsync(shareHolder);
            shareHolder.Name = "Share Holder Updated Name";
            #endregion

            #region Act
            bool updated = ShareHolderBusiness.Update<ShareHolder>(shareHolder);
            ShareHolder shareHolderFromDb = await ShareHolderBusiness.GetAsync<ShareHolder>(x => x.Id == shareHolder.Id);
            #endregion

            #region Assert
            Assert.That(added, Is.True);
            Assert.That(updated, Is.True);
            Assert.That(shareHolderFromDb, Is.Not.Null);
            Assert.That(shareHolderFromDb.Id, Is.EqualTo(shareHolder.Id));
            Assert.That(shareHolderFromDb.Name, Is.EqualTo(shareHolder.Name));
            #endregion
        }

        [Test]
        public async Task ShareHolder_DeleteShareHolder_Success()
        {
            #region Arrange
            ShareHolder shareHolder = TestsSettings.GetShareHolder();
            bool added = await ShareHolderBusiness.AddAsync(shareHolder);
            #endregion

            #region Act
            object deleted = await ShareHolderBusiness.DeleteAsync<ShareHolder>(shareHolder.Id);
            ShareHolder shareHolderFromDb = await ShareHolderBusiness.GetAsync<ShareHolder>(x => x.Id == shareHolder.Id);
            #endregion

            #region Assert
            Assert.That(added, Is.True);
            Assert.That(deleted, Is.True);
            Assert.That(shareHolderFromDb, Is.Null);
            #endregion
        }
    }
}
