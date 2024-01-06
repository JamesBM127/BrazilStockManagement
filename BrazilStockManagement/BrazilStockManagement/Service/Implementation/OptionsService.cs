using BrazilStockManagement.Business.Implementation;
using BrazilStockManagement.Entity;
using BrazilStockManagement.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace BrazilStockManagement.Service.Implementation
{
    public class OptionsService
    {
        private readonly OptionsBusiness OptionsBusiness;
        private readonly PortfolioBusiness PortfolioBusiness;
        private readonly FinancialInstitutionBusiness FinancialInstitutionBusiness;
        private readonly ProfitBusiness ProfitBusiness;
        private IReadOnlyCollection<Portfolio> OptionsInPortfolio;

        public OptionsService(OptionsBusiness optionsBusiness,
                              PortfolioBusiness portfolioBusiness,
                              FinancialInstitutionBusiness financialInstitutionBusiness,
                              ProfitBusiness profitBusiness)
        {
            OptionsBusiness = optionsBusiness;
            PortfolioBusiness = portfolioBusiness;
            FinancialInstitutionBusiness = financialInstitutionBusiness;
            ProfitBusiness = profitBusiness;
        }

        public async Task<Options> GetAsync(string code)
        {
            Options option = await OptionsBusiness.GetAsync<Options>(x => x.Code == code);
            return option;
        }

        public async Task<IReadOnlyCollection<Options>> ListAsync(Expression<Func<Options, bool>>? expression = null, Func<IQueryable<Options>, IIncludableQueryable<Options, object>> include = null)
        {
            IReadOnlyCollection<Options> optionList = await OptionsBusiness.ListAsync(expression, include);
            return optionList;
        }

        public async Task<bool> UpdateAsync(Options options)
        {
            bool updated = OptionsBusiness.Update<Options>(options);
            bool saved = false;
            if (updated)
                saved = await OptionsBusiness.CommitAsync(updated);

            return saved;
        }

        public async Task<List<Options>> UpdateAsync(IEnumerable<Options> options)
        {
            OptionsBusiness.Update<Options>(options);
            List<Options> notSavedEntities = await OptionsBusiness.CommitAsync<Options>(true);

            return notSavedEntities;
        }

        public async Task<List<Options>> ListHolderOptionsAsync(Guid shareHolderId)
        {
            List<Options> options = new();
            IReadOnlyCollection<FinancialInstitution> financialInstitutions = await FinancialInstitutionBusiness.ListAsync<FinancialInstitution>(x => x.ShareHolderId == shareHolderId);
            OptionsInPortfolio = await PortfolioBusiness.ListAsync<Portfolio>(x => x.ShareHolderId == shareHolderId && x.InvestmentType == InvestmentType.Derivative);

            foreach (FinancialInstitution financialInstitution in financialInstitutions)
            {
                foreach (Portfolio portfolio in OptionsInPortfolio)
                {
                    IReadOnlyCollection<Options> optionsFromDb = await ListAsync(x =>
                                                                                 x.FinancialInstitutionId == financialInstitution.Id &&
                                                                                 x.Code == portfolio.Code &&
                                                                                 x.ExerciseDate >= DateTime.Today,
                                                                                 y => y.Include(f => f.FinancialInstitution));

                    options.AddRange(optionsFromDb);
                }
            }

            return options;
        }

        public async Task<List<Options>> OptionsExerciseAsync(Guid shareHolderId, int exerciseMonth, Dictionary<string, decimal> stockPricesByCode)
        {
            List<Options> options = await ListHolderOptionsAsync(shareHolderId);
            options = options.Where(x => x.ExerciseDate.Month == exerciseMonth).ToList();

            List<Options> exercisedOptions = new();

            decimal stockPrice = 0;

            foreach (Options option in options)
            {
                if (stockPricesByCode.TryGetValue(option.StockCode, out stockPrice))
                {
                    bool exercised = false;

                    if (option.DerivativeType == DerivativeType.Call)
                        exercised = await Call(stockPrice, option);

                    else
                        exercised = await Put(stockPrice, option);

                    if (exercised)
                        exercisedOptions.Add(option);

                    await PortfolioBusiness.DeleteAsync<Portfolio>(x => x.Code == option.Code && x.ShareHolderId == shareHolderId);
                }
            }

            await ProfitBusiness.CommitAsync(true);
            return exercisedOptions;
        }

        private async Task<bool> Call(decimal stockPrice, Options option, decimal otherCosts = 0)
        {
            bool exercised = false;

            //Exercida
            if (stockPrice >= option.Strike)
                exercised = await ExerciseOption(option, otherCosts);

            //Pó
            else
                await BecamePowder(option);

            return exercised;
        }

        private async Task<bool> Put(decimal stockPrice, Options option, decimal otherCosts = 0)
        {
            bool exercised = false;
            //Pó
            if (stockPrice > option.Strike)
                await BecamePowder(option);

            //Exercida
            else
                exercised = await ExerciseOption(option, otherCosts);

            return exercised;
        }

        private async Task BecamePowder(Options option)
        {
            decimal value = 0;
            decimal taxValue = 0;

            //Prejuizo
            if (option.OperationType == OperationType.Buy)
            {
                value -= option.Total;
            }
            //Lucro
            if (option.OperationType == OperationType.Sell)
            {
                value += option.Total;
                taxValue = 0.15m;
            }

            Profit profit = new Profit()
            {
                Value = value,
                TaxExempt = false,
                TaxValue = taxValue,
                ProfitType = ProfitType.OptionDelisting,
                TransactionType = TransactionType.Short,
                Code = option.Code,
                OperationDate = option.OperationDate,
                OperationType = option.OperationType,
                InvestmentType = option.InvestmentType,
                ShareHolderId = option.FinancialInstitution.ShareHolderId
            };

            await ProfitBusiness.AddAsync(profit);
        }

        private async Task<bool> ExerciseOption(Options option, decimal otherCosts = 0)
        {
            //int amount = OptionsInPortfolio.Where(x => x.Code == option.Code).Select(x => x.Quantity).First();
            int amount = Math.Abs(OptionsInPortfolio.Where(x => x.Code == option.Code).Select(x => x.Quantity).First());
            decimal price = 0;
            OperationType operationType = 0;

            //Call config
            if (option.DerivativeType == DerivativeType.Call)
            {
                if (option.OperationType == OperationType.Sell)
                {
                    operationType = OperationType.Sell;
                    otherCosts = -otherCosts;
                }
                else
                    operationType = OperationType.Buy;

                price = option.Strike + option.Price;
            }
            //Put config
            else
            {
                if (option.OperationType == OperationType.Buy)
                { 
                    otherCosts = -otherCosts;
                    operationType = OperationType.Sell;
                }
                 
                else
                    operationType = OperationType.Buy;

                price = option.Strike - option.Price;
            }

            Stock stock = new Stock()
            {
                Company = option.Company,
                Code = option.StockCode,

                Amount = amount,
                Price = price,
                OtherCosts = Math.Abs(otherCosts),
                Total = (amount * price) + otherCosts,

                OperationDate = DateTime.Today,
                OperationType = operationType,
                InvestmentType = InvestmentType.Stock,

                FinancialInstitutionId = option.FinancialInstitutionId
            };

            bool added = await PortfolioBusiness.BuyOrSellPortfolioAsync(stock);
            return await PortfolioBusiness.CommitAsync(added);
        }
    }
}
