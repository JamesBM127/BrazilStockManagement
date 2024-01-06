using BrazilStockManagement.StockRepository.Interface;
using JBMDatabase.Business;

namespace BrazilStockManagement.Business.Implementation
{
    public class FixedDepositBusiness : BaseBusiness
    {
        public FixedDepositBusiness(IStockManagerRepository repository) : base(repository)
        {
        }
    }
}
