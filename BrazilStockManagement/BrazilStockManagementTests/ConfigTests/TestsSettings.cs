using BrazilStockManagement.Entity;
using BrazilStockManagementTests.ConfigTests.ConfigServices;

namespace BrazilStockManagementTests.ConfigTests
{
    public class TestsSettings
    {
        public static async Task<StockContext> CreateDb(DatabaseOptions databaseOptions = DatabaseOptions.InMemoryDatabase, string jsonSection = "ConnectionStringModelOnlyRequiredProperties")
        {
            IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            string connectionString = ConnectionStringSettings.CreateConnectionString(configuration, jsonSection);

            DbContextOptions<StockContext> options = ConfigDbContextOptions.GetDbContextOptions<StockContext>(databaseOptions, connectionString);
            StockContext context = new StockContext(options);

            await new ServiceCollection().EnsureCreate<StockContext>(connectionString, databaseOptions);

            return context;
        }

        public static async Task<StockTestContext> CreateDbToSeed(DatabaseOptions databaseOptions = DatabaseOptions.SqlServer)
        {
            IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            string connectionString = ConnectionStringSettings.CreateConnectionString(configuration, "ConnectionStringTestFrontEnd");

            DbContextOptions<StockTestContext> options = ConfigDbContextOptions.GetDbContextOptions<StockTestContext>(databaseOptions, connectionString);
            StockTestContext context = new StockTestContext(options);

            await new ServiceCollection().EnsureCreate<StockTestContext>(connectionString, databaseOptions);

            return context;
        }

        public static async Task<ShareHolder> CreateAndGetShareHolderInDb(IStockManagerRepository repository, string name = "Share Holder 1")
        {
            ShareHolderBusiness shareHolderBusiness = new ShareHolderBusiness(repository);
            ShareHolder shareHolder = GetShareHolder(name);
            await shareHolderBusiness.AddAsync(shareHolder);
            return shareHolder;
        }

        public static async Task<FinancialInstitution> CreateAndGetFinancialInstitutionInDb(IStockManagerRepository repository, string name = "Financial Institution 1")
        {
            ShareHolder shareHolder = await CreateAndGetShareHolderInDb(repository);

            FinancialInstitutionBusiness financialInstitutionBusiness = new FinancialInstitutionBusiness(repository);

            FinancialInstitution financialInstitution = GetFinancialInstitution(shareHolder.Id, name);
            bool saved = await financialInstitutionBusiness.AddAsync(financialInstitution);
            await financialInstitutionBusiness.Commit(saved);
            return financialInstitution;
        }

        public static async Task<Stock> CreateAndGetStockInDb(IStockManagerRepository repository, string name = "Company 1")
        {
            FinancialInstitution financialInstitution = await CreateAndGetFinancialInstitutionInDb(repository);

            StockBusiness stockBusiness = new StockBusiness(repository);
            PortfolioBusiness portfolioBusiness = new PortfolioBusiness(repository);

            VariableIncomeServiceTests variableIncomeService = new VariableIncomeServiceTests(stockBusiness, portfolioBusiness: portfolioBusiness);

            Stock stock = GetStock(financialInstitution, name);

            await variableIncomeService.AddNewVariableIncome(stock);

            return stock;
        }

        public static async Task<Reit> CreateAndGetReitInDb(IStockManagerRepository repository, string name = "Company 1")
        {
            FinancialInstitution financialInstitution = await CreateAndGetFinancialInstitutionInDb(repository);

            ReitBusiness reitBusiness = new ReitBusiness(repository);

            Reit reit = GetReit(financialInstitution, name);
            await reitBusiness.Commit(await reitBusiness.AddAsync(reit));

            return reit;
        }

        public static ShareHolder GetShareHolder(string name = "Share Holder 1")
        {
            return new ShareHolder()
            {
                Name = name
            };
        }

        public static FinancialInstitution GetFinancialInstitution(Guid shareHolderId, string name = "Financial Institution 1", string account = "123-4")
        {
            return new FinancialInstitution()
            {
                Name = name,
                Account = account,
                ShareHolderId = shareHolderId
            };
        }

        public static FixedDeposit GetFixedDeposit(Guid financialInstitutionId, string company = "Company 1")
        {
            DateTime now = DateTime.Now;
            FixedDeposit fixedDeposit = new FixedDeposit()
            {
                FinancialInstitutionId = financialInstitutionId,

                OperationDate = now,
                OperationType = OperationType.Buy,
                InvestmentType = InvestmentType.FixedDeposit,

                Amount = 1,
                Price = 1234.56m,
                Company = company,

                Profitability = 102.5m,
                IndexType = IndexType.CDI,
                LiquidityDate = now,
                RecordDate = now,
                DueDate = new DateTime(now.Year + 1, now.Month, now.Day),
                FixedDepositsType = FixedDepositsType.CDB
            };

            return fixedDeposit;
        }

        /// <summary>
        /// Default values:
        /// Amount = 10 |
        /// Price = 10
        /// </summary>
        public static Stock GetStock(FinancialInstitution financialInstitution, string company = "ABCD Company", int amount = 10, decimal price = 10m)
        {
            DateTime now = DateTime.Now;

            Stock stock = new Stock()
            {
                FinancialInstitutionId = financialInstitution.Id,

                OperationDate = now,
                OperationType = OperationType.Buy,
                InvestmentType = InvestmentType.Stock,

                Amount = amount,
                Price = price,
                Company = company,

                Code = company.Substring(0, 4) + "3",
                Total = amount * price
            };

            return stock;
        }
        
        /// <summary>
        /// Default values: amount=1000 \ price=R$0,50
        /// </summary>
        public static Options GetOptions(FinancialInstitution financialInstitution, string code = "ABCDA111", int amount = 1000, decimal price = 0.5m)
        {
            DateTime now = DateTime.Now;

            Options options = new()
            {
                Strike = 13m,
                ExerciseDate = new DateTime(now.Year, now.Month, 20),
                DerivativeType = DerivativeType.Call,

                Code = code,
                Total = amount * price,

                Amount = amount,
                StockCode = code.Substring(0, 4) + "3",
                Price = price,
                Company = "ABCD S.A",

                OperationDate = new DateTime(now.Year, now.Month, now.Day-1),
                OperationType = OperationType.Buy,
                InvestmentType = InvestmentType.Derivative,

                FinancialInstitutionId = financialInstitution.Id,
                Id = Guid.NewGuid()
            };

            return options;
        }

        /// <summary>
        /// Default values:
        /// Amount = 10 |
        /// Price = 10
        /// </summary>
        public static Reit GetReit(FinancialInstitution financialInstitution, string company = "Company 1", int amount = 10, decimal price = 10m)
        {
            DateTime now = DateTime.Now;

            Reit reit = new Reit()
            {
                FinancialInstitutionId = financialInstitution.Id,

                OperationDate = now,
                OperationType = OperationType.Buy,
                InvestmentType = InvestmentType.Reit,

                Amount = amount,
                Price = price,
                Company = company,

                Code = "FIIA11",
                Total = amount * price
            };

            return reit;
        }

        public static BonusShare GetBonusShare(Guid portfolioId)
        {
            DateTime now = DateTime.Now;

            BonusShare bonusShare = new BonusShare()
            {
                Proportion = 0.10m,

                Value = 13.15m,
                RecordDate = now,
                ExDate = new DateTime(now.Year, now.Month, now.Day + 1),
                PaymentDate = new DateTime(now.Year, now.Month + 1, now.Day),
                PortfolioId = portfolioId
            };

            return bonusShare;
        }

        public static Dividend GetDividend(Guid portfolioId, DividendType dividendType = DividendType.Dividendo)
        {
            DateTime now = new DateTime(2022, 12, 1);

            Dividend dividend = new Dividend()
            {
                DividendType = dividendType,

                Value = 13.15m,
                Amount = 100,
                RecordDate = now,
                ExDate = now.AddDays(1),
                PaymentDate = now.AddMonths(1),
                PortfolioId = portfolioId
            };

            return dividend;
        }

        public static TEntity GetVariableIncomeToSell<TEntity>(TEntity entity, int amount = 100, decimal price = 11m) where TEntity : VariableIncome
        {
            entity.Amount = amount;
            entity.OperationType = OperationType.Sell;
            entity.Price = price;
            entity.Total = entity.Price * entity.Amount;

            return entity;
        }
    }
}
