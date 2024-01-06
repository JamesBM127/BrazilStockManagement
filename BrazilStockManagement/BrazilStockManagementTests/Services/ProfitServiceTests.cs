using BrazilStockManagement.Business.Implementation;
using JBMDatabase.Repos;

namespace BrazilStockManagementTests.Services
{
    public class ProfitServiceTests
    {
        public StockContext Context { get; set; }
        public IStockManagerRepository Repository;
        public ProfitService ProfitService { get; set; }
        public VariableIncomeServiceTests VariableIncomeService { get; set; }
        private bool DeleteTestDb = true;
        public OptionsBusiness OptionsBusiness { get; set; }

        [TearDown]
        public void TearDown()
        {
            //try
            //{
            //    Context.Database.EnsureDeleted();
            //}
            //catch { }

            //Context = null;
        }

        [SetUp]
        public async Task SetUp()
        {
            Context = await TestsSettings.CreateDb(DatabaseOptions.SqlServer);

            try
            {
                if (DeleteTestDb)
                {
                    Context.Database.EnsureDeleted();
                    Context.Database.EnsureCreated();
                }
            }
            finally { DeleteTestDb = false; }

            Repository = new StockManagerRepository(Context);
            ProfitBusiness profitBusiness = new ProfitBusiness(Repository);
            OptionsBusiness = new OptionsBusiness(Repository);

            StockBusiness stockBusiness = new StockBusiness(Repository);
            PortfolioBusiness portfolioBusiness = new PortfolioBusiness(Repository);
            VariableIncomeService = new VariableIncomeServiceTests(stockBusiness, portfolioBusiness: portfolioBusiness);

            ProfitService = new ProfitService(profitBusiness);

        }

        #region Normal Operation Buy then Sell
        [Test]
        public async Task ProfitService_GetAllNotTaxExamptProfitsBuyThenSell_Success()
        {
            #region Arrange
            await ArrangeBuyThenSell();
            #endregion

            #region Act
            IReadOnlyCollection<Profit> profits = await ProfitService.GetProfits();
            #endregion

            #region Assert
            Assert.That(profits.Count, Is.EqualTo(2));
            #endregion
        }

        [Test]
        public async Task ProfitService_GetAllTaxExamptProfitsBuyThenSell_Success()
        {
            #region Arrange
            await ArrangeBuyThenSell();
            #endregion

            #region Act
            IReadOnlyCollection<Profit> profits = await ProfitService.GetProfits(true);
            #endregion

            #region Assert
            Assert.That(profits.Count, Is.EqualTo(1));
            #endregion
        }
        
        [Test]
        public async Task ProfitService_GetAllNotTaxExamptProfitsByDateBuyThenSell_Success()
        {
            #region Arrange
            await ArrangeBuyThenSell();
            #endregion

            #region Act
            IReadOnlyCollection<Profit> profits = await ProfitService.GetProfits(DateTime.Now.Year, DateTime.Now.Month);
            #endregion

            #region Assert
            Assert.That(profits.Count, Is.EqualTo(1));
            #endregion
        }

        [Test]
        public async Task ProfitService_GetAllTaxExamptProfitsByDateBuyThenSell_Success()
        {
            #region Arrange
            await ArrangeBuyThenSell();
            #endregion

            #region Act
            IReadOnlyCollection<Profit> profits = await ProfitService.GetProfits(DateTime.Now.Year, DateTime.Now.Month + 3, true);
            #endregion

            #region Assert
            Assert.That(profits.Count, Is.EqualTo(1));
            #endregion
        }

        [Test]
        public async Task ProfitService_GetLossThenGetProfitCompensatingTheLoss_Success()
        {
            #region Arrange
            await ArrangeBuyThenSellOneOfKindOfProfitType();
            DateTime firstBuyDate = DateTime.Now;
            DateTime secondBuyDate = DateTime.Now.AddMonths(1);
            #endregion

            #region Act
            IReadOnlyCollection<Profit> profits1stBuy = await ProfitService.GetProfits(firstBuyDate.Year, firstBuyDate.Month);
            IReadOnlyCollection<Profit> profits2ndBuy = await ProfitService.GetProfits(secondBuyDate.Year, secondBuyDate.Month);

            decimal profitDayTrade = profits1stBuy.Where(x => x.ProfitType == ProfitType.DayTrade).Select(x => x.Value).Sum();
            profitDayTrade += profits2ndBuy.Where(x => x.ProfitType == ProfitType.DayTrade).Select(x => x.Value).Sum();

            decimal profitSwingTrade = profits1stBuy.Where(x => x.ProfitType == ProfitType.SwingTrade).Select(x => x.Value).Sum();
            profitSwingTrade += profits2ndBuy.Where(x => x.ProfitType == ProfitType.SwingTrade).Select(x => x.Value).Sum();
            #endregion

            #region Assert
            Assert.That(profitDayTrade, Is.Zero);
            Assert.That(profitSwingTrade, Is.Zero);
            #endregion
        }

        #endregion

        #region Sold Operation Sell then Buy
        [Test]
        public async Task ProfitService_GetAllNotTaxExamptProfitsSellThenBuy_Success()
        {
            #region Arrange
            await ArrangeSellThenBuy();
            #endregion

            #region Act
            IReadOnlyCollection<Profit> profits = await ProfitService.GetProfits();
            #endregion

            #region Assert
            Assert.That(profits.Count, Is.EqualTo(4));
            #endregion
        }

        [Test]
        public async Task ProfitService_GetAllTaxExamptProfitsSellThenBuy_Success()
        {
            #region Arrange
            await ArrangeSellThenBuy();
            #endregion

            #region Act
            IReadOnlyCollection<Profit> profits = await ProfitService.GetProfits(true);
            #endregion

            #region Assert
            Assert.That(profits.Count, Is.EqualTo(0));
            #endregion
        }

        [Test]
        public async Task ProfitService_GetAllNotTaxExamptProfitsByDateSellThenBuy_Success()
        {
            #region Arrange
            await ArrangeSellThenBuy();
            #endregion

            #region Act
            IReadOnlyCollection<Profit> profits = await ProfitService.GetProfits(DateTime.Now.Year, DateTime.Now.Month);
            #endregion

            #region Assert
            Assert.That(profits.Count, Is.EqualTo(2));
            #endregion
        }

        [Test]
        public async Task ProfitService_GetAllTaxExamptProfitsByDateSellThenBuy_Success()
        {
            #region Arrange
            await ArrangeSellThenBuy();
            #endregion

            #region Act
            IReadOnlyCollection<Profit> profits = await ProfitService.GetProfits(DateTime.Now.Year, DateTime.Now.Month + 3, true);
            #endregion

            #region Assert
            Assert.That(profits.Count, Is.EqualTo(0));
            #endregion
        }

        #endregion

        #region Options

        [TestCase(OperationType.Sell, DerivativeType.Call)]
        [TestCase(OperationType.Sell, DerivativeType.Put)]
        public async Task ProfitService_SellOptionThenBuyWithLoss_Success(OperationType OperationType, DerivativeType DerivativeType)
        {
            #region Arrange
            DeleteTestDb = true;
            await ArrangeSellThenBuyOptions(OperationType, DerivativeType, VariableIncomeService, Repository);
            IReadOnlyCollection<Options> optionsList = await OptionsBusiness.ListAsync<Options>();
            Options? optionSell = optionsList.FirstOrDefault();
            Options optionsBuy = new Options();
            optionsBuy.OperationType = OperationType.Buy;
            optionsBuy.Price += 0.1m;
            optionsBuy.Amount = 100;
            optionsBuy.Total = optionsBuy.Amount * optionsBuy.Price;
            optionsBuy.OperationDate = optionsBuy.OperationDate.AddDays(1);

            await VariableIncomeService.AddNewVariableIncome(optionsBuy);
            #endregion

            #region Act
            IReadOnlyCollection<Profit> profits = await ProfitService.GetProfits();
            Profit? profit = profits.FirstOrDefault();
            #endregion

            #region Assert
            Assert.That(profit.Value, Is.EqualTo((optionSell.Price - optionsBuy.Price)*optionsBuy.Amount));
            #endregion
        }

        [TestCase(OperationType.Sell, DerivativeType.Call)]
        [TestCase(OperationType.Sell, DerivativeType.Put)]
        public async Task ProfitService_SellOptionThenBuyWithGain_Success(OperationType OperationType, DerivativeType DerivativeType)
        {
            #region Arrange
            DeleteTestDb = true;
            await ArrangeSellThenBuyOptions(OperationType, DerivativeType, VariableIncomeService, Repository);
            IReadOnlyCollection<Options> optionsList = await OptionsBusiness.ListAsync<Options>();
            Options? optionSell = optionsList.FirstOrDefault();
            Options optionsBuy = new Options();
            optionsBuy.OperationType = OperationType.Buy;
            optionsBuy.Price -= 0.1m;
            optionsBuy.Amount = 100;
            optionsBuy.Total = optionsBuy.Amount * optionsBuy.Price;
            optionsBuy.OperationDate = optionsBuy.OperationDate.AddDays(1);

            await VariableIncomeService.AddNewVariableIncome(optionsBuy);
            #endregion

            #region Act
            IReadOnlyCollection<Profit> profits = await ProfitService.GetProfits();
            Profit? profit = profits.FirstOrDefault();
            #endregion

            #region Assert
            Assert.That(profit.Value, Is.EqualTo((optionSell.Price - optionsBuy.Price)*optionsBuy.Amount));
            #endregion
        }
        
        [TestCase(OperationType.Buy, DerivativeType.Call)]
        [TestCase(OperationType.Buy, DerivativeType.Put)]
        public async Task ProfitService_BuyOptionThenSellWithGain_Success(OperationType OperationType, DerivativeType DerivativeType)
        {
            #region Arrange
            DeleteTestDb = true;
            await ArrangeSellThenBuyOptions(OperationType, DerivativeType, VariableIncomeService, Repository);
            IReadOnlyCollection<Options> optionsList = await OptionsBusiness.ListAsync<Options>();
            Options? optionBuy = optionsList.FirstOrDefault();
            Options optionsSell = new Options();
            optionsSell.OperationType = OperationType.Sell;
            optionsSell.Price += 0.1m;
            optionsSell.Amount = 100;
            optionsSell.Total = optionsSell.Amount * optionsSell.Price;
            optionsSell.OperationDate = optionsSell.OperationDate.AddDays(1);

            await VariableIncomeService.AddNewVariableIncome(optionsSell);
            #endregion

            #region Act
            IReadOnlyCollection<Profit> profits = await ProfitService.GetProfits();
            Profit? profit = profits.FirstOrDefault();
            decimal profitValue = (optionsSell.Price - optionBuy.Price) * optionsSell.Amount;
            #endregion

            #region Assert
            Assert.That(profit.Value, Is.EqualTo(profitValue));
            #endregion
        }
          
        [TestCase(OperationType.Buy, DerivativeType.Call)]
        [TestCase(OperationType.Buy, DerivativeType.Put)]
        public async Task ProfitService_BuyOptionThenSellWithLoss_Success(OperationType OperationType, DerivativeType DerivativeType)
        {
            #region Arrange
            DeleteTestDb = true;
            await ArrangeSellThenBuyOptions(OperationType, DerivativeType, VariableIncomeService, Repository);
            IReadOnlyCollection<Options> optionsList = await OptionsBusiness.ListAsync<Options>();
            Options? optionBuy = optionsList.FirstOrDefault();
            Options optionsSell = new Options();
            optionsSell.OperationType = OperationType.Sell;
            optionsSell.Price -= 0.1m;
            optionsSell.Amount = 100;
            optionsSell.Total = optionsSell.Amount * optionsSell.Price;
            optionsSell.OperationDate = optionsSell.OperationDate.AddDays(1);

            await VariableIncomeService.AddNewVariableIncome(optionsSell);
            #endregion

            #region Act
            IReadOnlyCollection<Profit> profits = await ProfitService.GetProfits();
            Profit? profit = profits.FirstOrDefault();
            decimal profitValue = (optionsSell.Price - optionBuy.Price) * optionsSell.Amount;
            #endregion

            #region Assert
            Assert.That(profit.Value, Is.EqualTo(profitValue));
            #endregion
        }

        #endregion

        private async Task ArrangeBuyThenSell()
        {
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);

            Stock stockBuy = TestsSettings.GetStock(financialInstitution, amount: 10000);
            bool addedBuy = await VariableIncomeService.AddNewVariableIncome<Stock>(stockBuy);

            Stock stockSell1 = TestsSettings.GetStock(financialInstitution);
            stockSell1.OperationType = OperationType.Sell;
            stockSell1.Price = stockBuy.Price - 1;
            stockSell1.Amount = 10;
            stockSell1.Total = stockSell1.Price * stockSell1.Amount;

            Stock stockSell2 = TestsSettings.GetStock(financialInstitution);
            stockSell2.OperationType = OperationType.Sell;
            stockSell2.OperationDate = stockBuy.OperationDate.AddMonths(1);
            stockSell2.Price = stockBuy.Price + 1;
            stockSell2.Amount = 5000;
            stockSell2.Total = stockSell2.Price * stockSell2.Amount;

            Stock stockSell3 = TestsSettings.GetStock(financialInstitution);
            stockSell3.OperationType = OperationType.Sell;
            stockSell3.OperationDate = stockBuy.OperationDate.AddMonths(3);
            stockSell3.Price = stockBuy.Price + 1;
            stockSell3.Amount = 100;
            stockSell3.Total = stockSell3.Price * stockSell3.Amount;

            bool added1 = await VariableIncomeService.AddNewVariableIncome<Stock>(stockSell1);
            bool added2 = await VariableIncomeService.AddNewVariableIncome<Stock>(stockSell2);
            bool added3 = await VariableIncomeService.AddNewVariableIncome<Stock>(stockSell3);
        }
        
        private async Task ArrangeBuyThenSellOneOfKindOfProfitType()
        {
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);

            Stock stockBuy1 = TestsSettings.GetStock(financialInstitution, amount: 10000);
            Stock stockBuy2 = TestsSettings.GetStock(financialInstitution, amount: 10000, company: "XPTO Company");
            stockBuy2.OperationDate = stockBuy1.OperationDate.AddMonths(1);
            await VariableIncomeService.AddNewVariableIncome<Stock>(stockBuy1);
            await VariableIncomeService.AddNewVariableIncome<Stock>(stockBuy2);

            //DayTrade
            //Prejuizo
            Stock stockSellDayTrade1 = TestsSettings.GetStock(financialInstitution);
            stockSellDayTrade1.OperationType = OperationType.Sell;
            stockSellDayTrade1.Price = stockBuy1.Price - 1;
            stockSellDayTrade1.Amount = 1000;
            stockSellDayTrade1.Total = stockSellDayTrade1.Price * stockSellDayTrade1.Amount;

            //Lucro compensando prejuizo
            Stock stockSellDayTrade2 = TestsSettings.GetStock(financialInstitution, company: "XPTO Company");
            stockSellDayTrade2.OperationType = OperationType.Sell;
            stockSellDayTrade2.OperationDate = stockBuy2.OperationDate;
            stockSellDayTrade2.Price = stockBuy1.Price + 1;
            stockSellDayTrade2.Amount = 1000;
            stockSellDayTrade2.Total = stockSellDayTrade2.Price * stockSellDayTrade2.Amount;

            //SwingTrade
            //Prejuizo
            Stock stockSellSwingTrade1 = TestsSettings.GetStock(financialInstitution);
            stockSellSwingTrade1.OperationType = OperationType.Sell;
            stockSellSwingTrade1.OperationDate = stockBuy1.OperationDate.AddDays(1);
            stockSellSwingTrade1.Price = stockBuy1.Price - 1;
            stockSellSwingTrade1.Amount = 1000;
            stockSellSwingTrade1.Total = stockSellSwingTrade1.Price * stockSellSwingTrade1.Amount;

            //Lucro compensando prejuizo
            Stock stockSellSwingTrade2 = TestsSettings.GetStock(financialInstitution);
            stockSellSwingTrade2.OperationType = OperationType.Sell;
            stockSellSwingTrade2.OperationDate = stockBuy2.OperationDate.AddDays(1);
            stockSellSwingTrade2.Price = stockBuy1.Price + 1;
            stockSellSwingTrade2.Amount = 1000;
            stockSellSwingTrade2.Total = stockSellSwingTrade2.Price * stockSellSwingTrade2.Amount;

            await VariableIncomeService.AddNewVariableIncome<Stock>(stockSellDayTrade1);
            await VariableIncomeService.AddNewVariableIncome<Stock>(stockSellDayTrade2);
            await VariableIncomeService.AddNewVariableIncome<Stock>(stockSellSwingTrade1);
            await VariableIncomeService.AddNewVariableIncome<Stock>(stockSellSwingTrade2);
        }
        
        private async Task ArrangeSellThenBuy()
        {
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);

            Stock stockSell = TestsSettings.GetStock(financialInstitution, amount: 10000);
            stockSell.OperationType = OperationType.Sell;
            bool addedBuy = await VariableIncomeService.AddNewVariableIncome<Stock>(stockSell);

            //Lucro
            Stock stockBuy1 = TestsSettings.GetStock(financialInstitution);
            stockBuy1.OperationType = OperationType.Buy;
            stockBuy1.OperationDate = stockSell.OperationDate.AddDays(1);
            stockBuy1.Price = stockSell.Price - 1;
            stockBuy1.Amount = 10;
            stockBuy1.Total = stockBuy1.Price * stockBuy1.Amount;

            //Prejuizo
            Stock stockBuy2 = TestsSettings.GetStock(financialInstitution);
            stockBuy2.OperationType = OperationType.Buy;
            stockBuy2.OperationDate = stockSell.OperationDate.AddMonths(1);
            stockBuy2.Price = stockSell.Price + 1;
            stockBuy2.Amount = 5000;
            stockBuy2.Total = stockBuy2.Price * stockBuy2.Amount;

            //Prejuizo
            Stock stockBuy3 = TestsSettings.GetStock(financialInstitution);
            stockBuy3.OperationType = OperationType.Buy;
            stockBuy3.OperationDate = stockSell.OperationDate.AddMonths(3);
            stockBuy3.Price = stockSell.Price + 1;
            stockBuy3.Amount = 100;
            stockBuy3.Total = stockBuy3.Price * stockBuy3.Amount;

            //Lucro
            Stock stockBuy4 = TestsSettings.GetStock(financialInstitution);
            stockBuy4.OperationType = OperationType.Buy;
            stockBuy4.OperationDate = stockSell.OperationDate.AddDays(1);
            stockBuy4.Price = stockSell.Price - 7;
            stockBuy4.Amount = 4000;
            stockBuy4.Total = stockBuy4.Price * stockBuy4.Amount;

            await VariableIncomeService.AddNewVariableIncome<Stock>(stockBuy1);
            await VariableIncomeService.AddNewVariableIncome<Stock>(stockBuy2);
            await VariableIncomeService.AddNewVariableIncome<Stock>(stockBuy3);
            await VariableIncomeService.AddNewVariableIncome<Stock>(stockBuy4);
        }
        
        private async Task<Guid> ArrangeSellThenBuyOptions(OperationType OperationType, DerivativeType DerivativeType, VariableIncomeServiceTests VariableIncomeService, IStockManagerRepository Repository)
        {
            Guid holderId = await OptionsServiceConfig.AssertOptions(OperationType, DerivativeType, VariableIncomeService, Repository);
            return holderId;
        }

        #region Arrange
        #endregion

        #region Act
        #endregion

        #region Assert
        #endregion
    }
}
