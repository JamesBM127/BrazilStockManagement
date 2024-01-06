using BrazilStockManagement.StockRepository.Interface;
using JBMDatabase.Business;

namespace BrazilStockManagement.Business.Implementation
{
    public class ProfitBusiness : BaseBusiness
    {
        public ProfitBusiness(IStockManagerRepository repository) : base(repository)
        {
        }
    }
}
