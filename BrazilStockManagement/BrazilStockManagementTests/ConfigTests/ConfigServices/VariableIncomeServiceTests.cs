using BrazilStockManagement.Service.Implementation;

namespace BrazilStockManagementTests.ConfigTests.ConfigServices
{
    public class VariableIncomeServiceTests : VariableIncomeService
    {
        public VariableIncomeServiceTests(StockBusiness stockBusiness = null, 
                                          ReitBusiness reitBusiness = null, 
                                          OptionsBusiness optionsBusiness = null, 
                                          PortfolioBusiness portfolioBusiness = null,
                                          FinancialInstitutionBusiness financialInstitutionBusiness = null, 
                                          ShareHolderBusiness shareHolderBusiness = null) 
                                          : base(stockBusiness, reitBusiness, portfolioBusiness, optionsBusiness, financialInstitutionBusiness, shareHolderBusiness)
        {
        }
    }
}
