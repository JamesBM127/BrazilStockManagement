using BrazilStockManagement.Business;
using BrazilStockManagement.StockRepository.Interface;
using JBMDatabase.Business;

namespace BrazilStockManagement.Business.Implementation
{
    public class ReitBusiness : BaseBusiness
    {
        public ReitBusiness(IStockManagerRepository repository) : base(repository)
        {
        }
    }
}
