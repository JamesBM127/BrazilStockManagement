using BrazilStockManagement.Entity;
using JBMDatabase.UnitOfWork;
using System.Diagnostics;

namespace BrazilStockManagementTests.Business
{
    public class OptionsTest
    {
        public StockContext Context { get; set; }
        public OptionsBusiness OptionsBusiness { get; set; }
        public IStockManagerRepository Repository { get; set; }
        //public string jsonSection = null;
        public string jsonSection = "ConnectionStringTestFrontEnd";

        [TearDown]
        public void TearDown()
        {
            try
            {
                //Context.Database.EnsureDeleted();
            }
            catch { }

            Repository = null;
            OptionsBusiness = null;
            Context = null;
        }

        [SetUp]
        public async Task SetUp()
        {

            //TearDown();

            if (jsonSection is not null)
                Context = await TestsSettings.CreateDb(DatabaseOptions.SqlServer, jsonSection);
            else
                Context = await TestsSettings.CreateDb(DatabaseOptions.SqlServer);

            Repository = new StockManagerRepository(Context);
            OptionsBusiness = new OptionsBusiness(Repository);
        }

        [Test]
        public async Task Options_CreateOptions_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            //Options Options = TestsSettings.GetStock(financialInstitution);
            Options options = new()
            {
                Strike = 13m,
                ExerciseDate = new DateTime(2023, 4, 20),
                DerivativeType = DerivativeType.Call,

                Code = "ABCDA111",
                Total = 100,

                Amount = 100,
                Price = 0.01m,
                Company = "ABCD S.A",

                OperationDate = DateTime.Now,
                OperationType = OperationType.Buy,
                InvestmentType = InvestmentType.Derivative,

                FinancialInstitutionId = financialInstitution.Id,
                Id = Guid.NewGuid()
            };
            #endregion

            #region Act
            bool added = await OptionsBusiness.AddAsync(options);
            #endregion

            #region Assert
            Assert.That(added, Is.True);
            #endregion
        }

        [Test]
        public async Task Options_GetOptions_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            Options options = new()
            {
                Strike = 13m,
                ExerciseDate = new DateTime(2023, 4, 20),
                DerivativeType = DerivativeType.Call,

                Code = "ABCDA111",
                Total = 100,

                Amount = 100,
                Price = 0.01m,
                Company = "ABCD S.A",

                OperationDate = DateTime.Now,
                OperationType = OperationType.Buy,
                InvestmentType = InvestmentType.Derivative,

                FinancialInstitutionId = financialInstitution.Id,
                Id = Guid.NewGuid()
            };
            bool added = await OptionsBusiness.AddAsync(options);
            bool saved = await OptionsBusiness.Commit(added);
            #endregion

            #region Act
            Options optionsFromDb = await OptionsBusiness.GetAsync<Options>(x => x.Id == options.Id);
            #endregion

            #region Assert
            Assert.That(added, Is.True);
            Assert.That(optionsFromDb, Is.Not.Null);
            Assert.That(optionsFromDb.Id, Is.EqualTo(options.Id));
            #endregion
        }

        [Test]
        public async Task Options_ListOptions_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            Options Options1 = TestsSettings.GetOptions(financialInstitution, "ABCD 111");
            Options Options2 = TestsSettings.GetOptions(financialInstitution, "ABCD 222");
            bool added1 = await OptionsBusiness.AddAsync(Options1);
            bool added2 = await OptionsBusiness.AddAsync(Options2);
            await OptionsBusiness.Commit(added1 && added2);
            #endregion

            #region Act
            IReadOnlyCollection<Options> Optionss = await OptionsBusiness.ListAsync<Options>();
            #endregion

            #region Assert
            //Assert.That(added1, Is.True);
            //Assert.That(added2, Is.True);
            Assert.That(Optionss, Is.Not.Null);
            Assert.That(Optionss, Has.Count.EqualTo(2));
            #endregion
        }

        [Test]
        public async Task Options_ListAllEspecifiqueOptionFromHolder_Success()
        {
            #region Arrange
            FinancialInstitutionBusiness institutionBusiness = new FinancialInstitutionBusiness(Repository);
            ShareHolderBusiness holderBusiness = new ShareHolderBusiness(Repository);
            var holderList = await holderBusiness.ListAsync<ShareHolder>(x => x.Name == "Holder 1", x => x.Include(y => y.FinancialInstitutions));
            var holder = holderList.FirstOrDefault();

            string code = "ABCDN111";
            string investmentType = InvestmentType.Derivative.ToString();
            IEnumerable<FinancialInstitution> financialInstitutions = holder.FinancialInstitutions;

            IEnumerable<VariableIncome>? entitiesFromDb = null;
            List<VariableIncome>? entititesToReturn = new();
            InvestmentType investmentTypeEnum = Enum.Parse<InvestmentType>(investmentType);

            switch (investmentTypeEnum)
            {
                case InvestmentType.Derivative:
                    foreach (FinancialInstitution item1 in financialInstitutions)
                    {
                        entitiesFromDb = await OptionsBusiness.ListAsync<Options>
                            (x => x.Code == code && x.FinancialInstitutionId == item1.Id);
                        entititesToReturn.AddRange(entitiesFromDb);
                    }
                    break;
            }
            #endregion

            #region Act


            #endregion

            #region Assert
            Assert.Pass();
            #endregion
        }

        [Test]
        public async Task Options_UpdateOptions_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            Options Options = TestsSettings.GetOptions(financialInstitution);
            bool added = await OptionsBusiness.AddAsync(Options);
            Options.Company = "Company Updated Name";
            #endregion

            #region Act
            bool updated = OptionsBusiness.Update<Options>(Options);
            Options OptionsFromDb = await OptionsBusiness.GetAsync<Options>(x => x.Id == Options.Id);
            #endregion

            #region Assert
            Assert.That(added, Is.True);
            Assert.That(updated, Is.True);
            Assert.That(OptionsFromDb, Is.Not.Null);
            Assert.That(OptionsFromDb.Id, Is.EqualTo(Options.Id));
            #endregion
        }

        [Test]
        public async Task Options_DeleteOptions_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            Options Options = TestsSettings.GetOptions(financialInstitution);
            bool added = await OptionsBusiness.AddAsync(Options);
            #endregion

            #region Act
            object deleted = await OptionsBusiness.DeleteAsync<Options>(Options.Id);
            Options OptionsFromDb = await OptionsBusiness.GetAsync<Options>(x => x.Id == Options.Id);
            #endregion

            #region Assert
            Assert.That(added, Is.True);
            Assert.That(deleted, Is.True);
            Assert.That(OptionsFromDb, Is.Null);
            #endregion
        }

        private async Task SeedOptions(FinancialInstitution financialInstitution)
        {
            int amount = 100;
            decimal price = 13m;
            DateTime now = DateTime.Now;

            Options Options3 = new()
            {
                Code = "ABCD4",
                Total = (amount + 50) * (price + 0.15m),

                Amount = amount + 50,
                Price = price + 0.15m,
                Company = "ABCD S.A",

                OperationDate = now,
                OperationType = OperationType.Buy,
                InvestmentType = InvestmentType.Derivative,

                FinancialInstitutionId = financialInstitution.Id,
                Id = Guid.NewGuid()
            };

            Options Options4 = new()
            {
                Code = "XPTO3",
                Total = (amount + 50) * (price + 0.15m),

                Amount = amount,
                Price = price,
                Company = "XPTO S.A",

                OperationDate = now,
                OperationType = OperationType.Buy,
                InvestmentType = InvestmentType.Derivative,

                FinancialInstitutionId = financialInstitution.Id,
                Id = Guid.NewGuid()
            };

            Options Options5 = new()
            {
                Code = "XPTO3",
                Total = (amount + 50) * (price + 0.15m),

                Amount = amount,
                Price = price,
                Company = "XPTO S.A",

                OperationDate = now,
                OperationType = OperationType.Sell,
                InvestmentType = InvestmentType.Derivative,

                FinancialInstitutionId = financialInstitution.Id,
                Id = Guid.NewGuid()
            };

            await OptionsBusiness.AddAsync(Options3);
            await OptionsBusiness.AddAsync(Options4);
            await OptionsBusiness.AddAsync(Options5);
        }
    }
}
