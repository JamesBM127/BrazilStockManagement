using BrazilStockManagement.Business.Implementation;
using BrazilStockManagement.Entity;
using System.Linq.Expressions;

namespace BrazilStockManagement.Service.Implementation
{
    public class ReceivedService
    {
        private readonly DividendBusiness DividendBusiness;
        private readonly BonusShareBusiness BonusShareBusiness;
        private readonly PortfolioBusiness PortfolioBusiness;

        public ReceivedService(DividendBusiness dividendBusiness,
                               BonusShareBusiness bonusShareBusiness,
                               PortfolioBusiness portfolioBusiness)
        {
            DividendBusiness = dividendBusiness;
            BonusShareBusiness = bonusShareBusiness;
            PortfolioBusiness = portfolioBusiness;
        }

        public async Task<bool> AddReceivedAsync<TEntity>(TEntity entity) where TEntity : Received
        {
            bool added, saved = false;

            if (typeof(TEntity) == typeof(Dividend))
            {
                added = await DividendBusiness.AddAsync(entity);
                saved = await DividendBusiness.CommitAsync(added);
            }
            else if (typeof(TEntity) == typeof(BonusShare))
            {
                BonusShare bonusShare = new(entity as BonusShare);
                Portfolio portfolio = await PortfolioBusiness.GetAsync<Portfolio>(x => x.Id == bonusShare.PortfolioId);
                bonusShare.Amount = (int)Math.Floor((bonusShare.Proportion/100) * portfolio.Quantity);

                added = await BonusShareBusiness.AddAsync(bonusShare);
                if (added)
                {
                    bool portfolioUpdated = PortfolioBusiness.AddBonusShare(portfolio, bonusShare);
                    saved = await BonusShareBusiness.CommitAsync(portfolioUpdated);
                }
            }

            return saved;
        }

        public async Task<bool> UpdateReceivedAsync<TEntity>(TEntity entity) where TEntity : Received
        {
            bool updated, saved = false;

            if (typeof(TEntity) == typeof(Dividend))
            {
                updated = DividendBusiness.Update(entity);
                saved = await DividendBusiness.CommitAsync(updated);
            }
            else if (typeof(TEntity) == typeof(BonusShare))
            {
                BonusShare oldBonusShare = await BonusShareBusiness.GetAsync<BonusShare>(x => x.Id == entity.Id);
                BonusShare newBonusShare = new(entity as BonusShare);

                Portfolio portfolio = await PortfolioBusiness.GetAsync<Portfolio>(x => x.Id == entity.PortfolioId);
                portfolio.RoolbackUpdateBonusShare(oldBonusShare);

                newBonusShare.Amount = (int)Math.Floor(newBonusShare.Proportion * portfolio.Quantity);

                portfolio.AddBonusShareToPortfolio(newBonusShare);

                updated = BonusShareBusiness.Update(newBonusShare);
                if (updated)
                {
                    bool portfolioUpdated = PortfolioBusiness.Update(portfolio);
                    saved = await BonusShareBusiness.CommitAsync(portfolioUpdated);
                }
            }

            return saved;
        }

        public async Task<IReadOnlyCollection<TEntity>> ListReceivedAsync<TEntity>(Expression<Func<TEntity, bool>>? expression = null) where TEntity : Received
        {
            if (typeof(TEntity) == typeof(Dividend))
                return await DividendBusiness.ListAsync(expression);

            else if (typeof(TEntity) == typeof(BonusShare))
                return await BonusShareBusiness.ListAsync(expression);

            return null;
        }

        public async Task<bool> DeleteReceivedAsync<TEntity>(TEntity entity) where TEntity : Received
        {
            bool deleted, saved = false;

            if (typeof(TEntity) == typeof(Dividend))
            {
                deleted = DividendBusiness.Delete(entity);
                saved = await DividendBusiness.CommitAsync(deleted);
            }
            else if (typeof(TEntity) == typeof(BonusShare))
            {
                BonusShare oldBonusShare = await BonusShareBusiness.GetAsync<BonusShare>(x => x.Id == entity.Id);
                Portfolio portfolio = await PortfolioBusiness.GetAsync<Portfolio>(x => x.Id == entity.PortfolioId);
                portfolio.RoolbackUpdateBonusShare(oldBonusShare);

                deleted = BonusShareBusiness.Delete(entity);
                if (deleted)
                {
                    bool portfolioUpdated = PortfolioBusiness.Update(portfolio);
                    saved = await BonusShareBusiness.CommitAsync(portfolioUpdated);
                }
            }

            return saved;
        }
    }
}
