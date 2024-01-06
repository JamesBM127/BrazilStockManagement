using BrazilStockManagement.Enums;
using JBMDatabase;

namespace BrazilStockManagement.Entity
{
    public class Portfolio : BaseEntity
    {
        public string Code { get; set; }
        public string Company { get; set; }
        public int Quantity { get; set; }
        public decimal Total { get; set; }
        public InvestmentType InvestmentType { get; set; }
        public Guid ShareHolderId { get; set; }
        public ShareHolder ShareHolder { get; set; }

        public Portfolio()
        {
        }

        public Portfolio(Portfolio portfolio)
        {
            Code = portfolio.Code;
            Company = portfolio.Company;
            Quantity = portfolio.Quantity;
            Total = portfolio.Total;
            InvestmentType = portfolio.InvestmentType;
            ShareHolderId = portfolio.ShareHolderId;
            ShareHolder = portfolio.ShareHolder;
        }
    }
}
