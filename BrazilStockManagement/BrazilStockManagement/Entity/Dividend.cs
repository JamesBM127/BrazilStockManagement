using BrazilStockManagement.Enums;

namespace BrazilStockManagement.Entity
{
    public class Dividend : Received
    {
        public DividendType DividendType { get; set; }
    }
}
