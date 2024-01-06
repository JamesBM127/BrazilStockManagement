using BrazilStockManagement.Business.Implementation;
using BrazilStockManagement.Entity;
using JBMDatabase;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace BrazilStockManagement.Service.Implementation
{
    public class FinancialInstitutionService
    {
        private readonly FinancialInstitutionBusiness FinancialInstitutionBusiness;

        public FinancialInstitutionService(FinancialInstitutionBusiness financialInstitutionBusiness)
        {
            FinancialInstitutionBusiness = financialInstitutionBusiness;
        }

        public async Task<bool> AddAsync(FinancialInstitution institution)
        {
            bool saved = false;
            try
            {
                saved = await FinancialInstitutionBusiness.AddAsync(institution);
                if (saved)
                {
                    saved = await FinancialInstitutionBusiness.CommitAsync(saved);
                    FinancialInstitutionBusiness.Detach(institution);
                }
            }
            catch (Exception ex)
            {
                //FinancialInstitutionBusiness.Rollback();
            }
            return saved;
        }

        public async Task<IReadOnlyCollection<TEntity>> ListAsync<TEntity>(Expression<Func<TEntity, bool>>? expression = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null) where TEntity : BaseEntity
        {
            return await FinancialInstitutionBusiness.ListAsync(expression, include);
        }

        public Guid GetHolderIdAsync(IReadOnlyCollection<ShareHolder> shareHolders, string holder)
        {
            Guid id = shareHolders.Where(x => x.Name == holder).First().Id;
            return id;
        }
    }
}
