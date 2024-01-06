using BrazilStockManagement.StockRepository.Interface;
using JBMDatabase.Business;

namespace BrazilStockManagement.Business.Implementation
{
    public class DividendBusiness : BaseBusiness
    {
        public DividendBusiness(IStockManagerRepository repository) : base(repository)
        {
        }
    }
}
