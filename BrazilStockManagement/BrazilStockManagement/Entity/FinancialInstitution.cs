using JBMDatabase;

namespace BrazilStockManagement.Entity
{
    public class FinancialInstitution : BaseEntity
    {
        public string Name { get; set; }
        public string Agency { get; set; }
        public string Account { get; set; }
        public string Digit { get; set; }
        public Guid ShareHolderId { get; set; }
        public ShareHolder ShareHolder { get; set; }
    }
}
