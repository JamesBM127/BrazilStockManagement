using BrazilStockManagement.Entity;
using BrazilStockManagement.Enums;
using BrazilStockManagement.Service.Implementation;
using BrazilStockManagement.StockRepository.Interface;
using JBMDatabase.Business;

namespace BrazilStockManagement.Business.Implementation
{
    public class PortfolioBusiness : BaseBusiness
    {
        public PortfolioBusiness(IStockManagerRepository repository) : base(repository)
        {
        }

        public async Task<bool> BuyOrSellPortfolioAsync<TEntity>(TEntity entity, Portfolio portfolio = null) where TEntity : VariableIncome
        {
            //Add entity in database
            if (!await AddAsync(entity))
                throw new ApplicationException($"{entity.Code} operation are not added to database");

            //Get FinancialInstitution to get the ShareHolderId in  PortfolioUpdate
            if (entity.FinancialInstitution is null)
                entity.FinancialInstitution = await GetAsync<FinancialInstitution>(x => x.Id == entity.FinancialInstitutionId);

            //Get portfolio to modifie
            if (portfolio == null)
                portfolio = await GetAsync<Portfolio>(x => x.Code == entity.Code && x.ShareHolderId == entity.FinancialInstitution.ShareHolderId);

            bool portfolioExists = portfolio != null;

            if (portfolioExists)
                await ConfigAndSaveProfit(entity, portfolio);

            portfolio = portfolio.UpdatePortfolio(entity);

            if (portfolioExists)
            {
                try
                {
                    if (portfolio.Quantity == 0)
                        return Delete<Portfolio>(portfolio);
                    else
                        return Update(portfolio);
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            else
                return await AddAsync(portfolio);
        }

        private async Task ConfigAndSaveProfit<TEntity>(TEntity entity, Portfolio portfolio) where TEntity : VariableIncome
        {
            Profit profit = ProfitService.GetOperationProfit(ref entity, ref portfolio);

            if (profit != null)
            {
                if (entity.OperationType == OperationType.Delisting)
                    profit.ProfitType = ProfitType.OptionDelisting;

                else
                    profit.ProfitType = await GetProfitTypeAsync<TEntity>(profit);

                profit.TaxExempt = false;

                if (profit.Value > 0)
                {
                    switch (profit.InvestmentType)
                    {
                        case InvestmentType.Stock:

                            if (profit.ProfitType == ProfitType.DayTrade)
                                profit.TaxValue = 0.2m;

                            else
                            {
                                profit.TaxExempt = await IsTaxExemptAsync(profit.OperationDate, (entity.Amount * entity.Price));

                                if (!profit.TaxExempt)
                                    profit.TaxValue = 0.15m;
                            }
                            break;

                        case InvestmentType.Reit:
                            profit.TaxValue = 0.2m;
                            break;

                        case InvestmentType.Derivative:
                            if (profit.ProfitType == ProfitType.DayTrade)
                                profit.TaxValue = 0.2m;

                            else
                                profit.TaxValue = 0.15m;
                            break;

                        default:
                            break;
                    }
                }

                bool profitAdded = await AddNewProfit(profit);
                if (!profitAdded)
                    throw new ApplicationException($"{entity.Code} operation are not added to database");
            }
        }

        private async Task<bool> IsTaxExemptAsync(DateTime operationDate, decimal sellValue)
        {
            IReadOnlyCollection<Stock> stockSells = await ListAsync<Stock>(x => x.OperationDate.Month == operationDate.Month
                                                                             && x.OperationDate.Year == operationDate.Year
                                                                             && x.OperationType == OperationType.Sell);

            decimal monthSellValue = stockSells.Sum(x => x.Total);
            monthSellValue += sellValue;

            //20000 is the maximun value, in the same month, to get TaxExempt in Brazil selling stocks
            if (monthSellValue < 20000)
                return true;
            else
            {
                IReadOnlyCollection<Profit> monthProfits = await SetAllMonthProfitsToNotTaxExempt(operationDate);
                Update<Profit>(monthProfits);
                return false;
            }
        }

        private async Task<ProfitType> GetProfitTypeAsync<TEntity>(Profit profit) where TEntity : VariableIncome
        {
            if (typeof(TEntity) == typeof(Reit))
                return ProfitType.Fii;

            IReadOnlyCollection<TEntity> entities = await ListAsync<TEntity>(x => x.OperationDate.Day == profit.OperationDate.Day
                                                                               && x.OperationDate.Month == profit.OperationDate.Month
                                                                               && x.OperationDate.Year == profit.OperationDate.Year
                                                                               && x.OperationType != profit.OperationType);

            if (entities.Count > 0)
                return ProfitType.DayTrade;
            else
                return ProfitType.SwingTrade;
        }

        private async Task<IReadOnlyCollection<Profit>> SetAllMonthProfitsToNotTaxExempt(DateTime operationDate)
        {
            IReadOnlyCollection<Profit> monthProfits = await ListAsync<Profit>(x => x.OperationDate.Month == operationDate.Month
                                                                                 && x.OperationDate.Year == operationDate.Year);

            foreach (Profit profit in monthProfits)
            {
                profit.TaxValue = 0.15m;
                profit.TaxExempt = false;
            }

            return monthProfits;
        }

        private async Task<bool> AddNewProfit(Profit profit)
        {
            bool added = await AddAsync<Profit>(profit);
            return added;
        }

        public async Task<bool> EditShareAsync<TEntity>(TEntity entity, TEntity oldEntity = null) where TEntity : VariableIncome
        {
            if (oldEntity is null)
                oldEntity = await GetAsync<TEntity>(x => x.Id == entity.Id);

            if (!Update(entity))
                return false;

            Portfolio portfolio = await GetAsync<Portfolio>(x => x.Code == entity.Code);

            portfolio.EditPortfolio(oldEntity, entity);

            return Update(portfolio);
        }

        public async Task<bool> DeleteShareAsync<TEntity>(Guid id, string code) where TEntity : VariableIncome
        {
            object deletedObject = await DeleteAsync<TEntity>(id);

            if (deletedObject is null)
                return false;

            Portfolio portfolio = await GetAsync<Portfolio>(x => x.Code == code);

            portfolio.DeleteVariableIncomeFromPortfolio((TEntity)deletedObject);

            return Update(portfolio);
        }

        public async Task<bool> DeleteShareAsync<TEntity>(TEntity entity) where TEntity : VariableIncome
        {
            bool deletedPortfolio = false;
            bool deletedEntity = Delete(entity);

            if (deletedEntity is false)
                return false;

            Portfolio portfolio = await GetAsync<Portfolio>(x => x.Code == entity.Code);

            if (entity.Amount == Math.Abs(portfolio.Quantity))
            {
                deletedPortfolio = Delete<Portfolio>(portfolio);
                return deletedPortfolio && deletedEntity;
            }
            else
            {
                portfolio.DeleteVariableIncomeFromPortfolio(entity);
                return Update(portfolio);
            }
        }

        public bool AddBonusShare(Portfolio portfolio, BonusShare bonusShare)
        {
            portfolio.AddBonusShareToPortfolio(bonusShare);

            return Update(portfolio);
        }
    }
}
