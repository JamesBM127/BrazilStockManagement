using JBMDatabase;

namespace BrazilStockManagement.Entity
{
    public class BonusShare : Received
    {
        public decimal Proportion { get; set; }

        public BonusShare()
        {
        }

        public BonusShare(BonusShare bonusShare)
        {
            Id = bonusShare.Id;
            Proportion = bonusShare.Proportion;
            Value = bonusShare.Value;
            Amount = bonusShare.Amount;
            ExDate = bonusShare.ExDate;
            RecordDate = bonusShare.RecordDate;
            PaymentDate = bonusShare.PaymentDate;
            PortfolioId = bonusShare.PortfolioId;
            Portfolio = bonusShare.Portfolio;
        }
    }
}
