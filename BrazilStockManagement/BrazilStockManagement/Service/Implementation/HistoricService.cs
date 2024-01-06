using BrazilStockManagement.Business.Implementation;
using BrazilStockManagement.Entity;
using BrazilStockManagement.Enums;
using JBMDatabase;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace BrazilStockManagement.Service.Implementation
{
    public class HistoricService
    {
        private readonly ShareHolderBusiness ShareHolderBusiness;
        private readonly PortfolioBusiness PortfolioBusiness;
        private readonly StockBusiness StockBusiness;
        private readonly ReitBusiness ReitBusiness;
        private readonly OptionsBusiness OptionsBusiness;
        private readonly FinancialInstitutionBusiness FinancialInstitutionBusiness;

        public HistoricService(ShareHolderBusiness shareHolderBusiness,
                               PortfolioBusiness portfolioBusiness,
                               StockBusiness stockBusiness,
                               ReitBusiness reitBusiness,
                               OptionsBusiness optionsBusiness,
                               FinancialInstitutionBusiness financialInstitutionBusiness)
        {
            ShareHolderBusiness = shareHolderBusiness;
            PortfolioBusiness = portfolioBusiness;
            StockBusiness = stockBusiness;
            ReitBusiness = reitBusiness;
            OptionsBusiness = optionsBusiness;
            FinancialInstitutionBusiness = financialInstitutionBusiness;
        }

        public async Task<ShareHolder> GetShareHolderAsync(Expression<Func<ShareHolder, bool>>? expression = null, Func<IQueryable<ShareHolder>, IIncludableQueryable<ShareHolder, object>> include = null)
        {
            var holder = await ShareHolderBusiness.ListAsync(expression, include);
            return holder.FirstOrDefault();
        }

        public async Task<IReadOnlyCollection<TEntity>> ListAsync<TEntity>(Expression<Func<TEntity, bool>>? expression = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null) where TEntity : BaseEntity
        {
            switch (typeof(TEntity).Name)
            {
                case "Stock":
                    return await StockBusiness.ListAsync(expression, include);

                case "Reit":
                    return await ReitBusiness.ListAsync(expression, include);

                case "FinancialInstitution":
                    return await FinancialInstitutionBusiness.ListAsync(expression, include);

                case "ShareHolder":
                    return await ShareHolderBusiness.ListAsync(expression, include);

                default:
                    return null;
            }
        }

        public async Task<IReadOnlyCollection<Portfolio>> ListPortfolioAsync(string holderName)
        {
            try
            {
                ShareHolder holder = await ShareHolderBusiness.GetAsync<ShareHolder>(x => x.Name == holderName);

                if (holder == null)
                    return null;

                return await PortfolioBusiness.ListAsync<Portfolio>(x => x.ShareHolderId == holder.Id);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IReadOnlyCollection<TEntity>> ListVariableIncome<TEntity>(string code) where TEntity : VariableIncome
        {
            IReadOnlyCollection<TEntity>? entities = null;

            if (typeof(TEntity) == typeof(Stock))
                entities = await StockBusiness.ListAsync<TEntity>(x => x.Code == code);

            else if (typeof(TEntity) == typeof(Reit))
                entities = await ReitBusiness.ListAsync<TEntity>(x => x.Code == code);

            return entities;
        }

        public async Task<List<VariableIncome>> ListVariableIncome(string code, string investmentType, IEnumerable<FinancialInstitution> financialInstitutions)
        {
            IEnumerable<VariableIncome>? entitiesFromDb = null;
            List<VariableIncome>? entititesToReturn = new();
            InvestmentType investmentTypeEnum = Enum.Parse<InvestmentType>(investmentType);

            switch (investmentTypeEnum)
            {
                case InvestmentType.Stock:
                    foreach (FinancialInstitution item1 in financialInstitutions)
                    {
                        entitiesFromDb = await StockBusiness.ListAsync<Stock>(x => x.Code == code && x.FinancialInstitutionId == item1.Id);
                        entititesToReturn.AddRange(entitiesFromDb);
                    }
                    break;

                case InvestmentType.Derivative:
                    foreach (FinancialInstitution item2 in financialInstitutions)
                    {
                        entitiesFromDb = await OptionsBusiness.ListAsync<Options>(x => x.Code == code && x.FinancialInstitutionId == item2.Id);
                        entititesToReturn.AddRange(entitiesFromDb);
                    }
                    break;

                case InvestmentType.Reit:
                    foreach (FinancialInstitution item3 in financialInstitutions)
                    {
                        entitiesFromDb = await ReitBusiness.ListAsync<Reit>(x => x.Code == code && x.FinancialInstitutionId == item3.Id);
                        entititesToReturn.AddRange(entitiesFromDb);
                    }
                    break;

                default:
                    break;
            }

            return entititesToReturn;
        }

        public async Task<InvestmentType> GetInvestmentType(string code)
        {
            Portfolio portfolio = await PortfolioBusiness.GetAsync<Portfolio>(x => x.Code == code);

            return portfolio.InvestmentType;
        }

        public string GetUrlEditIncome(VariableIncome income)
        {
            return $"/Edit/{income.InvestmentType}/{income.Id}";
        }
    }
}
