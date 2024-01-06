using BrazilStockManagement.Business.Implementation;
using BrazilStockManagement.Entity;
using BrazilStockManagement.Enums;

namespace BrazilStockManagement.Service.Implementation
{
    public class EditFixedDepositService
    {
        //private readonly PortfolioBusiness PortfolioBusiness;
        private readonly FixedDepositBusiness FixedDepositBusiness;

        public EditFixedDepositService(PortfolioBusiness portfolioBusiness, FixedDepositBusiness fixedDepositBusiness)
        {
            // PortfolioBusiness = portfolioBusiness;
            FixedDepositBusiness = fixedDepositBusiness;
        }

        public async Task<FixedDeposit> GetAsync(Guid id)
        {
            FixedDeposit fixedDeposit = await FixedDepositBusiness.GetAsync<FixedDeposit>(x => x.Id == id);

            return fixedDeposit;
        }

        public async Task<bool> Update(FixedDeposit fixedDeposit)
        {
            bool updated = FixedDepositBusiness.Update(fixedDeposit);
            bool saved = false;
            if (updated)
                saved = await FixedDepositBusiness.CommitAsync(updated);

            return saved;
        }

        public async Task<bool> Delete(FixedDeposit fixedDeposit)
        {
            bool deleted = FixedDepositBusiness.Delete(fixedDeposit);
            bool saved = false;
            if (deleted)
                saved = await FixedDepositBusiness.CommitAsync(deleted);

            return saved;
        }

        public List<TEnum> GetEnumValues<TEnum>() where TEnum : struct
        {
            List<TEnum> enumValuesList = Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToList();

            return enumValuesList;
        }
    }
}
