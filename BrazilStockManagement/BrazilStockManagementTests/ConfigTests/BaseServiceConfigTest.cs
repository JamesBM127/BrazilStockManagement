using BrazilStockManagement.Service;

namespace BrazilStockManagementTests.ConfigTests
{
    public class BaseServiceConfigTest : BaseService
    {
        public BaseServiceConfigTest(BonusShareBusiness bonusShareBusiness,
                                     DividendBusiness dividendBusiness,
                                     FinancialInstitutionBusiness financialInstitutionBusiness,
                                     FixedDepositBusiness fixedDepositBusiness,
                                     OptionsBusiness optionsBusiness,
                                     PortfolioBusiness portfolioBusiness,
                                     ProfitBusiness profitBusiness,
                                     ReitBusiness reitBusiness,
                                     ShareHolderBusiness shareHolderBusiness,
                                     StockBusiness stockBusiness)
                                     
                                      : base(bonusShareBusiness,
                                             dividendBusiness,
                                             financialInstitutionBusiness,
                                             fixedDepositBusiness,
                                             optionsBusiness,
                                             portfolioBusiness,
                                             profitBusiness,
                                             reitBusiness,
                                             shareHolderBusiness,
                                             stockBusiness)
        {
        }
    }
}
