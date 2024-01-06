using BrazilStockManagement.Data;
using BrazilStockManagement.StockRepository.Interface;
using JBMDatabase.UnitOfWork;

namespace BrazilStockManagement.StockRepository.Implementation
{
    public class StockManagerRepository : UoW, IStockManagerRepository
    {
        public StockManagerRepository(StockContext context) : base(context)
        {
        }
    }
}
