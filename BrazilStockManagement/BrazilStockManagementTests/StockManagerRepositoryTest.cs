using JBMDatabase.Repos;
using JBMDatabase.UnitOfWork;

namespace BrazilStockManagementTests
{
    public class StockManagerRepositoryTest : UoW, IStockManagerRepository
    {
        public StockManagerRepositoryTest(StockTestContext context) : base(context)
        {
        }
    }
}
