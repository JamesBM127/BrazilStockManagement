using BrazilStockManagement.StockRepository.Interface;
using JBMDatabase.Business;

namespace BrazilStockManagement.Business.Implementation
{
    public class OptionsBusiness : BaseBusiness
    {
        public OptionsBusiness(IStockManagerRepository repository) : base(repository)
        {

        }
    }
}
