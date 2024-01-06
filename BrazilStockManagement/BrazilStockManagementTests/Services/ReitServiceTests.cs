using BrazilStockManagementTests.ConfigTests.ConfigServices;
using JBMDatabase.Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrazilStockManagementTests.Services
{
    public class ReitServiceTests
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
            ReitBusiness ReitBusiness = new ReitBusiness(Repository);
            PortfolioBusiness PortfolioBusiness = new PortfolioBusiness(Repository);
            VariableIncomeService = new VariableIncomeServiceTests(null, ReitBusiness, null, PortfolioBusiness);
        }

        #region Buy First / Sell Last

        [TestCase(20, 5)]
        [TestCase(20, 10)]
        [TestCase(5, 5)]
        [TestCase(5, 10)]
        //Regular profit and Loss
        public async Task ReitProfit_BuyThenSellProfitAndLoss_Success(decimal sellPrice, int amountSell)
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);

            Reit reitBuy = TestsSettings.GetReit(financialInstitution);
            bool addedBuy = await VariableIncomeService.AddNewVariableIncome<Reit>(reitBuy);

            Reit reitSell = TestsSettings.GetReit(financialInstitution);
            reitSell.OperationType = OperationType.Sell;
            reitSell.Price = sellPrice;
            reitSell.Amount = amountSell;
            reitSell.Total = sellPrice * amountSell;
            #endregion

            #region Act
            bool addedSell = await VariableIncomeService.AddNewVariableIncome<Reit>(reitSell);

            IReadOnlyCollection<Profit> profits = await Repository.ListAsync<Profit>();

            decimal profitFromDb = profits.Sum(x => x.Value);
            decimal profitValue = reitSell.Total - (reitBuy.Price * amountSell);
            #endregion

            #region Assert
            Assert.That(addedBuy && addedSell, Is.True);
            Assert.That(profitFromDb, Is.EqualTo(profitValue));
            #endregion
        }

        [TestCase(5)]
        [TestCase(10)]
        //Buy reit and sell at the same price
        public async Task ReitProfit_BuyThenSellAtTheSamePrice_Success(int amountSell)
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);

            Reit reitBuy = TestsSettings.GetReit(financialInstitution);
            bool addedBuy = await VariableIncomeService.AddNewVariableIncome<Reit>(reitBuy);

            Reit reitSell = TestsSettings.GetReit(financialInstitution);
            reitSell.Total = reitSell.Price * amountSell;
            reitSell.OperationType = OperationType.Sell;
            #endregion

            #region Act
            bool addedSell = await VariableIncomeService.AddNewVariableIncome<Reit>(reitSell);

            IReadOnlyCollection<Profit> profits = await Repository.ListAsync<Profit>();

            decimal profitFromDb = profits.Sum(x => x.Value);
            decimal profitValue = reitSell.Total - (reitBuy.Price * amountSell);
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
        public async Task ReitProfit_BuyThenSellReversingTheProfitToLossAndViceVersa_Success(decimal priceSell1, decimal priceSell2,
                                                                                              int amountSell1, int amountSell2)
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);

            Reit reitBuy = TestsSettings.GetReit(financialInstitution);
            bool addedBuy = await VariableIncomeService.AddNewVariableIncome<Reit>(reitBuy);

            //1st sell
            Reit reitSell1 = TestsSettings.GetReit(financialInstitution);
            reitSell1.Price = reitBuy.Price + priceSell1;
            reitSell1.Amount = amountSell1;
            reitSell1.Total = reitSell1.Price * amountSell1;
            reitSell1.OperationType = OperationType.Sell;

            //2nd sell
            Reit reitSell2 = TestsSettings.GetReit(financialInstitution);
            reitSell2.Price = reitBuy.Price + priceSell2;
            reitSell2.Amount = amountSell2;
            reitSell2.Total = reitSell2.Price * amountSell2;
            reitSell2.OperationType = OperationType.Sell;
            #endregion

            #region Act
            bool addedSell1 = await VariableIncomeService.AddNewVariableIncome<Reit>(reitSell1);
            bool addedSell2 = await VariableIncomeService.AddNewVariableIncome<Reit>(reitSell2);

            IReadOnlyCollection<Profit> profits = await Repository.ListAsync<Profit>();

            decimal profitFromDb = profits.Sum(x => x.Value);
            decimal profitValue = (reitSell1.Total + reitSell2.Total) - (reitBuy.Price * (amountSell1 + amountSell2));
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
        public async Task ReitProfit_BuyThenSellAtTheEndZeroProfitZeroLoss_Success(decimal priceSell, int amountSell)
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);

            Reit reitBuy = TestsSettings.GetReit(financialInstitution);
            bool addedBuy = await VariableIncomeService.AddNewVariableIncome<Reit>(reitBuy);

            //1st sell with Loss
            Reit reitSell1 = TestsSettings.GetReit(financialInstitution);
            reitSell1.Price = reitBuy.Price + priceSell;
            reitSell1.Amount = amountSell;
            reitSell1.Total = reitSell1.Price * amountSell;
            reitSell1.OperationType = OperationType.Sell;

            //2nd sell with profit biiggers than loss
            Reit reitSell2 = TestsSettings.GetReit(financialInstitution);
            reitSell2.Price = reitBuy.Price - priceSell;
            reitSell2.Amount = amountSell;
            reitSell2.Total = reitSell2.Price * amountSell;
            reitSell2.OperationType = OperationType.Sell;
            #endregion

            #region Act
            bool addedSell1 = await VariableIncomeService.AddNewVariableIncome<Reit>(reitSell1);
            bool addedSell2 = await VariableIncomeService.AddNewVariableIncome<Reit>(reitSell2);

            IReadOnlyCollection<Profit> profits = await Repository.ListAsync<Profit>();

            decimal profitFromDb = profits.Sum(x => x.Value);
            decimal profitValue = (reitSell1.Total + reitSell2.Total) - (reitBuy.Price * (reitSell1.Amount + reitSell2.Amount));
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
        public async Task ReitProfit_SellThenBuyProfitAndLoss_Success(decimal buyPrice, int amountBuy)
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);

            Reit reitSell = TestsSettings.GetReit(financialInstitution);
            reitSell.OperationType = OperationType.Sell;
            bool addedSell = await VariableIncomeService.AddNewVariableIncome<Reit>(reitSell);

            Reit reitBuy = TestsSettings.GetReit(financialInstitution);
            reitBuy.Price = buyPrice;
            reitBuy.Amount = amountBuy;
            reitBuy.Total = buyPrice * amountBuy;
            #endregion

            #region Act
            bool addedBuy = await VariableIncomeService.AddNewVariableIncome<Reit>(reitBuy);

            IReadOnlyCollection<Profit> profits = await Repository.ListAsync<Profit>();

            decimal profitFromDb = profits.Sum(x => x.Value);
            decimal profitValue = (reitSell.Price * amountBuy) - (reitBuy.Price * amountBuy);
            #endregion

            #region Assert
            Assert.That(addedSell && addedBuy, Is.True);
            Assert.That(profitFromDb, Is.EqualTo(profitValue));
            #endregion
        }

        [TestCase(5)]
        [TestCase(10)]
        //Sell reit and buy at the same price
        public async Task ReitProfit_SellThenBuyAtTheSamePrice_Success(int amountBuy)
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);

            Reit reitSell = TestsSettings.GetReit(financialInstitution);
            reitSell.OperationType = OperationType.Sell;
            bool addedSell = await VariableIncomeService.AddNewVariableIncome<Reit>(reitSell);

            Reit reitBuy = TestsSettings.GetReit(financialInstitution);
            reitBuy.Total = reitBuy.Price * amountBuy;
            #endregion

            #region Act
            bool addedBuy = await VariableIncomeService.AddNewVariableIncome<Reit>(reitBuy);

            IReadOnlyCollection<Profit> profits = await Repository.ListAsync<Profit>();

            decimal profitFromDb = profits.Sum(x => x.Value);
            decimal profitValue = (reitSell.Price * amountBuy) - (reitBuy.Price * amountBuy);
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
        public async Task ReitProfit_SellThenBuyReversingTheProfitToLossAndViceVersa_Success(decimal priceBuy1, decimal priceBuy2,
                                                                                              int amountBuy1, int amountBuy2)
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);

            Reit reitSell = TestsSettings.GetReit(financialInstitution);
            reitSell.OperationType = OperationType.Sell;
            bool addedSell = await VariableIncomeService.AddNewVariableIncome<Reit>(reitSell);

            //1st buy
            Reit reitBuy1 = TestsSettings.GetReit(financialInstitution);
            reitBuy1.Price = reitSell.Price + priceBuy1;
            reitBuy1.Amount = amountBuy1;
            reitBuy1.Total = reitBuy1.Price * amountBuy1;

            //2nd buy
            Reit reitBuy2 = TestsSettings.GetReit(financialInstitution);
            reitBuy2.Price = reitSell.Price + priceBuy2;
            reitBuy2.Amount = amountBuy2;
            reitBuy2.Total = reitBuy2.Price * amountBuy2;
            #endregion

            #region Act
            bool addedBuy1 = await VariableIncomeService.AddNewVariableIncome<Reit>(reitBuy1);
            bool addedBuy2 = await VariableIncomeService.AddNewVariableIncome<Reit>(reitBuy2);

            IReadOnlyCollection<Profit> profits = await Repository.ListAsync<Profit>();

            decimal profitFromDb = profits.Sum(x => x.Value);
            decimal profitValue = (reitBuy1.Total + reitBuy2.Total) - (reitSell.Price * (amountBuy1 + amountBuy2));
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
        public async Task ReitProfit_SellThenBuyAtTheEndZeroProfitZeroLoss_Success(decimal priceSell, int amountSell)
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);

            Reit reitBuy = TestsSettings.GetReit(financialInstitution);
            bool addedBuy = await VariableIncomeService.AddNewVariableIncome<Reit>(reitBuy);

            //1st sell with Loss
            Reit reitSell1 = TestsSettings.GetReit(financialInstitution);
            reitSell1.Price = reitBuy.Price + priceSell;
            reitSell1.Amount = amountSell;
            reitSell1.Total = reitSell1.Price * amountSell;
            reitSell1.OperationType = OperationType.Sell;

            //2nd sell with profit biiggers than loss
            Reit reitSell2 = TestsSettings.GetReit(financialInstitution);
            reitSell2.Price = reitBuy.Price - priceSell;
            reitSell2.Amount = amountSell;
            reitSell2.Total = reitSell2.Price * amountSell;
            reitSell2.OperationType = OperationType.Sell;
            #endregion

            #region Act
            bool addedSell1 = await VariableIncomeService.AddNewVariableIncome<Reit>(reitSell1);
            bool addedSell2 = await VariableIncomeService.AddNewVariableIncome<Reit>(reitSell2);

            IReadOnlyCollection<Profit> profits = await Repository.ListAsync<Profit>();

            decimal profitFromDb = profits.Sum(x => x.Value);
            decimal profitValue = (reitSell1.Total + reitSell2.Total) - (reitBuy.Price * (reitSell1.Amount + reitSell2.Amount));
            #endregion

            #region Assert
            Assert.That(addedBuy && addedSell1 && addedSell2, Is.True);
            Assert.That(profitFromDb, Is.Zero);
            Assert.That(profitFromDb, Is.EqualTo(profitValue));
            #endregion
        }

        #endregion

        [Test]
        public async Task ReitProfit_ProfitBuyThanABunchSellsInDiferentMonths_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            Reit reitBuy = TestsSettings.GetReit(financialInstitution);
            reitBuy.Amount = 1000;
            reitBuy.Total = reitBuy.Amount * reitBuy.Price;
            bool addedBuy = await VariableIncomeService.AddNewVariableIncome<Reit>(reitBuy);

            Reit reitSell1 = TestsSettings.GetReit(financialInstitution);
            reitSell1 = TestsSettings.GetVariableIncomeToSell(reitSell1, 73, 16.5m);

            Reit reitSell2 = TestsSettings.GetReit(financialInstitution);
            reitSell2 = TestsSettings.GetVariableIncomeToSell(reitSell2, 82, 13.11m);
            reitSell2.OperationDate = reitSell1.OperationDate.AddMonths(1);

            Reit reitSell3 = TestsSettings.GetReit(financialInstitution);
            reitSell3 = TestsSettings.GetVariableIncomeToSell(reitSell3, 1, 8m);
            reitSell3.OperationDate = reitSell2.OperationDate.AddMonths(1);
            #endregion

            #region Act
            bool addedSell1 = await VariableIncomeService.AddNewVariableIncome<Reit>(reitSell1);
            bool addedSell2 = await VariableIncomeService.AddNewVariableIncome<Reit>(reitSell2);
            bool addedSell3 = await VariableIncomeService.AddNewVariableIncome<Reit>(reitSell3);

            var profits = await Repository.ListAsync<Profit>();
            decimal profitValueFromDb = profits.Sum(x => x.Value);

            decimal profitValue = reitSell1.Total - (reitSell1.Amount * reitBuy.Price);
            profitValue += reitSell2.Total - (reitSell2.Amount * reitBuy.Price);
            profitValue += reitSell3.Total - (reitSell3.Amount * reitBuy.Price);
            #endregion

            #region Assert
            Assert.That(addedBuy, Is.True);
            Assert.That(addedSell1 && addedSell2 && addedSell3, Is.True);
            Assert.That(profitValue, Is.EqualTo(profitValueFromDb));
            #endregion
        }

        [Test]
        public async Task ReitProfit_ProfitBuyThanSellWithoutTaxExempt_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            Reit reitBuy = TestsSettings.GetReit(financialInstitution);
            reitBuy.Amount = 10000;
            reitBuy.Total = reitBuy.Amount * reitBuy.Price;
            bool addedBuy = await VariableIncomeService.AddNewVariableIncome<Reit>(reitBuy);

            Reit reitSell = TestsSettings.GetReit(financialInstitution);
            reitSell = TestsSettings.GetVariableIncomeToSell(reitSell, 2300, 15.5m);
            #endregion

            #region Act
            bool addedSell1 = await VariableIncomeService.AddNewVariableIncome<Reit>(reitSell);

            var profits = await Repository.ListAsync<Profit>();
            decimal profitValueFromDb = profits.Sum(x => x.Value);

            decimal profitValue = reitSell.Total - (reitSell.Amount * reitBuy.Price);
            #endregion

            #region Assert
            Assert.That(addedBuy, Is.True);
            Assert.That(addedSell1, Is.True);
            Assert.That(profitValue, Is.EqualTo(profitValueFromDb));
            #endregion
        }

        [Test]
        public async Task ReitProfit_ProfitBuyThanSellFirstWithThanWithoutTaxExempt_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            Reit reitBuy = TestsSettings.GetReit(financialInstitution);
            reitBuy.Amount = 10000;
            reitBuy.Total = reitBuy.Amount * reitBuy.Price;
            bool addedBuy = await VariableIncomeService.AddNewVariableIncome<Reit>(reitBuy);

            Reit reitSell1 = TestsSettings.GetReit(financialInstitution);
            reitSell1 = TestsSettings.GetVariableIncomeToSell(reitSell1, 100, 16.5m);

            Reit reitSell2 = TestsSettings.GetReit(financialInstitution);
            reitSell2 = TestsSettings.GetVariableIncomeToSell(reitSell2, 2000, 11m);
            #endregion

            #region Act
            bool addedSell1 = await VariableIncomeService.AddNewVariableIncome<Reit>(reitSell1);
            bool addedSell2 = await VariableIncomeService.AddNewVariableIncome<Reit>(reitSell2);

            var profits = await Repository.ListAsync<Profit>();
            decimal profitValueFromDb = profits.Sum(x => x.Value);

            decimal profitValue = reitSell1.Total - (reitSell1.Amount * reitBuy.Price);
            profitValue += reitSell2.Total - (reitSell2.Amount * reitBuy.Price);
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
        public async Task ReitProfit_ProfitBuyThanSellWithLossThanWithNotTaxExemptProfit_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            Reit reitBuy = TestsSettings.GetReit(financialInstitution);
            reitBuy.Amount = 10000;
            reitBuy.Total = reitBuy.Amount * reitBuy.Price;
            bool addedBuy = await VariableIncomeService.AddNewVariableIncome<Reit>(reitBuy);

            Reit reitSell1 = TestsSettings.GetReit(financialInstitution);
            reitSell1 = TestsSettings.GetVariableIncomeToSell(reitSell1, 1000, 9);

            Reit reitSell2 = TestsSettings.GetReit(financialInstitution);
            reitSell2 = TestsSettings.GetVariableIncomeToSell(reitSell2, 100, 11m);
            #endregion

            #region Act
            bool addedSell1 = await VariableIncomeService.AddNewVariableIncome<Reit>(reitSell1);
            bool addedSell2 = await VariableIncomeService.AddNewVariableIncome<Reit>(reitSell2);

            var profits = await Repository.ListAsync<Profit>();
            decimal profitValueFromDb = profits.Sum(x => x.Value);

            decimal profitValue = reitSell1.Total - (reitSell1.Amount * reitBuy.Price);
            profitValue += reitSell2.Total - (reitSell2.Amount * reitBuy.Price);
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
        public async Task ReitProfit_GanhosLiquidosEmOperacoesAte20kM_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            Reit reitBuy = TestsSettings.GetReit(financialInstitution);
            reitBuy.Amount = 10000;
            reitBuy.Total = reitBuy.Amount * reitBuy.Price;
            bool addedBuy = await VariableIncomeService.AddNewVariableIncome<Reit>(reitBuy);

            Reit reitSell1 = TestsSettings.GetReit(financialInstitution);
            reitSell1 = TestsSettings.GetVariableIncomeToSell(reitSell1, 10, 20m);

            Reit reitSell2 = TestsSettings.GetReit(financialInstitution);
            reitSell2 = TestsSettings.GetVariableIncomeToSell(reitSell2, 400, 60m);

            Reit reitSell3 = TestsSettings.GetReit(financialInstitution);
            reitSell3 = TestsSettings.GetVariableIncomeToSell(reitSell3, 500, 50m);

            Reit reitSell4 = TestsSettings.GetReit(financialInstitution);
            reitSell4 = TestsSettings.GetVariableIncomeToSell(reitSell4, 600, 55m);
            #endregion

            #region Act
            bool addedSell1 = await VariableIncomeService.AddNewVariableIncome<Reit>(reitSell1);
            bool addedSell2 = await VariableIncomeService.AddNewVariableIncome<Reit>(reitSell2);
            bool addedSell3 = await VariableIncomeService.AddNewVariableIncome<Reit>(reitSell3);
            bool addedSell4 = await VariableIncomeService.AddNewVariableIncome<Reit>(reitSell4);

            var profits = await Repository.ListAsync<Profit>();
            decimal profitValueFromDb = profits.Sum(x => x.Value);

            decimal profitValue = reitSell1.Total - (reitSell1.Amount * reitBuy.Price);
            profitValue += reitSell2.Total - (reitSell2.Amount * reitBuy.Price);
            profitValue += reitSell3.Total - (reitSell3.Amount * reitBuy.Price);
            profitValue += reitSell4.Total - (reitSell4.Amount * reitBuy.Price);
            #endregion

            #region Assert
            Assert.That(addedBuy, Is.True);
            Assert.That(addedSell1 && addedSell2 && addedSell3 && addedSell4, Is.True);
            Assert.That(profitValue, Is.EqualTo(profitValueFromDb));
            #endregion
        }

        [Test]
        //Teste para calculo de declaração de IR destinado ao campo de lucros nas vendas mensais
        public async Task ReitProfit_GanhosEmOperacoesMisturandoMesAcimaMesAbaixoDe20kVendasMensais_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            Reit reitBuy = TestsSettings.GetReit(financialInstitution);
            reitBuy.Amount = 10000;
            reitBuy.Total = reitBuy.Amount * reitBuy.Price;
            bool addedBuy = await VariableIncomeService.AddNewVariableIncome<Reit>(reitBuy);

            Reit reitSell1 = TestsSettings.GetReit(financialInstitution);
            reitSell1 = TestsSettings.GetVariableIncomeToSell(reitSell1, 10, 20m);

            Reit reitSell2 = TestsSettings.GetReit(financialInstitution);
            reitSell2 = TestsSettings.GetVariableIncomeToSell(reitSell2, 400, 60m);
            reitSell2.OperationDate = reitSell1.OperationDate.AddMonths(1);

            Reit reitSell3 = TestsSettings.GetReit(financialInstitution);
            reitSell3 = TestsSettings.GetVariableIncomeToSell(reitSell3, 10, 50m);
            reitSell3.OperationDate = reitSell2.OperationDate.AddMonths(1);

            Reit reitSell4 = TestsSettings.GetReit(financialInstitution);
            reitSell4 = TestsSettings.GetVariableIncomeToSell(reitSell4, 600, 55m);
            reitSell4.OperationDate = reitSell3.OperationDate.AddMonths(1);

            #endregion

            #region Act
            bool addedSell1 = await VariableIncomeService.AddNewVariableIncome<Reit>(reitSell1);
            bool addedSell2 = await VariableIncomeService.AddNewVariableIncome<Reit>(reitSell2);
            bool addedSell3 = await VariableIncomeService.AddNewVariableIncome<Reit>(reitSell3);
            bool addedSell4 = await VariableIncomeService.AddNewVariableIncome<Reit>(reitSell4);

            var profitsListTaxExempt = await Repository.ListAsync<Profit>(x => x.TaxExempt);
            decimal profitTaxExemptFromDb = profitsListTaxExempt.Sum(x => x.Value);

            var profitsListNotTaxExempt = await Repository.ListAsync<Profit>(x => !x.TaxExempt);
            decimal profitNotTaxExemptFromDb = profitsListNotTaxExempt.Sum(x => x.Value);

            decimal profitNotTaxExempt = reitSell1.Total - (reitSell1.Amount * reitBuy.Price);
            profitNotTaxExempt += reitSell2.Total - (reitSell2.Amount * reitBuy.Price);
            profitNotTaxExempt += reitSell3.Total - (reitSell3.Amount * reitBuy.Price);
            profitNotTaxExempt += reitSell4.Total - (reitSell4.Amount * reitBuy.Price);
            #endregion

            #region Assert
            Assert.That(addedBuy, Is.True);
            Assert.That(addedSell1 && addedSell2 && addedSell3 && addedSell4, Is.True);
            Assert.That(profitTaxExemptFromDb, Is.EqualTo(0));
            Assert.That(profitNotTaxExempt, Is.EqualTo(profitNotTaxExemptFromDb));
            #endregion
        }

        [Test]
        //Teste para calculo de declaração de IR destinado ao campo de lucros tributáveis nas vendas mensais
        //superiores a R$20.0000,00
        public async Task ReitProfit_GanhosBrutosEmOperacoesAcima20kM_Success()
        {
            #region Arrange
            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            Reit reitBuy = TestsSettings.GetReit(financialInstitution);
            reitBuy.Amount = 10000;
            reitBuy.Total = reitBuy.Amount * reitBuy.Price;
            bool addedBuy = await VariableIncomeService.AddNewVariableIncome<Reit>(reitBuy);

            Reit reitSell1 = TestsSettings.GetReit(financialInstitution);
            reitSell1 = TestsSettings.GetVariableIncomeToSell(reitSell1, 10, 20m);

            Reit reitSell2 = TestsSettings.GetReit(financialInstitution);
            reitSell2 = TestsSettings.GetVariableIncomeToSell(reitSell2, 400, 60m);

            Reit reitSell3 = TestsSettings.GetReit(financialInstitution);
            reitSell3 = TestsSettings.GetVariableIncomeToSell(reitSell3, 500, 50m);

            Reit reitSell4 = TestsSettings.GetReit(financialInstitution);
            reitSell4 = TestsSettings.GetVariableIncomeToSell(reitSell4, 600, 55m);
            #endregion

            #region Act
            bool addedSell1 = await VariableIncomeService.AddNewVariableIncome<Reit>(reitSell1);
            bool addedSell2 = await VariableIncomeService.AddNewVariableIncome<Reit>(reitSell2);
            bool addedSell3 = await VariableIncomeService.AddNewVariableIncome<Reit>(reitSell3);
            bool addedSell4 = await VariableIncomeService.AddNewVariableIncome<Reit>(reitSell4);

            var profits = await Repository.ListAsync<Profit>();
            decimal profitValueFromDb = profits.Sum(x => x.Value);

            decimal profitValue = reitSell1.Total - (reitSell1.Amount * reitBuy.Price);
            profitValue += reitSell2.Total - (reitSell2.Amount * reitBuy.Price);
            profitValue += reitSell3.Total - (reitSell3.Amount * reitBuy.Price);
            profitValue += reitSell4.Total - (reitSell4.Amount * reitBuy.Price);
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
