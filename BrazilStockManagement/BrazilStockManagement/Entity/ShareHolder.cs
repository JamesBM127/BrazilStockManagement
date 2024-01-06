using JBMDatabase;

namespace BrazilStockManagement.Entity
{
    public class ShareHolder : BaseEntity
    {
        public string Name { get; set; }
        public string CpfCnpj { get; set; }
        public IEnumerable<FinancialInstitution> FinancialInstitutions { get; set; }
        public IEnumerable<Portfolio> Portfolios { get; set; }
        public IEnumerable<Profit> Profits { get; set; }
    }
}
