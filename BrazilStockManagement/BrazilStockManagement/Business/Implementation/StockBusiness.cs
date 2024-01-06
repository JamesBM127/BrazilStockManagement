using BrazilStockManagement.StockRepository.Interface;
using JBMDatabase.Business;

namespace BrazilStockManagement.Business.Implementation
{
    public class StockBusiness : BaseBusiness
    {
        public StockBusiness(IStockManagerRepository repository) : base(repository)
        {

        }
    }
}
