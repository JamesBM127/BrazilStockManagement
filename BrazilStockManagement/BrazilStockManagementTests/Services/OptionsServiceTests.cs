using BrazilStockManagementTests.ConfigTests.ConfigServices;
using JBMDatabase.Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrazilStockManagementTests.Services
{
    public class OptionsServiceTests
    {
        public StockContext Context { get; set; }
        public IStockManagerRepository Repository;
        public VariableIncomeServiceTests VariableIncomeService { get; set; }
        //public OptionsService OptionsService { get; set; }
        public OptionsServiceConfig OptionsService { get; set; }
        private bool DeleteTestDb = true;
        public ProfitBusiness ProfitBusiness { get; set; }
        
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
            OptionsBusiness OptionsBusiness = new OptionsBusiness(Repository);
            ProfitBusiness = new ProfitBusiness(Repository);
            PortfolioBusiness PortfolioBusiness = new PortfolioBusiness(Repository);
            FinancialInstitutionBusiness FinancialInstitution = new FinancialInstitutionBusiness(Repository);

            VariableIncomeService = new VariableIncomeServiceTests(portfolioBusiness: PortfolioBusiness, optionsBusiness: OptionsBusiness);
            OptionsService = new OptionsServiceConfig(OptionsBusiness, PortfolioBusiness, FinancialInstitution, ProfitBusiness);

            
        }

        #region Buy First / Sell Last

        [TestCase(20, 5)]
        [TestCase(20, 10)]
        [TestCase(5, 5)]
        [TestCase(5, 10)]
        //Regular profit and Loss
        public async Task OptionsProfit_BuyThenSellProfitAndLoss_Success(decimal sellPrice, int amountSell)
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);

            Options optionsBuy = TestsSettings.GetOptions(financialInstitution);
            bool addedBuy = await VariableIncomeService.AddNewVariableIncome<Options>(optionsBuy);

            Options optionsSell = TestsSettings.GetOptions(financialInstitution);
            optionsSell.OperationType = OperationType.Sell;
            optionsSell.Price = sellPrice;
            optionsSell.Amount = amountSell;
            optionsSell.Total = sellPrice * amountSell;
            #endregion

            #region Act
            bool addedSell = await VariableIncomeService.AddNewVariableIncome<Options>(optionsSell);

            IReadOnlyCollection<Profit> profits = await Repository.ListAsync<Profit>();

            decimal profitFromDb = profits.Sum(x => x.Value);
            decimal profitValue = optionsSell.Total - (optionsBuy.Price * amountSell);
            #endregion

            #region Assert
            Assert.That(addedBuy && addedSell, Is.True);
            Assert.That(profitFromDb, Is.EqualTo(profitValue));
            #endregion
        }

        [TestCase(5)]
        [TestCase(10)]
        //Buy options and sell at the same price
        public async Task OptionsProfit_BuyThenSellAtTheSamePrice_Success(int amountSell)
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);

            Options optionsBuy = TestsSettings.GetOptions(financialInstitution);
            bool addedBuy = await VariableIncomeService.AddNewVariableIncome<Options>(optionsBuy);

            Options optionsSell = TestsSettings.GetOptions(financialInstitution);
            optionsSell.Total = optionsSell.Price * amountSell;
            optionsSell.OperationType = OperationType.Sell;
            #endregion

            #region Act
            bool addedSell = await VariableIncomeService.AddNewVariableIncome<Options>(optionsSell);

            IReadOnlyCollection<Profit> profits = await Repository.ListAsync<Profit>();

            decimal profitFromDb = profits.Sum(x => x.Value);
            decimal profitValue = optionsSell.Total - (optionsBuy.Price * amountSell);
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
        public async Task OptionsProfit_BuyThenSellReversingTheProfitToLossAndViceVersa_Success(decimal priceSell1, decimal priceSell2,
                                                                                              int amountSell1, int amountSell2)
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);

            Options optionsBuy = TestsSettings.GetOptions(financialInstitution);
            bool addedBuy = await VariableIncomeService.AddNewVariableIncome<Options>(optionsBuy);

            //1st sell
            Options optionsSell1 = TestsSettings.GetOptions(financialInstitution);
            optionsSell1.Price = optionsBuy.Price + priceSell1;
            optionsSell1.Amount = amountSell1;
            optionsSell1.Total = optionsSell1.Price * amountSell1;
            optionsSell1.OperationType = OperationType.Sell;

            //2nd sell
            Options optionsSell2 = TestsSettings.GetOptions(financialInstitution);
            optionsSell2.Price = optionsBuy.Price + priceSell2;
            optionsSell2.Amount = amountSell2;
            optionsSell2.Total = optionsSell2.Price * amountSell2;
            optionsSell2.OperationType = OperationType.Sell;
            #endregion

            #region Act
            bool addedSell1 = await VariableIncomeService.AddNewVariableIncome<Options>(optionsSell1);
            bool addedSell2 = await VariableIncomeService.AddNewVariableIncome<Options>(optionsSell2);

            IReadOnlyCollection<Profit> profits = await Repository.ListAsync<Profit>();

            decimal profitFromDb = profits.Sum(x => x.Value);
            decimal profitValue = (optionsSell1.Total + optionsSell2.Total) - (optionsBuy.Price * (amountSell1 + amountSell2));
            #endregion

            #region Assert
            Assert.That(addedBuy && addedSell1 && addedSell2, Is.True);
            Assert.That(profitFromDb, Is.EqualTo(profitValue));
            #endregion
        }

        [Test]
        //Buy then sell, get profit then loss, and vice versa, to finish with not gain neither loss
        public async Task OptionsProfit_BuyThenSellAtTheEndZeroProfitZeroLoss_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);

            Options optionsBuy = TestsSettings.GetOptions(financialInstitution);
            bool addedBuy = await VariableIncomeService.AddNewVariableIncome<Options>(optionsBuy);

            //1st sell with Loss
            Options optionsSell1 = TestsSettings.GetOptions(financialInstitution, amount: 500, price: 0.1m);
            optionsSell1.OperationType = OperationType.Sell;

            //2nd sell with profit biiggers than loss
            Options optionsSell2 = TestsSettings.GetOptions(financialInstitution, amount: 500, price: 0.9m);
            optionsSell2.OperationType = OperationType.Sell;
            #endregion

            #region Act
            bool addedSell1 = await VariableIncomeService.AddNewVariableIncome<Options>(optionsSell1);
            bool addedSell2 = await VariableIncomeService.AddNewVariableIncome<Options>(optionsSell2);

            IReadOnlyCollection<Profit> profits = await Repository.ListAsync<Profit>();

            decimal profitFromDb = profits.Sum(x => x.Value);
            decimal profitValue = (optionsSell1.Total + optionsSell2.Total) - (optionsBuy.Price * (optionsSell1.Amount + optionsSell2.Amount));
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
        public async Task OptionsProfit_SellThenBuyProfitAndLoss_Success(decimal buyPrice, int amountBuy)
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);

            Options optionsSell = TestsSettings.GetOptions(financialInstitution);
            optionsSell.OperationType = OperationType.Sell;
            bool addedSell = await VariableIncomeService.AddNewVariableIncome<Options>(optionsSell);

            Options optionsBuy = TestsSettings.GetOptions(financialInstitution);
            optionsBuy.Price = buyPrice;
            optionsBuy.Amount = amountBuy;
            optionsBuy.Total = buyPrice * amountBuy;
            #endregion

            #region Act
            bool addedBuy = await VariableIncomeService.AddNewVariableIncome<Options>(optionsBuy);

            IReadOnlyCollection<Profit> profits = await Repository.ListAsync<Profit>();

            decimal profitFromDb = profits.Sum(x => x.Value);
            decimal profitValue = (optionsSell.Price * amountBuy) - (optionsBuy.Price * amountBuy);
            #endregion

            #region Assert
            Assert.That(addedSell && addedBuy, Is.True);
            Assert.That(profitFromDb, Is.EqualTo(profitValue));
            #endregion
        }

        [TestCase(5)]
        [TestCase(10)]
        //Sell options and buy at the same price
        public async Task OptionsProfit_SellThenBuyAtTheSamePrice_Success(int amountBuy)
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);

            Options optionsSell = TestsSettings.GetOptions(financialInstitution);
            optionsSell.OperationType = OperationType.Sell;
            bool addedSell = await VariableIncomeService.AddNewVariableIncome<Options>(optionsSell);

            Options optionsBuy = TestsSettings.GetOptions(financialInstitution);
            optionsBuy.Total = optionsBuy.Price * amountBuy;
            #endregion

            #region Act
            bool addedBuy = await VariableIncomeService.AddNewVariableIncome<Options>(optionsBuy);

            IReadOnlyCollection<Profit> profits = await Repository.ListAsync<Profit>();

            decimal profitFromDb = profits.Sum(x => x.Value);
            decimal profitValue = (optionsSell.Price * amountBuy) - (optionsBuy.Price * amountBuy);
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
        public async Task OptionsProfit_SellThenBuyReversingTheProfitToLossAndViceVersa_Success(decimal priceBuy1, decimal priceBuy2,
                                                                                              int amountBuy1, int amountBuy2)
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);

            Options optionsSell = TestsSettings.GetOptions(financialInstitution);
            optionsSell.OperationType = OperationType.Sell;
            bool addedSell = await VariableIncomeService.AddNewVariableIncome<Options>(optionsSell);

            //1st buy
            Options optionsBuy1 = TestsSettings.GetOptions(financialInstitution);
            optionsBuy1.Price = optionsSell.Price + priceBuy1;
            optionsBuy1.Amount = amountBuy1;
            optionsBuy1.Total = optionsBuy1.Price * amountBuy1;

            //2nd buy
            Options optionsBuy2 = TestsSettings.GetOptions(financialInstitution);
            optionsBuy2.Price = optionsSell.Price + priceBuy2;
            optionsBuy2.Amount = amountBuy2;
            optionsBuy2.Total = optionsBuy2.Price * amountBuy2;
            #endregion

            #region Act
            bool addedBuy1 = await VariableIncomeService.AddNewVariableIncome<Options>(optionsBuy1);
            bool addedBuy2 = await VariableIncomeService.AddNewVariableIncome<Options>(optionsBuy2);

            IReadOnlyCollection<Profit> profits = await Repository.ListAsync<Profit>();

            decimal profitFromDb = profits.Sum(x => x.Value);
            decimal profitValue = (optionsBuy1.Total + optionsBuy2.Total) - (optionsSell.Price * (amountBuy1 + amountBuy2));
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
        public async Task OptionsProfit_SellThenBuyAtTheEndZeroProfitZeroLoss_Success(decimal priceSell, int amountSell)
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);

            Options optionsBuy = TestsSettings.GetOptions(financialInstitution);
            bool addedBuy = await VariableIncomeService.AddNewVariableIncome<Options>(optionsBuy);

            //1st sell with Loss
            Options optionsSell1 = TestsSettings.GetOptions(financialInstitution);
            optionsSell1.Price = optionsBuy.Price + priceSell;
            optionsSell1.Amount = amountSell;
            optionsSell1.Total = optionsSell1.Price * amountSell;
            optionsSell1.OperationType = OperationType.Sell;

            //2nd sell with profit biiggers than loss
            Options optionsSell2 = TestsSettings.GetOptions(financialInstitution);
            optionsSell2.Price = optionsBuy.Price - priceSell;
            optionsSell2.Amount = amountSell;
            optionsSell2.Total = optionsSell2.Price * amountSell;
            optionsSell2.OperationType = OperationType.Sell;
            #endregion

            #region Act
            bool addedSell1 = await VariableIncomeService.AddNewVariableIncome<Options>(optionsSell1);
            bool addedSell2 = await VariableIncomeService.AddNewVariableIncome<Options>(optionsSell2);

            IReadOnlyCollection<Profit> profits = await Repository.ListAsync<Profit>();

            decimal profitFromDb = profits.Sum(x => x.Value);
            decimal profitValue = (optionsSell1.Total + optionsSell2.Total) - (optionsBuy.Price * (optionsSell1.Amount + optionsSell2.Amount));
            #endregion

            #region Assert
            Assert.That(addedBuy && addedSell1 && addedSell2, Is.True);
            Assert.That(profitFromDb, Is.Zero);
            Assert.That(profitFromDb, Is.EqualTo(profitValue));
            #endregion
        }

        #endregion

        [Test]
        public async Task OptionsProfit_ProfitBuyThanABunchSellsInDiferentMonths_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            Options optionsBuy = TestsSettings.GetOptions(financialInstitution);
            optionsBuy.Amount = 1000;
            optionsBuy.Total = optionsBuy.Amount * optionsBuy.Price;
            bool addedBuy = await VariableIncomeService.AddNewVariableIncome<Options>(optionsBuy);

            Options optionsSell1 = TestsSettings.GetOptions(financialInstitution);
            optionsSell1 = TestsSettings.GetVariableIncomeToSell(optionsSell1, 73, 16.5m);

            Options optionsSell2 = TestsSettings.GetOptions(financialInstitution);
            optionsSell2 = TestsSettings.GetVariableIncomeToSell(optionsSell2, 82, 13.11m);
            optionsSell2.OperationDate = optionsSell1.OperationDate.AddMonths(1);

            Options optionsSell3 = TestsSettings.GetOptions(financialInstitution);
            optionsSell3 = TestsSettings.GetVariableIncomeToSell(optionsSell3, 1, 8m);
            optionsSell3.OperationDate = optionsSell2.OperationDate.AddMonths(1);
            #endregion

            #region Act
            bool addedSell1 = await VariableIncomeService.AddNewVariableIncome<Options>(optionsSell1);
            bool addedSell2 = await VariableIncomeService.AddNewVariableIncome<Options>(optionsSell2);
            bool addedSell3 = await VariableIncomeService.AddNewVariableIncome<Options>(optionsSell3);

            var profits = await Repository.ListAsync<Profit>();
            decimal profitValueFromDb = profits.Sum(x => x.Value);

            decimal profitValue = optionsSell1.Total - (optionsSell1.Amount * optionsBuy.Price);
            profitValue += optionsSell2.Total - (optionsSell2.Amount * optionsBuy.Price);
            profitValue += optionsSell3.Total - (optionsSell3.Amount * optionsBuy.Price);
            #endregion

            #region Assert
            Assert.That(addedBuy, Is.True);
            Assert.That(addedSell1 && addedSell2 && addedSell3, Is.True);
            Assert.That(profitValue, Is.EqualTo(profitValueFromDb));
            #endregion
        }

        [Test]
        public async Task OptionsProfit_ProfitBuyThanSellWithoutTaxExempt_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            Options optionsBuy = TestsSettings.GetOptions(financialInstitution);
            optionsBuy.Amount = 10000;
            optionsBuy.Total = optionsBuy.Amount * optionsBuy.Price;
            bool addedBuy = await VariableIncomeService.AddNewVariableIncome<Options>(optionsBuy);

            Options optionsSell = TestsSettings.GetOptions(financialInstitution);
            optionsSell = TestsSettings.GetVariableIncomeToSell(optionsSell, 2300, 15.5m);
            #endregion

            #region Act
            bool addedSell1 = await VariableIncomeService.AddNewVariableIncome<Options>(optionsSell);

            var profits = await Repository.ListAsync<Profit>();
            decimal profitValueFromDb = profits.Sum(x => x.Value);

            decimal profitValue = optionsSell.Total - (optionsSell.Amount * optionsBuy.Price);
            #endregion

            #region Assert
            Assert.That(addedBuy, Is.True);
            Assert.That(addedSell1, Is.True);
            Assert.That(profitValue, Is.EqualTo(profitValueFromDb));
            #endregion
        }

        [Test]
        public async Task OptionsProfit_ProfitBuyThanSellFirstWithThanWithoutTaxExempt_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            Options optionsBuy = TestsSettings.GetOptions(financialInstitution);
            optionsBuy.Amount = 10000;
            optionsBuy.Total = optionsBuy.Amount * optionsBuy.Price;
            bool addedBuy = await VariableIncomeService.AddNewVariableIncome<Options>(optionsBuy);

            Options optionsSell1 = TestsSettings.GetOptions(financialInstitution);
            optionsSell1 = TestsSettings.GetVariableIncomeToSell(optionsSell1, 100, 16.5m);

            Options optionsSell2 = TestsSettings.GetOptions(financialInstitution);
            optionsSell2 = TestsSettings.GetVariableIncomeToSell(optionsSell2, 2000, 11m);
            #endregion

            #region Act
            bool addedSell1 = await VariableIncomeService.AddNewVariableIncome<Options>(optionsSell1);
            bool addedSell2 = await VariableIncomeService.AddNewVariableIncome<Options>(optionsSell2);

            var profits = await Repository.ListAsync<Profit>();
            decimal profitValueFromDb = profits.Sum(x => x.Value);

            decimal profitValue = optionsSell1.Total - (optionsSell1.Amount * optionsBuy.Price);
            profitValue += optionsSell2.Total - (optionsSell2.Amount * optionsBuy.Price);
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
        public async Task OptionsProfit_ProfitBuyThanSellWithLossThanWithNotTaxExemptProfit_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            Options optionsBuy = TestsSettings.GetOptions(financialInstitution);
            optionsBuy.Amount = 10000;
            optionsBuy.Total = optionsBuy.Amount * optionsBuy.Price;
            bool addedBuy = await VariableIncomeService.AddNewVariableIncome<Options>(optionsBuy);

            Options optionsSell1 = TestsSettings.GetOptions(financialInstitution);
            optionsSell1 = TestsSettings.GetVariableIncomeToSell(optionsSell1, 1000, 9);

            Options optionsSell2 = TestsSettings.GetOptions(financialInstitution);
            optionsSell2 = TestsSettings.GetVariableIncomeToSell(optionsSell2, 100, 11m);
            #endregion

            #region Act
            bool addedSell1 = await VariableIncomeService.AddNewVariableIncome<Options>(optionsSell1);
            bool addedSell2 = await VariableIncomeService.AddNewVariableIncome<Options>(optionsSell2);

            var profits = await Repository.ListAsync<Profit>();
            decimal profitValueFromDb = profits.Sum(x => x.Value);

            decimal profitValue = optionsSell1.Total - (optionsSell1.Amount * optionsBuy.Price);
            profitValue += optionsSell2.Total - (optionsSell2.Amount * optionsBuy.Price);
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
        //inferiores a R20.0000,00
        public async Task OptionsProfit_GanhosLiquidosEmOperacoesAte20kM_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            Options optionsBuy = TestsSettings.GetOptions(financialInstitution);
            optionsBuy.Amount = 10000;
            optionsBuy.Total = optionsBuy.Amount * optionsBuy.Price;
            bool addedBuy = await VariableIncomeService.AddNewVariableIncome<Options>(optionsBuy);

            Options optionsSell1 = TestsSettings.GetOptions(financialInstitution);
            optionsSell1 = TestsSettings.GetVariableIncomeToSell(optionsSell1, 10, 20m);

            Options optionsSell2 = TestsSettings.GetOptions(financialInstitution);
            optionsSell2 = TestsSettings.GetVariableIncomeToSell(optionsSell2, 400, 60m);

            Options optionsSell3 = TestsSettings.GetOptions(financialInstitution);
            optionsSell3 = TestsSettings.GetVariableIncomeToSell(optionsSell3, 500, 50m);

            Options optionsSell4 = TestsSettings.GetOptions(financialInstitution);
            optionsSell4 = TestsSettings.GetVariableIncomeToSell(optionsSell4, 600, 55m);
            #endregion

            #region Act
            bool addedSell1 = await VariableIncomeService.AddNewVariableIncome<Options>(optionsSell1);
            bool addedSell2 = await VariableIncomeService.AddNewVariableIncome<Options>(optionsSell2);
            bool addedSell3 = await VariableIncomeService.AddNewVariableIncome<Options>(optionsSell3);
            bool addedSell4 = await VariableIncomeService.AddNewVariableIncome<Options>(optionsSell4);

            var profits = await Repository.ListAsync<Profit>();
            decimal profitValueFromDb = profits.Sum(x => x.Value);

            decimal profitValue = optionsSell1.Total - (optionsSell1.Amount * optionsBuy.Price);
            profitValue += optionsSell2.Total - (optionsSell2.Amount * optionsBuy.Price);
            profitValue += optionsSell3.Total - (optionsSell3.Amount * optionsBuy.Price);
            profitValue += optionsSell4.Total - (optionsSell4.Amount * optionsBuy.Price);
            #endregion

            #region Assert
            Assert.That(addedBuy, Is.True);
            Assert.That(addedSell1 && addedSell2 && addedSell3 && addedSell4, Is.True);
            Assert.That(profitValue, Is.EqualTo(profitValueFromDb));
            #endregion
        }

        [Test]
        //Teste para calculo de declaração de IR destinado ao campo de lucros nas vendas mensais
        public async Task OptionsProfit_GanhosEmOperacoesMisturandoMesAcimaMesAbaixoDe20kVendasMensais_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            Options optionsBuy = TestsSettings.GetOptions(financialInstitution);
            optionsBuy.Amount = 10000;
            optionsBuy.Total = optionsBuy.Amount * optionsBuy.Price;
            bool addedBuy = await VariableIncomeService.AddNewVariableIncome<Options>(optionsBuy);

            Options optionsSell1 = TestsSettings.GetOptions(financialInstitution);
            optionsSell1 = TestsSettings.GetVariableIncomeToSell(optionsSell1, 10, 20m);

            Options optionsSell2 = TestsSettings.GetOptions(financialInstitution);
            optionsSell2 = TestsSettings.GetVariableIncomeToSell(optionsSell2, 400, 60m);
            optionsSell2.OperationDate = optionsSell1.OperationDate.AddMonths(1);

            Options optionsSell3 = TestsSettings.GetOptions(financialInstitution);
            optionsSell3 = TestsSettings.GetVariableIncomeToSell(optionsSell3, 10, 50m);
            optionsSell3.OperationDate = optionsSell2.OperationDate.AddMonths(1);

            Options optionsSell4 = TestsSettings.GetOptions(financialInstitution);
            optionsSell4 = TestsSettings.GetVariableIncomeToSell(optionsSell4, 600, 55m);
            optionsSell4.OperationDate = optionsSell3.OperationDate.AddMonths(1);

            #endregion

            #region Act
            bool addedSell1 = await VariableIncomeService.AddNewVariableIncome<Options>(optionsSell1);
            bool addedSell2 = await VariableIncomeService.AddNewVariableIncome<Options>(optionsSell2);
            bool addedSell3 = await VariableIncomeService.AddNewVariableIncome<Options>(optionsSell3);
            bool addedSell4 = await VariableIncomeService.AddNewVariableIncome<Options>(optionsSell4);

            var profitsListTaxExempt = await Repository.ListAsync<Profit>(x => x.TaxExempt);
            decimal profitTaxExemptFromDb = profitsListTaxExempt.Sum(x => x.Value);

            var profitsListNotTaxExempt = await Repository.ListAsync<Profit>(x => !x.TaxExempt);
            decimal profitNotTaxExemptFromDb = profitsListNotTaxExempt.Sum(x => x.Value);

            decimal profitTaxExempt = optionsSell1.Total - (optionsSell1.Amount * optionsBuy.Price);
            profitTaxExempt += optionsSell3.Total - (optionsSell3.Amount * optionsBuy.Price);

            decimal profitNotTaxExempt = optionsSell2.Total - (optionsSell2.Amount * optionsBuy.Price);
            profitNotTaxExempt += optionsSell4.Total - (optionsSell4.Amount * optionsBuy.Price);
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
        //superiores a R20.0000,00
        public async Task OptionsProfit_GanhosBrutosEmOperacoesAcima20kM_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            Options optionsBuy = TestsSettings.GetOptions(financialInstitution);
            optionsBuy.Amount = 10000;
            optionsBuy.Total = optionsBuy.Amount * optionsBuy.Price;
            bool addedBuy = await VariableIncomeService.AddNewVariableIncome<Options>(optionsBuy);

            Options optionsSell1 = TestsSettings.GetOptions(financialInstitution);
            optionsSell1 = TestsSettings.GetVariableIncomeToSell(optionsSell1, 10, 20m);

            Options optionsSell2 = TestsSettings.GetOptions(financialInstitution);
            optionsSell2 = TestsSettings.GetVariableIncomeToSell(optionsSell2, 400, 60m);

            Options optionsSell3 = TestsSettings.GetOptions(financialInstitution);
            optionsSell3 = TestsSettings.GetVariableIncomeToSell(optionsSell3, 500, 50m);

            Options optionsSell4 = TestsSettings.GetOptions(financialInstitution);
            optionsSell4 = TestsSettings.GetVariableIncomeToSell(optionsSell4, 600, 55m);
            #endregion

            #region Act
            bool addedSell1 = await VariableIncomeService.AddNewVariableIncome<Options>(optionsSell1);
            bool addedSell2 = await VariableIncomeService.AddNewVariableIncome<Options>(optionsSell2);
            bool addedSell3 = await VariableIncomeService.AddNewVariableIncome<Options>(optionsSell3);
            bool addedSell4 = await VariableIncomeService.AddNewVariableIncome<Options>(optionsSell4);

            var profits = await Repository.ListAsync<Profit>();
            decimal profitValueFromDb = profits.Sum(x => x.Value);

            decimal profitValue = optionsSell1.Total - (optionsSell1.Amount * optionsBuy.Price);
            profitValue += optionsSell2.Total - (optionsSell2.Amount * optionsBuy.Price);
            profitValue += optionsSell3.Total - (optionsSell3.Amount * optionsBuy.Price);
            profitValue += optionsSell4.Total - (optionsSell4.Amount * optionsBuy.Price);
            #endregion

            #region Assert
            Assert.That(addedBuy, Is.True);
            Assert.That(addedSell1 && addedSell2 && addedSell3 && addedSell4, Is.True);
            Assert.That(profitValue, Is.EqualTo(profitValueFromDb));
            #endregion
        }

        #endregion

        #region Exercise of Options
        [TestCase(OperationType.Buy, DerivativeType.Call)]
        [TestCase(OperationType.Sell, DerivativeType.Call)]
        public async Task OptionsExercise_CallExercise_Success(OperationType OperationType, DerivativeType DerivativeType)
        {
            #region Arrange
            DeleteTestDb = true;
            Guid holderId = await OptionsServiceConfig.AssertOptions(OperationType, DerivativeType, VariableIncomeService, Repository);
            
            Dictionary<string, decimal> StockDictionary = new Dictionary<string, decimal>()
            {
                {"PETR4", 26.82m},
                {"VALE3", 104.98m},
                {"ABCD3", 14m},
                {"ITUB4", 28.35m},
                {"BBDC4", 22.20m}
            };
            #endregion

            #region Act
            List<Options> options = await OptionsService.OptionsExerciseAsync(holderId, DateTime.Now.Month, StockDictionary);
            #endregion

            #region Assert
            Assert.That(options.Count(), Is.EqualTo(2));
            #endregion
        }

        [TestCase(OperationType.Buy, DerivativeType.Put)]
        [TestCase(OperationType.Sell, DerivativeType.Put)]
        public async Task OptionsExercise_PutExercise_Success(OperationType OperationType, DerivativeType DerivativeType)
        {
            #region Arrange
            DeleteTestDb = true;
            Guid holderId = await OptionsServiceConfig.AssertOptions(OperationType, DerivativeType, VariableIncomeService, Repository);
            
            Dictionary<string, decimal> StockDictionary = new Dictionary<string, decimal>()
            {
                {"PETR4", 26.82m},
                {"VALE3", 104.98m},
                {"ABCD3", 14m},
                {"ITUB4", 28.35m},
                {"BBDC4", 22.20m}
            };
            #endregion

            #region Act
            List<Options> options = await OptionsService.OptionsExerciseAsync(holderId, DateTime.Now.Month, StockDictionary);
            #endregion

            #region Assert
            Assert.That(options.Count(), Is.EqualTo(1));
            #endregion
        }

        #endregion

        #region Options became powder

        [TestCase(OperationType.Buy, DerivativeType.Call)]
        [TestCase(OperationType.Sell, DerivativeType.Call)]
        public async Task OptionsExercise_CallBuyPowder_Success(OperationType OperationType, DerivativeType DerivativeType)
        {
            #region Arrange
            Guid holderId = await OptionsServiceConfig.AssertOptions(OperationType, DerivativeType, VariableIncomeService, Repository);
            DeleteTestDb = true;

            Dictionary<string, decimal> StockDictionary = new Dictionary<string, decimal>()
            {
                {"PETR4", 26.82m},
                {"VALE3", 104.98m},
                {"ABCD3", 14m},
                {"ITUB4", 28.35m},
                {"BBDC4", 22.20m}
            };
            #endregion

            #region Act
            await OptionsService.OptionsExerciseAsync(holderId, DateTime.Now.Month, StockDictionary);
            IReadOnlyCollection<Profit> profitList = await ProfitBusiness.ListAsync<Profit>();
            #endregion

            #region Assert
            Assert.That(profitList.Count(), Is.EqualTo(1));
            #endregion
        }

        [TestCase(OperationType.Buy, DerivativeType.Put)]
        [TestCase(OperationType.Sell, DerivativeType.Put)]
        public async Task OptionsExercise_PutBuyPowder_Success(OperationType OperationType, DerivativeType DerivativeType)
        {
            #region Arrange
            Guid holderId = await OptionsServiceConfig.AssertOptions(OperationType, DerivativeType, VariableIncomeService, Repository);
            DeleteTestDb = true;

            Dictionary<string, decimal> StockDictionary = new Dictionary<string, decimal>()
            {
                {"PETR4", 26.82m},
                {"VALE3", 104.98m},
                {"ABCD3", 14m},
                {"ITUB4", 28.35m},
                {"BBDC4", 22.20m}
            };
            #endregion

            #region Act
            await OptionsService.OptionsExerciseAsync(holderId, DateTime.Now.Month, StockDictionary);
            IReadOnlyCollection<Profit> profitList = await ProfitBusiness.ListAsync<Profit>();
            #endregion

            #region Assert
            Assert.That(profitList.Count(), Is.EqualTo(1));
            #endregion
        }

        #endregion
    }
}
