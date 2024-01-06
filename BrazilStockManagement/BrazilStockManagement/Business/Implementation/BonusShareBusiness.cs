using BrazilStockManagement.StockRepository.Interface;
using JBMDatabase.Business;

namespace BrazilStockManagement.Business.Implementation
{
    public class BonusShareBusiness : BaseBusiness
    {
        public BonusShareBusiness(IStockManagerRepository repository) : base(repository)
        {

        }
    }
}
