using BrazilStockManagementTests.ConfigTests.ConfigServices;

namespace BrazilStockManagementTests.Services
{
    public class StockServiceTests
    {
        public StockContext Context { get; set; }
        public IStockManagerRepository Repository;
        public VariableIncomeServiceTests VariableIncomeService { get; set; }

        [TearDown]
        public void TearDown()
        {
            try
            {
                Context.Database.EnsureDeleted();
            }
            catch { }

            Context = null;
        }

        [SetUp]
        public async Task SetUp()
        {
            Context = await TestsSettings.CreateDb(DatabaseOptions.InMemoryDatabase);

            Repository = new StockManagerRepository(Context);
            StockBusiness StockBusiness = new StockBusiness(Repository);
            PortfolioBusiness PortfolioBusiness = new PortfolioBusiness(Repository);
            VariableIncomeService = new VariableIncomeServiceTests(StockBusiness, null, null, PortfolioBusiness);
        }

        #region Buy First / Sell Last

        [TestCase(20, 5)]
        [TestCase(20, 10)]
        [TestCase(5, 5)]
        [TestCase(5, 10)]
        //Regular profit and Loss
        public async Task StockProfit_BuyThenSellProfitAndLoss_Success(decimal sellPrice, int amountSell)
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);

            Stock stockBuy = TestsSettings.GetStock(financialInstitution);
            bool addedBuy = await VariableIncomeService.AddNewVariableIncome<Stock>(stockBuy);

            Stock stockSell = TestsSettings.GetStock(financialInstitution);
            stockSell.OperationType = OperationType.Sell;
            stockSell.Price = sellPrice;
            stockSell.Amount = amountSell;
            stockSell.Total = sellPrice * amountSell;
            #endregion

            #region Act
            bool addedSell = await VariableIncomeService.AddNewVariableIncome<Stock>(stockSell);

            IReadOnlyCollection<Profit> profits = await Repository.ListAsync<Profit>();

            decimal profitFromDb = profits.Sum(x => x.Value);
            decimal profitValue = stockSell.Total - (stockBuy.Price * amountSell);
            #endregion

            #region Assert
            Assert.That(addedBuy && addedSell, Is.True);
            Assert.That(profitFromDb, Is.EqualTo(profitValue));
            #endregion
        }

        [TestCase(5)]
        [TestCase(10)]
        //Buy stock and sell at the same price
        public async Task StockProfit_BuyThenSellAtTheSamePrice_Success(int amountSell)
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);

            Stock stockBuy = TestsSettings.GetStock(financialInstitution);
            bool addedBuy = await VariableIncomeService.AddNewVariableIncome<Stock>(stockBuy);

            Stock stockSell = TestsSettings.GetStock(financialInstitution);
            stockSell.Total = stockSell.Price * amountSell;
            stockSell.OperationType = OperationType.Sell;
            #endregion

            #region Act
            bool addedSell = await VariableIncomeService.AddNewVariableIncome<Stock>(stockSell);

            IReadOnlyCollection<Profit> profits = await Repository.ListAsync<Profit>();

            decimal profitFromDb = profits.Sum(x => x.Value);
            decimal profitValue = stockSell.Total - (stockBuy.Price * amountSell);
            #endregion

            #region Assert
            Assert.That(addedBuy && addedSell, Is.True);
            Assert.That(profitValue, Is.Zero);
            Assert.That(profitFromDb, Is.EqualTo(profitValue));
            #endregion
        }

        [TestCase(-1, 2, 1, 5)]
        [TestCase(1, -2, 1, 5)]
        [TestCase(-3, 7, 1, 5)]
        [TestCase(2, -5, 1, 5)]
        //Buy then sell with profit, then sell more but with a bigger loss reversing to end with with loss, and vice versa
        public async Task StockProfit_BuyThenSellReversingTheProfitToLossAndViceVersa_Success(decimal priceSell1, decimal priceSell2,
                                                                                              int amountSell1, int amountSell2)
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);

            Stock stockBuy = TestsSettings.GetStock(financialInstitution);
            bool addedBuy = await VariableIncomeService.AddNewVariableIncome<Stock>(stockBuy);

            //1st sell
            Stock stockSell1 = TestsSettings.GetStock(financialInstitution);
            stockSell1.Price = stockBuy.Price + priceSell1;
            stockSell1.Amount = amountSell1;
            stockSell1.Total = stockSell1.Price * amountSell1;
            stockSell1.OperationType = OperationType.Sell;

            //2nd sell
            Stock stockSell2 = TestsSettings.GetStock(financialInstitution);
            stockSell2.Price = stockBuy.Price + priceSell2;
            stockSell2.Amount = amountSell2;
            stockSell2.Total = stockSell2.Price * amountSell2;
            stockSell2.OperationType = OperationType.Sell;
            #endregion

            #region Act
            bool addedSell1 = await VariableIncomeService.AddNewVariableIncome<Stock>(stockSell1);
            bool addedSell2 = await VariableIncomeService.AddNewVariableIncome<Stock>(stockSell2);

            IReadOnlyCollection<Profit> profits = await Repository.ListAsync<Profit>();

            decimal profitFromDb = profits.Sum(x => x.Value);
            decimal profitValue = (stockSell1.Total + stockSell2.Total) - (stockBuy.Price * (amountSell1 + amountSell2));
            #endregion

            #region Assert
            Assert.That(addedBuy && addedSell1 && addedSell2, Is.True);
            Assert.That(profitFromDb, Is.EqualTo(profitValue));
            #endregion
        }

        [TestCase(1, 3)]
        [TestCase(-1, 3)]
        [TestCase(-3, 3)]
        [TestCase(7, 3)]
        //Buy then sell, get profit then loss, and vice versa, to finish with not gain neither loss
        public async Task StockProfit_BuyThenSellAtTheEndZeroProfitZeroLoss_Success(decimal priceSell, int amountSell)
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);

            Stock stockBuy = TestsSettings.GetStock(financialInstitution);
            bool addedBuy = await VariableIncomeService.AddNewVariableIncome<Stock>(stockBuy);

            //1st sell with Loss
            Stock stockSell1 = TestsSettings.GetStock(financialInstitution);
            stockSell1.Price = stockBuy.Price + priceSell;
            stockSell1.Amount = amountSell;
            stockSell1.Total = stockSell1.Price * amountSell;
            stockSell1.OperationType = OperationType.Sell;

            //2nd sell with profit biiggers than loss
            Stock stockSell2 = TestsSettings.GetStock(financialInstitution);
            stockSell2.Price = stockBuy.Price - priceSell;
            stockSell2.Amount = amountSell;
            stockSell2.Total = stockSell2.Price * amountSell;
            stockSell2.OperationType = OperationType.Sell;
            #endregion

            #region Act
            bool addedSell1 = await VariableIncomeService.AddNewVariableIncome<Stock>(stockSell1);
            bool addedSell2 = await VariableIncomeService.AddNewVariableIncome<Stock>(stockSell2);

            IReadOnlyCollection<Profit> profits = await Repository.ListAsync<Profit>();

            decimal profitFromDb = profits.Sum(x => x.Value);
            decimal profitValue = (stockSell1.Total + stockSell2.Total) - (stockBuy.Price * (stockSell1.Amount + stockSell2.Amount));
            #endregion

            #region Assert
            Assert.That(addedBuy && addedSell1 && addedSell2, Is.True);
            Assert.That(profitFromDb, Is.Zero);
            Assert.That(profitFromDb, Is.EqualTo(profitValue));
            #endregion
        }

        #endregion

        #region Sell First / Buy Last

        [TestCase(20, 5)]  //Loss
        [TestCase(20, 10)] //Loss
        [TestCase(5, 5)]  //Profit
        [TestCase(5, 10)] //Profit
        //Regular profit and Loss
        public async Task StockProfit_SellThenBuyProfitAndLoss_Success(decimal buyPrice, int amountBuy)
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);

            Stock stockSell = TestsSettings.GetStock(financialInstitution);
            stockSell.OperationType = OperationType.Sell;
            bool addedSell = await VariableIncomeService.AddNewVariableIncome<Stock>(stockSell);

            Stock stockBuy = TestsSettings.GetStock(financialInstitution);
            stockBuy.Price = buyPrice;
            stockBuy.Amount = amountBuy;
            stockBuy.Total = buyPrice * amountBuy;
            #endregion

            #region Act
            bool addedBuy = await VariableIncomeService.AddNewVariableIncome<Stock>(stockBuy);

            IReadOnlyCollection<Profit> profits = await Repository.ListAsync<Profit>();

            decimal profitFromDb = profits.Sum(x => x.Value);
            decimal profitValue = (stockSell.Price * amountBuy) - (stockBuy.Price * amountBuy);
            #endregion

            #region Assert
            Assert.That(addedSell && addedBuy, Is.True);
            Assert.That(profitFromDb, Is.EqualTo(profitValue));
            #endregion
        }

        [TestCase(5)]
        [TestCase(10)]
        //Sell stock and buy at the same price
        public async Task StockProfit_SellThenBuyAtTheSamePrice_Success(int amountBuy)
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);

            Stock stockSell = TestsSettings.GetStock(financialInstitution);
            stockSell.OperationType = OperationType.Sell;
            bool addedSell = await VariableIncomeService.AddNewVariableIncome<Stock>(stockSell);

            Stock stockBuy = TestsSettings.GetStock(financialInstitution);
            stockBuy.Total = stockBuy.Price * amountBuy;
            #endregion

            #region Act
            bool addedBuy = await VariableIncomeService.AddNewVariableIncome<Stock>(stockBuy);

            IReadOnlyCollection<Profit> profits = await Repository.ListAsync<Profit>();

            decimal profitFromDb = profits.Sum(x => x.Value);
            decimal profitValue = (stockSell.Price * amountBuy) - (stockBuy.Price * amountBuy);
            #endregion

            #region Assert
            Assert.That(addedSell && addedBuy, Is.True);
            Assert.That(profitValue, Is.Zero);
            Assert.That(profitFromDb, Is.EqualTo(profitValue));
            #endregion
        }

        [TestCase(-1, 2, 1, 5)]
        [TestCase(1, -2, 1, 5)]
        [TestCase(-3, 7, 1, 9)]
        [TestCase(2, -5, 1, 10)]
        //Sell then Buy with profit, then sell more but with a bigger loss reversing to end with with loss, and vice versa
        public async Task StockProfit_SellThenBuyReversingTheProfitToLossAndViceVersa_Success(decimal priceBuy1, decimal priceBuy2,
                                                                                              int amountBuy1, int amountBuy2)
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);

            Stock stockSell = TestsSettings.GetStock(financialInstitution);
            stockSell.OperationType = OperationType.Sell;
            bool addedSell = await VariableIncomeService.AddNewVariableIncome<Stock>(stockSell);

            //1st buy
            Stock stockBuy1 = TestsSettings.GetStock(financialInstitution);
            stockBuy1.Price = stockSell.Price + priceBuy1;
            stockBuy1.Amount = amountBuy1;
            stockBuy1.Total = stockBuy1.Price * amountBuy1;

            //2nd buy
            Stock stockBuy2 = TestsSettings.GetStock(financialInstitution);
            stockBuy2.Price = stockSell.Price + priceBuy2;
            stockBuy2.Amount = amountBuy2;
            stockBuy2.Total = stockBuy2.Price * amountBuy2;
            #endregion

            #region Act
            bool addedBuy1 = await VariableIncomeService.AddNewVariableIncome<Stock>(stockBuy1);
            bool addedBuy2 = await VariableIncomeService.AddNewVariableIncome<Stock>(stockBuy2);

            IReadOnlyCollection<Profit> profits = await Repository.ListAsync<Profit>();

            decimal profitFromDb = profits.Sum(x => x.Value);
            decimal profitValue = (stockBuy1.Total + stockBuy2.Total) - (stockSell.Price * (amountBuy1 + amountBuy2));
            #endregion

            #region Assert
            Assert.That(addedSell && addedBuy1 && addedBuy2, Is.True);
            Assert.That(Math.Abs(profitFromDb), Is.EqualTo(Math.Abs(profitValue)));
            #endregion
        }

        [TestCase(1, 3)]
        [TestCase(-1, 3)]
        [TestCase(-3, 3)]
        [TestCase(7, 3)]
        //Buy then sell, get profit then loss, and vice versa, to finish with not gain neither loss
        public async Task StockProfit_SellThenBuyAtTheEndZeroProfitZeroLoss_Success(decimal priceSell, int amountSell)
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);

            Stock stockBuy = TestsSettings.GetStock(financialInstitution);
            bool addedBuy = await VariableIncomeService.AddNewVariableIncome<Stock>(stockBuy);

            //1st sell with Loss
            Stock stockSell1 = TestsSettings.GetStock(financialInstitution);
            stockSell1.Price = stockBuy.Price + priceSell;
            stockSell1.Amount = amountSell;
            stockSell1.Total = stockSell1.Price * amountSell;
            stockSell1.OperationType = OperationType.Sell;

            //2nd sell with profit biiggers than loss
            Stock stockSell2 = TestsSettings.GetStock(financialInstitution);
            stockSell2.Price = stockBuy.Price - priceSell;
            stockSell2.Amount = amountSell;
            stockSell2.Total = stockSell2.Price * amountSell;
            stockSell2.OperationType = OperationType.Sell;
            #endregion

            #region Act
            bool addedSell1 = await VariableIncomeService.AddNewVariableIncome<Stock>(stockSell1);
            bool addedSell2 = await VariableIncomeService.AddNewVariableIncome<Stock>(stockSell2);

            IReadOnlyCollection<Profit> profits = await Repository.ListAsync<Profit>();

            decimal profitFromDb = profits.Sum(x => x.Value);
            decimal profitValue = (stockSell1.Total + stockSell2.Total) - (stockBuy.Price * (stockSell1.Amount + stockSell2.Amount));
            #endregion

            #region Assert
            Assert.That(addedBuy && addedSell1 && addedSell2, Is.True);
            Assert.That(profitFromDb, Is.Zero);
            Assert.That(profitFromDb, Is.EqualTo(profitValue));
            #endregion
        }

        #endregion

        [Test]
        public async Task StockProfit_ProfitBuyThanABunchSellsInDiferentMonths_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            Stock stockBuy = TestsSettings.GetStock(financialInstitution);
            stockBuy.Amount = 1000;
            stockBuy.Total = stockBuy.Amount * stockBuy.Price;
            bool addedBuy = await VariableIncomeService.AddNewVariableIncome<Stock>(stockBuy);

            Stock stockSell1 = TestsSettings.GetStock(financialInstitution);
            stockSell1 = TestsSettings.GetVariableIncomeToSell(stockSell1, 73, 16.5m);

            Stock stockSell2 = TestsSettings.GetStock(financialInstitution);
            stockSell2 = TestsSettings.GetVariableIncomeToSell(stockSell2, 82, 13.11m);
            stockSell2.OperationDate = stockSell1.OperationDate.AddMonths(1);

            Stock stockSell3 = TestsSettings.GetStock(financialInstitution);
            stockSell3 = TestsSettings.GetVariableIncomeToSell(stockSell3, 1, 8m);
            stockSell3.OperationDate = stockSell2.OperationDate.AddMonths(1);
            #endregion

            #region Act
            bool addedSell1 = await VariableIncomeService.AddNewVariableIncome<Stock>(stockSell1);
            bool addedSell2 = await VariableIncomeService.AddNewVariableIncome<Stock>(stockSell2);
            bool addedSell3 = await VariableIncomeService.AddNewVariableIncome<Stock>(stockSell3);

            var profits = await Repository.ListAsync<Profit>();
            decimal profitValueFromDb = profits.Sum(x => x.Value);

            decimal profitValue = stockSell1.Total - (stockSell1.Amount * stockBuy.Price);
            profitValue += stockSell2.Total - (stockSell2.Amount * stockBuy.Price);
            profitValue += stockSell3.Total - (stockSell3.Amount * stockBuy.Price);
            #endregion

            #region Assert
            Assert.That(addedBuy, Is.True);
            Assert.That(addedSell1 && addedSell2 && addedSell3, Is.True);
            Assert.That(profitValue, Is.EqualTo(profitValueFromDb));
            #endregion
        }

        [Test]
        public async Task StockProfit_ProfitBuyThanSellWithoutTaxExempt_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            Stock stockBuy = TestsSettings.GetStock(financialInstitution);
            stockBuy.Amount = 10000;
            stockBuy.Total = stockBuy.Amount * stockBuy.Price;
            bool addedBuy = await VariableIncomeService.AddNewVariableIncome<Stock>(stockBuy);

            Stock stockSell = TestsSettings.GetStock(financialInstitution);
            stockSell = TestsSettings.GetVariableIncomeToSell(stockSell, 2300, 15.5m);
            #endregion

            #region Act
            bool addedSell1 = await VariableIncomeService.AddNewVariableIncome<Stock>(stockSell);

            var profits = await Repository.ListAsync<Profit>();
            decimal profitValueFromDb = profits.Sum(x => x.Value);

            decimal profitValue = stockSell.Total - (stockSell.Amount * stockBuy.Price);
            #endregion

            #region Assert
            Assert.That(addedBuy, Is.True);
            Assert.That(addedSell1, Is.True);
            Assert.That(profitValue, Is.EqualTo(profitValueFromDb));
            #endregion
        }

        [Test]
        public async Task StockProfit_ProfitBuyThanSellFirstWithThanWithoutTaxExempt_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            Stock stockBuy = TestsSettings.GetStock(financialInstitution);
            stockBuy.Amount = 10000;
            stockBuy.Total = stockBuy.Amount * stockBuy.Price;
            bool addedBuy = await VariableIncomeService.AddNewVariableIncome<Stock>(stockBuy);

            Stock stockSell1 = TestsSettings.GetStock(financialInstitution);
            stockSell1 = TestsSettings.GetVariableIncomeToSell(stockSell1, 100, 16.5m);

            Stock stockSell2 = TestsSettings.GetStock(financialInstitution);
            stockSell2 = TestsSettings.GetVariableIncomeToSell(stockSell2, 2000, 11m);
            #endregion

            #region Act
            bool addedSell1 = await VariableIncomeService.AddNewVariableIncome<Stock>(stockSell1);
            bool addedSell2 = await VariableIncomeService.AddNewVariableIncome<Stock>(stockSell2);

            var profits = await Repository.ListAsync<Profit>();
            decimal profitValueFromDb = profits.Sum(x => x.Value);

            decimal profitValue = stockSell1.Total - (stockSell1.Amount * stockBuy.Price);
            profitValue += stockSell2.Total - (stockSell2.Amount * stockBuy.Price);
            #endregion

            #region Assert
            Assert.That(addedBuy, Is.True);
            Assert.That(addedSell1 && addedSell2, Is.True);
            Assert.That(profitValue, Is.EqualTo(profitValueFromDb));

            foreach (var profit in profits)
                Assert.That(profit.TaxExempt, Is.False);
            #endregion
        }

        [Test]
        public async Task StockProfit_ProfitBuyThanSellWithLossThanWithNotTaxExemptProfit_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            Stock stockBuy = TestsSettings.GetStock(financialInstitution);
            stockBuy.Amount = 10000;
            stockBuy.Total = stockBuy.Amount * stockBuy.Price;
            bool addedBuy = await VariableIncomeService.AddNewVariableIncome<Stock>(stockBuy);

            Stock stockSell1 = TestsSettings.GetStock(financialInstitution);
            stockSell1 = TestsSettings.GetVariableIncomeToSell(stockSell1, 1000, 9);

            Stock stockSell2 = TestsSettings.GetStock(financialInstitution);
            stockSell2 = TestsSettings.GetVariableIncomeToSell(stockSell2, 100, 11m);
            #endregion

            #region Act
            bool addedSell1 = await VariableIncomeService.AddNewVariableIncome<Stock>(stockSell1);
            bool addedSell2 = await VariableIncomeService.AddNewVariableIncome<Stock>(stockSell2);

            var profits = await Repository.ListAsync<Profit>();
            decimal profitValueFromDb = profits.Sum(x => x.Value);

            decimal profitValue = stockSell1.Total - (stockSell1.Amount * stockBuy.Price);
            profitValue += stockSell2.Total - (stockSell2.Amount * stockBuy.Price);
            #endregion

            #region Assert
            Assert.That(addedBuy, Is.True);
            Assert.That(addedSell1 && addedSell2, Is.True);
            Assert.That(profitValue, Is.EqualTo(profitValueFromDb));
            #endregion
        }

        #region Testes específicos para declaração de IR sobre lucros em operações

        [Test]
        //Teste para calculo de declaração de IR destinado ao campo de lucros isentos nas vendas mensais
        //inferiores a R$20.0000,00
        public async Task StockProfit_GanhosLiquidosEmOperacoesAte20kM_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            Stock stockBuy = TestsSettings.GetStock(financialInstitution);
            stockBuy.Amount = 10000;
            stockBuy.Total = stockBuy.Amount * stockBuy.Price;
            bool addedBuy = await VariableIncomeService.AddNewVariableIncome<Stock>(stockBuy);

            Stock stockSell1 = TestsSettings.GetStock(financialInstitution);
            stockSell1 = TestsSettings.GetVariableIncomeToSell(stockSell1, 10, 20m);

            Stock stockSell2 = TestsSettings.GetStock(financialInstitution);
            stockSell2 = TestsSettings.GetVariableIncomeToSell(stockSell2, 400, 60m);

            Stock stockSell3 = TestsSettings.GetStock(financialInstitution);
            stockSell3 = TestsSettings.GetVariableIncomeToSell(stockSell3, 500, 50m);

            Stock stockSell4 = TestsSettings.GetStock(financialInstitution);
            stockSell4 = TestsSettings.GetVariableIncomeToSell(stockSell4, 600, 55m);
            #endregion

            #region Act
            bool addedSell1 = await VariableIncomeService.AddNewVariableIncome<Stock>(stockSell1);
            bool addedSell2 = await VariableIncomeService.AddNewVariableIncome<Stock>(stockSell2);
            bool addedSell3 = await VariableIncomeService.AddNewVariableIncome<Stock>(stockSell3);
            bool addedSell4 = await VariableIncomeService.AddNewVariableIncome<Stock>(stockSell4);

            var profits = await Repository.ListAsync<Profit>();
            decimal profitValueFromDb = profits.Sum(x => x.Value);

            decimal profitValue = stockSell1.Total - (stockSell1.Amount * stockBuy.Price);
            profitValue += stockSell2.Total - (stockSell2.Amount * stockBuy.Price);
            profitValue += stockSell3.Total - (stockSell3.Amount * stockBuy.Price);
            profitValue += stockSell4.Total - (stockSell4.Amount * stockBuy.Price);
            #endregion

            #region Assert
            Assert.That(addedBuy, Is.True);
            Assert.That(addedSell1 && addedSell2 && addedSell3 && addedSell4, Is.True);
            Assert.That(profitValue, Is.EqualTo(profitValueFromDb));
            #endregion
        }

        [Test]
        //Teste para calculo de declaração de IR destinado ao campo de lucros nas vendas mensais
        public async Task StockProfit_GanhosEmOperacoesMisturandoMesAcimaMesAbaixoDe20kVendasMensais_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            Stock stockBuy = TestsSettings.GetStock(financialInstitution);
            stockBuy.Amount = 10000;
            stockBuy.Total = stockBuy.Amount * stockBuy.Price;
            bool addedBuy = await VariableIncomeService.AddNewVariableIncome<Stock>(stockBuy);

            Stock stockSell1 = TestsSettings.GetStock(financialInstitution);
            stockSell1 = TestsSettings.GetVariableIncomeToSell(stockSell1, 10, 20m);
            stockSell1.OperationDate = stockBuy.OperationDate.AddDays(1);

            Stock stockSell2 = TestsSettings.GetStock(financialInstitution);
            stockSell2 = TestsSettings.GetVariableIncomeToSell(stockSell2, 400, 60m);
            stockSell2.OperationDate = stockSell1.OperationDate.AddMonths(1);

            Stock stockSell3 = TestsSettings.GetStock(financialInstitution);
            stockSell3 = TestsSettings.GetVariableIncomeToSell(stockSell3, 10, 50m);
            stockSell3.OperationDate = stockSell2.OperationDate.AddMonths(1);

            Stock stockSell4 = TestsSettings.GetStock(financialInstitution);
            stockSell4 = TestsSettings.GetVariableIncomeToSell(stockSell4, 600, 55m);
            stockSell4.OperationDate = stockSell3.OperationDate.AddMonths(1);

            #endregion

            #region Act
            bool addedSell1 = await VariableIncomeService.AddNewVariableIncome<Stock>(stockSell1);
            bool addedSell2 = await VariableIncomeService.AddNewVariableIncome<Stock>(stockSell2);
            bool addedSell3 = await VariableIncomeService.AddNewVariableIncome<Stock>(stockSell3);
            bool addedSell4 = await VariableIncomeService.AddNewVariableIncome<Stock>(stockSell4);

            var profitsListTaxExempt = await Repository.ListAsync<Profit>(x => x.TaxExempt);
            decimal profitTaxExemptFromDb = profitsListTaxExempt.Sum(x => x.Value);

            var profitsListNotTaxExempt = await Repository.ListAsync<Profit>(x => !x.TaxExempt);
            decimal profitNotTaxExemptFromDb = profitsListNotTaxExempt.Sum(x => x.Value);

            decimal profitTaxExempt = stockSell1.Total - (stockSell1.Amount * stockBuy.Price);
            profitTaxExempt += stockSell3.Total - (stockSell3.Amount * stockBuy.Price);
            
            decimal profitNotTaxExempt = stockSell2.Total - (stockSell2.Amount * stockBuy.Price);
            profitNotTaxExempt += stockSell4.Total - (stockSell4.Amount * stockBuy.Price);
            #endregion

            #region Assert
            Assert.That(addedBuy, Is.True);
            Assert.That(addedSell1 && addedSell2 && addedSell3 && addedSell4, Is.True);
            Assert.That(profitTaxExempt, Is.EqualTo(profitTaxExemptFromDb));
            Assert.That(profitNotTaxExempt, Is.EqualTo(profitNotTaxExemptFromDb));
            #endregion
        }

        [Test]
        //Teste para calculo de declaração de IR destinado ao campo de lucros tributáveis nas vendas mensais
        //superiores a R$20.0000,00
        public async Task StockProfit_GanhosBrutosEmOperacoesAcima20kM_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            Stock stockBuy = TestsSettings.GetStock(financialInstitution);
            stockBuy.Amount = 10000;
            stockBuy.Total = stockBuy.Amount * stockBuy.Price;
            bool addedBuy = await VariableIncomeService.AddNewVariableIncome<Stock>(stockBuy);

            Stock stockSell1 = TestsSettings.GetStock(financialInstitution);
            stockSell1 = TestsSettings.GetVariableIncomeToSell(stockSell1, 10, 20m);

            Stock stockSell2 = TestsSettings.GetStock(financialInstitution);
            stockSell2 = TestsSettings.GetVariableIncomeToSell(stockSell2, 400, 60m);

            Stock stockSell3 = TestsSettings.GetStock(financialInstitution);
            stockSell3 = TestsSettings.GetVariableIncomeToSell(stockSell3, 500, 50m);

            Stock stockSell4 = TestsSettings.GetStock(financialInstitution);
            stockSell4 = TestsSettings.GetVariableIncomeToSell(stockSell4, 600, 55m);
            #endregion

            #region Act
            bool addedSell1 = await VariableIncomeService.AddNewVariableIncome<Stock>(stockSell1);
            bool addedSell2 = await VariableIncomeService.AddNewVariableIncome<Stock>(stockSell2);
            bool addedSell3 = await VariableIncomeService.AddNewVariableIncome<Stock>(stockSell3);
            bool addedSell4 = await VariableIncomeService.AddNewVariableIncome<Stock>(stockSell4);

            var profits = await Repository.ListAsync<Profit>();
            decimal profitValueFromDb = profits.Sum(x => x.Value);

            decimal profitValue = stockSell1.Total - (stockSell1.Amount * stockBuy.Price);
            profitValue += stockSell2.Total - (stockSell2.Amount * stockBuy.Price);
            profitValue += stockSell3.Total - (stockSell3.Amount * stockBuy.Price);
            profitValue += stockSell4.Total - (stockSell4.Amount * stockBuy.Price);
            #endregion

            #region Assert
            Assert.That(addedBuy, Is.True);
            Assert.That(addedSell1 && addedSell2 && addedSell3 && addedSell4, Is.True);
            Assert.That(profitValue, Is.EqualTo(profitValueFromDb));
            #endregion
        }

        #endregion
    }
}
