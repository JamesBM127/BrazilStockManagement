using BrazilStockManagement.StockRepository.Interface;
using JBMDatabase.Business;

namespace BrazilStockManagement.Business.Implementation
{
    public class ShareHolderBusiness : BaseBusiness
    {
        public ShareHolderBusiness(IStockManagerRepository repository) : base(repository)
        {
        }
    }
}
