using BrazilStockManagement.Enums;

namespace BrazilStockManagement.Entity
{
    public class FixedDeposit : Investment
    {
        public decimal Profitability { get; set; }
        public DateTime? LiquidityDate { get; set; }
        public DateTime? RecordDate { get; set; }
        public DateTime DueDate { get; set; }
        public IndexType IndexType { get; set; }
        public FixedDepositsType FixedDepositsType { get; set; }

        public static FixedDeposit Clone(FixedDeposit fixedDeposit)
        {
            FixedDeposit? newFixedDeposit = new()
            {
                Id = fixedDeposit.Id,
                Amount = fixedDeposit.Amount,
                Company = fixedDeposit.Company,
                DueDate = fixedDeposit.DueDate,
                FinancialInstitution = fixedDeposit.FinancialInstitution,
                FinancialInstitutionId = fixedDeposit.FinancialInstitutionId,
                FixedDepositsType = fixedDeposit.FixedDepositsType,
                IndexType = fixedDeposit.IndexType,
                InvestmentType = fixedDeposit.InvestmentType,
                LiquidityDate = fixedDeposit.LiquidityDate,
                OperationDate = fixedDeposit.OperationDate,
                RecordDate = fixedDeposit.RecordDate,
                OperationType = fixedDeposit.OperationType,
                Price = fixedDeposit.Price,
                Profitability = fixedDeposit.Profitability
            };

            return newFixedDeposit;
        }
    }
}
