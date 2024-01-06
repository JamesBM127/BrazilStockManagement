using JBMDatabase.UnitOfWork;
using System.Diagnostics;

namespace BrazilStockManagement.Entity
{
    public class Options : Derivative
    {
        public decimal Strike { get; set; }
        public string StockCode { get; set; }
    }
}
