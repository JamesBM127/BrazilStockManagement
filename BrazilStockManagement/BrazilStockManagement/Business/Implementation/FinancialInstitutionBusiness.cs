using BrazilStockManagement.StockRepository.Interface;
using JBMDatabase.Business;

namespace BrazilStockManagement.Business.Implementation
{
    public class FinancialInstitutionBusiness : BaseBusiness
    {
        public FinancialInstitutionBusiness(IStockManagerRepository repository) : base(repository)
        {
        }
    }
}
