using BrazilStockManagement.Business.Implementation;
using BrazilStockManagement.Entity;
using BrazilStockManagement.Enums;
using BrazilStockManagement.Enums;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using System.Text;

namespace BrazilStockManagement.Service.Implementation
{
    public class VariableIncomeService
    {
        private readonly StockBusiness StockBusiness;
        private readonly ReitBusiness ReitBusiness;
        private readonly OptionsBusiness OptionsBusiness;
        private readonly PortfolioBusiness PortfolioBusiness;
        private readonly ShareHolderBusiness ShareHolderBusiness;
        private readonly FinancialInstitutionBusiness FinancialInstitutionBusiness;

        public VariableIncomeService(StockBusiness stockBusiness,
                                     ReitBusiness reitBusiness,
                                     PortfolioBusiness portfolioBusiness,
                                     OptionsBusiness optionsBusiness,
                                     FinancialInstitutionBusiness financialInstitutionBusiness,
                                     ShareHolderBusiness shareHolderBusiness)
        {
            StockBusiness = stockBusiness;
            ReitBusiness = reitBusiness;
            OptionsBusiness = optionsBusiness;
            PortfolioBusiness = portfolioBusiness;
            FinancialInstitutionBusiness = financialInstitutionBusiness;
            ShareHolderBusiness = shareHolderBusiness;
        }

        public async Task<bool> AddNewVariableIncome<TEntity>(TEntity entity) where TEntity : VariableIncome
        {
            bool added = false;

            if (entity.Company == null || string.IsNullOrWhiteSpace(entity.Company))
                entity.Company = entity.Code;

            if (typeof(TEntity) == typeof(VariableIncome))
            {
                switch (entity.InvestmentType)
                {
                    case InvestmentType.Stock:
                        added = await AddNewStock(entity);
                        break;

                    case InvestmentType.Reit:
                        added = await AddNewReit(entity);
                        break;

                    case InvestmentType.Derivative:
                        added = await AddVariableIncomeAsync(entity);
                        break;
                }
            }

            else
                added = await AddVariableIncomeAsync(entity);

            return added;
        }

        public async Task<bool> AddNewVariableIncome<TEntity>(TEntity entity, Portfolio portfolio = null) where TEntity : VariableIncome
        {
            bool added = false;

            if (entity.Company == null || string.IsNullOrWhiteSpace(entity.Company))
                entity.Company = entity.Code;

            if (typeof(TEntity) == typeof(VariableIncome))
            {
                switch (entity.InvestmentType)
                {
                    case InvestmentType.Stock:
                        added = await AddNewStock(entity, portfolio);
                        break;

                    case InvestmentType.Reit:
                        added = await AddNewReit(entity, portfolio);
                        break;

                    case InvestmentType.Derivative:
                        added = await AddVariableIncomeAsync(entity, portfolio);
                        break;
                }
            }

            else
                added = await AddVariableIncomeAsync(entity, portfolio);

            return added;
        }

        private async Task<bool> AddNewStock<TEntity>(TEntity entity, Portfolio portfolio = null) where TEntity : VariableIncome
        {
            Stock stock = VariableIncome.ConvertTo<Stock>(entity);
            return await AddVariableIncomeAsync(stock, portfolio);
        }

        private async Task<bool> AddNewReit<TEntity>(TEntity entity, Portfolio portfolio = null) where TEntity : VariableIncome
        {
            Reit reit = VariableIncome.ConvertTo<Reit>(entity);
            return await AddVariableIncomeAsync(reit, portfolio);
        }

        private async Task<bool> AddVariableIncomeAsync<TEntity>(TEntity entity, Portfolio portfolio = null) where TEntity : VariableIncome
        {
            bool updated = await PortfolioBusiness.BuyOrSellPortfolioAsync(entity, portfolio);
            bool saved = await PortfolioBusiness.CommitAsync(updated);

            return saved;
        }

        public async Task<TEntity> GetEntity<TEntity>(Expression<Func<TEntity, bool>> expression = null) where TEntity : VariableIncome
        {
            TEntity entity = null;

            Type entityType = typeof(TEntity);

            if (entityType == typeof(Stock))
                entity = await StockBusiness.GetAsync(expression);

            else if (entityType == typeof(Reit))
                entity = await ReitBusiness.GetAsync(expression);

            else if (entityType == typeof(Options))
                entity = await OptionsBusiness.GetAsync(expression);

            return entity;
        }

        public string GetUrlHistoric(Portfolio portfolio)
        {
            return $"/Historico/{portfolio.ShareHolder.Name}/{portfolio.InvestmentType}/{portfolio.Code}";
        }

        public string GetCollapseStockNumber(ref uint collapseStocks, bool next = false)
        {
            if (next)
                return "collapseStocks" + collapseStocks++.ToString();
            else
                return "collapseStocks" + collapseStocks.ToString();
        }

        public string GetCollapseReitNumber(ref uint collapseReit, bool next = false)
        {
            if (next)
                return "collapseReit" + collapseReit++.ToString();
            else
                return "collapseReit" + collapseReit.ToString();
        }

        public string GetCollapseOptionsNumber(ref uint collapseOptions, bool next = false)
        {
            if (next)
                return "collapseOptions" + collapseOptions++.ToString();
            else
                return "collapseOptions" + collapseOptions.ToString();
        }

        public async Task<IReadOnlyCollection<Portfolio>> ListAsync()
        {
            IReadOnlyCollection<Portfolio>? entities = await PortfolioBusiness.ListAsync<Portfolio>();

            return entities;
        }

        public async Task<ShareHolder> GetShareHolderAsync(Expression<Func<ShareHolder, bool>>? expression = null, Func<IQueryable<ShareHolder>, IIncludableQueryable<ShareHolder, object>> include = null)
        {
            var holder = await ShareHolderBusiness.ListAsync(expression, include);
            return holder.FirstOrDefault();
        }

        public async Task<IReadOnlyCollection<FinancialInstitution>> GetFinancialInstitutionsAsync(Guid shareHolderId)
        {
            return await FinancialInstitutionBusiness.ListAsync<FinancialInstitution>(x => x.ShareHolderId == shareHolderId);
        }

        public DerivativeType GetDerivativeType(string optionCode)
        {
            string codeLetter = optionCode[4].ToString();

            byte[] codeLetterAscii = Encoding.ASCII.GetBytes(codeLetter.ToUpper());

            if (codeLetterAscii.FirstOrDefault() >= 65 && codeLetterAscii.FirstOrDefault() <= 76)
                return DerivativeType.Call;

            if (codeLetterAscii.FirstOrDefault() >= 77 && codeLetterAscii.FirstOrDefault() <= 88)
                return DerivativeType.Put;

            else
                throw new ArgumentOutOfRangeException(optionCode, "Código da opção inválido!");
        }

        public List<TEnum> GetEnumValues<TEnum>() where TEnum : struct
        {
            List<TEnum> dividendTypes = Enum.GetValues(typeof(DividendType)).Cast<TEnum>().ToList();

            return dividendTypes;
        }

        public void NormalizeLot(ref int amount, ref int oldAmount)
        {
            if (amount < 100)
                amount = SetToFullStandartLot(amount);

            else if (amount > 100 && amount % 100 != 0)
                amount = StandartLot(amount, oldAmount);

            oldAmount = amount;
        }

        public async Task<bool> SetInplit(Portfolio portfolio, int factor)
        {
            portfolio.Quantity = (int)Math.Floor((double)portfolio.Quantity / factor);
            bool updated = PortfolioBusiness.Update(portfolio);
            return await PortfolioBusiness.CommitAsync(updated);
        }

        public async Task<bool> SetSplit(Portfolio portfolio, int factor)
        {
            portfolio.Quantity = portfolio.Quantity * factor;
            bool updated = PortfolioBusiness.Update(portfolio);
            return await PortfolioBusiness.CommitAsync(updated);
        }

        private int SetToFullStandartLot(int amount)
        {
            return amount *= 100;
        }

        private int StandartLot(int amount, int oldAmount)
        {
            //Add only one standart lot
            if (amount - oldAmount == 1)
            {
                amount--;
                amount += 100;
            }
            //Remove only one standart lot
            else if (oldAmount - amount == 1)
            {
                amount++;
                amount -= 100;
            }
            //Set a standart lot
            else
            {
                decimal newValue = amount / 100;
                newValue = Math.Floor(newValue);
                amount = (int)newValue * 100;
            }

            return amount;
        }
    }
}
