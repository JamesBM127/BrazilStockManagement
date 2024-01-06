using BrazilStockManagement.Enums;

namespace BrazilStockManagement.Entity
{
    public class Profit : Operation
    {
        public decimal Value { get; set; }
        public bool TaxExempt { get; set; } = false;
        public decimal TaxValue { get; set; }
        public string Code { get; set; }
        public ProfitType ProfitType { get; set; }
        public TransactionType TransactionType { get; set; }
        public Guid VariableIncomeId { get; set; }
        public Guid ShareHolderId { get; set; }
        public ShareHolder ShareHolder { get; set; }
    }
}
