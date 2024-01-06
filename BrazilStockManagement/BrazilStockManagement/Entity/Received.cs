using BrazilStockManagement.Enums;
using JBMDatabase;

namespace BrazilStockManagement.Entity
{
    public abstract class Received : BaseEntity
    {
        public decimal Value { get; set; }
        public int Amount { get; set; }
        public DateTime ExDate { get; set; }
        public DateTime RecordDate { get; set; }
        public DateTime? PaymentDate { get; set; }
        public Guid? PortfolioId { get; set; }
        public Portfolio? Portfolio { get; set; }
    }
}
