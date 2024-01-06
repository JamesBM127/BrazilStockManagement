using BrazilStockManagement.Enums;
using JBMDatabase;

namespace BrazilStockManagement.Entity
{
    public abstract class Operation : BaseEntity
    {
        public DateTime OperationDate { get; set; }
        public OperationType OperationType { get; set; }
        public InvestmentType InvestmentType { get; set; }
    }
}
