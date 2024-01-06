using BrazilStockManagement.Business.Implementation;
using BrazilStockManagement.Entity;
using BrazilStockManagement.Enums;

namespace BrazilStockManagement.Service.Implementation
{
    public class PortfolioFrontService
    {
        private readonly PortfolioBusiness PortfolioBusiness;
        private readonly ShareHolderBusiness ShareHolderBusiness;
        private readonly FinancialInstitutionBusiness FinancialInstitutionBusiness;
        private readonly FixedDepositBusiness FixedDepositBusiness;
        private readonly StockBusiness StockBusiness;
        private readonly ReitBusiness ReitBusiness;

        private IReadOnlyCollection<Portfolio> PortfolioList;

        public PortfolioFrontService(PortfolioBusiness portfolioBusiness,
                                     ShareHolderBusiness shareHolderBusiness,
                                     FinancialInstitutionBusiness financialInstitutionBusiness,
                                     FixedDepositBusiness fixedDepositBusiness,
                                     StockBusiness stockBusiness,
                                     ReitBusiness reitBusiness)
        {
            PortfolioBusiness = portfolioBusiness;
            ShareHolderBusiness = shareHolderBusiness;
            FinancialInstitutionBusiness = financialInstitutionBusiness;
            FixedDepositBusiness = fixedDepositBusiness;
            StockBusiness = stockBusiness;
            ReitBusiness = reitBusiness;
        }

        public async Task<bool> AddAsync<TEntity>(TEntity entity) where TEntity : VariableIncome
        {
            bool saved = false;
            try
            {
                saved = await PortfolioBusiness.BuyOrSellPortfolioAsync(entity);
                if (saved)
                {
                    saved = await PortfolioBusiness.CommitAsync(saved);
                    Portfolio port = await PortfolioBusiness.GetAsync<Portfolio>(x => x.Code == entity.Code);
                    PortfolioBusiness.Detach(entity);
                }
            }
            catch (Exception ex)
            {
                //PortfolioBusiness.Rollback();
            }
            return saved;
        }

        public async Task<IReadOnlyCollection<Portfolio>> GetPortfolioListAsync(string holderName)
        {
            try
            {
                ShareHolder holder = await ShareHolderBusiness.GetAsync<ShareHolder>(x => x.Name == holderName);

                if (holder == null)
                    return null;

                PortfolioList = await PortfolioBusiness.ListAsync<Portfolio>(x => x.ShareHolderId == holder.Id);
            }
            catch (Exception ex)
            {
                throw;
            }

            return PortfolioList;
        }

        public async Task<IReadOnlyCollection<FixedDeposit>> GetFixedDepositListAsync(string holderName)
        {
            IReadOnlyCollection<FixedDeposit>? fixedDeposits = null;

            ShareHolder holder = await ShareHolderBusiness.GetAsync<ShareHolder>(x => x.Name == holderName);
            if (holder == null)
                return null;

            FinancialInstitution financialInstitution = await FinancialInstitutionBusiness.GetAsync<FinancialInstitution>(x => x.ShareHolderId == holder.Id);
            if (financialInstitution == null)
                return null;

            fixedDeposits = await FixedDepositBusiness.ListAsync<FixedDeposit>(x => x.FinancialInstitutionId == financialInstitution.Id);


            return fixedDeposits;
        }

        public async Task<IReadOnlyCollection<FinancialInstitution>> GetInstitutions(string holderName)
        {
            ShareHolder holder = await ShareHolderBusiness.GetAsync<ShareHolder>(x => x.Name == holderName);

            if (holder == null)
                return null;

            IReadOnlyCollection<FinancialInstitution> financialInstitution = await FinancialInstitutionBusiness.ListAsync<FinancialInstitution>(x => x.ShareHolderId == holder.Id);
            return financialInstitution;
        }

        public async Task<TEntity> GetVariableIncome<TEntity>(string code) where TEntity : VariableIncome
        {
            VariableIncome? variableIncome = null;

            if (typeof(TEntity) == typeof(Stock))
            {
                variableIncome = await StockBusiness.GetAsync<Stock>(x => x.Code == code);
            }
            else if (typeof(TEntity) == typeof(Reit))
            {
                variableIncome = await ReitBusiness.GetAsync<Reit>(x => x.Code == code);
            }

            return variableIncome as TEntity;
        }

        public decimal GetPosition(string code, int shareQuantity)
        {
            //Received the true price from API, this is a example;
            decimal sharePrice = 12.12m;
            decimal value = Math.Round(shareQuantity * sharePrice, 2);
            return value;
        }

        public decimal GetAveragePrice(decimal spent, int shareQuantity)
        {
            decimal averagePrice = 0;
            try
            {
                averagePrice = Math.Round(spent / shareQuantity, 2);
            }
            catch (DivideByZeroException ex)
            {
                averagePrice = 0;
            }

            return averagePrice;
        }

        public decimal GetAlocation(string code, int shareQuantity, decimal patrimony)
        {
            decimal position = GetPosition(code, shareQuantity);

            if (position <= 0)
                return 0;

            decimal allocation = 0;
            try
            {
                allocation = Math.Round(position / patrimony * 100, 2);
            }
            catch (DivideByZeroException ex)
            {
                allocation = 0;
            }

            return allocation;
        }

        public decimal GetPatrimony()
        {
            decimal patrimony = 0;
            if (PortfolioList is null)
                return 0;
            foreach (var item in PortfolioList)
            {
                if (item.Quantity > 0)
                    patrimony += GetPosition(item.Code, item.Quantity);
            }

            return patrimony;
        }

        public IEnumerable<Portfolio> GetOnlyStocks(IReadOnlyCollection<Portfolio> portfolioList)
        {
            if (portfolioList is not null)
                return portfolioList.Where(x => x.InvestmentType == InvestmentType.Stock);

            return null;
        }

        public IEnumerable<Portfolio> GetOnlyReits(IReadOnlyCollection<Portfolio> portfolioList)
        {
            if (portfolioList is not null)
                return portfolioList.Where(x => x.InvestmentType == InvestmentType.Reit);

            return null;
        }

        public IEnumerable<Portfolio> GetOnlyOptions(IReadOnlyCollection<Portfolio> portfolioList)
        {
            if (portfolioList is not null)
                return portfolioList.Where(x => x.InvestmentType == InvestmentType.Derivative);

            return null;
        }
    }
}
