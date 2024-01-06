using BrazilStockManagement.Business.Implementation;

namespace BrazilStockManagement.Service
{
    public class BaseService
    {
        public readonly BonusShareBusiness BonusShareBusiness;
        public readonly DividendBusiness DividendBusiness;
        public readonly FinancialInstitutionBusiness FinancialInstitutionBusiness;
        public readonly FixedDepositBusiness FixedDepositBusiness;
        public readonly OptionsBusiness OptionsBusiness;
        public readonly PortfolioBusiness PortfolioBusiness;
        public readonly ProfitBusiness ProfitBusiness;
        public readonly ReitBusiness ReitBusiness;
        public readonly ShareHolderBusiness ShareHolderBusiness;
        public readonly StockBusiness StockBusiness;

        public BaseService(BonusShareBusiness bonusShareBusiness, 
                              DividendBusiness dividendBusiness, 
                              FinancialInstitutionBusiness financialInstitutionBusiness, 
                              FixedDepositBusiness fixedDepositBusiness, 
                              OptionsBusiness optionsBusiness, 
                              PortfolioBusiness portfolioBusiness, 
                              ProfitBusiness profitBusiness, 
                              ReitBusiness reitBusiness, 
                              ShareHolderBusiness shareHolderBusiness,
                              StockBusiness stockBusiness)
        {
            BonusShareBusiness = bonusShareBusiness;
            DividendBusiness = dividendBusiness;
            FinancialInstitutionBusiness = financialInstitutionBusiness;
            FixedDepositBusiness = fixedDepositBusiness;
            OptionsBusiness = optionsBusiness;
            PortfolioBusiness = portfolioBusiness;
            ProfitBusiness = profitBusiness;
            ReitBusiness = reitBusiness;
            ShareHolderBusiness = shareHolderBusiness;
            StockBusiness = stockBusiness;
        }
    }
}
