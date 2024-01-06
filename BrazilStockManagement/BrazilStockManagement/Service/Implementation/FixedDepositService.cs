using BrazilStockManagement.Business.Implementation;
using BrazilStockManagement.Entity;
using BrazilStockManagement.Enums;
using JBMDatabase;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace BrazilStockManagement.Service.Implementation
{
    public class FixedDepositService
    {
        private readonly FixedDepositBusiness FixedDepositBusiness;
        private readonly ShareHolderBusiness ShareHolderBusiness;

        public FixedDepositService(FixedDepositBusiness fixedDepositBusiness, ShareHolderBusiness shareHolderBusiness)
        {
            FixedDepositBusiness = fixedDepositBusiness;
            ShareHolderBusiness = shareHolderBusiness;
        }

        public async Task<bool> AddAsync(FixedDeposit fixedDeposit)
        {
            bool added = await FixedDepositBusiness.AddAsync(fixedDeposit);

            bool saved = false;
            if (added)
                saved = await FixedDepositBusiness.CommitAsync(added);

            return saved;
        }

        public async Task<IReadOnlyCollection<TEntity>> ListAsync<TEntity>(Expression<Func<TEntity, bool>>? expression = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null) where TEntity : BaseEntity
        {
            return await ShareHolderBusiness.ListAsync(expression, include);
        }

        public List<TEnum> GetEnumValues<TEnum>() where TEnum : struct
        {
            List<TEnum> fixedDepositsTypes = Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToList();

            return fixedDepositsTypes;
        }
    }
}
