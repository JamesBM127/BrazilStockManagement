namespace BrazilStockManagementTests
{
    public class SeedDbTest
    {
        //public StockTestContext StockTestContext { get; set; }
        public DbContext StockTestContext { get; set; }

        [SetUp]
        public async Task SetUp()
        {
            //StockTestContext = await TestsSettings.CreateDbToSeed(DatabaseOptions.SqlServer, "oi");
            StockTestContext = await TestsSettings.CreateDb(DatabaseOptions.SqlServer);
        }

        [Test]
        public void CreateDb()
        {
            //StockTestContext.Database.EnsureCreatedAsync().Wait();
            Assert.Pass();
        }

        [Test]
        public void DeleteDb()
        {
            StockTestContext.Database.EnsureDeletedAsync().Wait();
            Assert.Pass();
        }

        [Test]
        public void DeleteThenCreateDb()
        {
            StockTestContext.Database.EnsureDeletedAsync().Wait();
            StockTestContext.Database.EnsureCreatedAsync().Wait();
            Assert.Pass();
        }
    }
}
