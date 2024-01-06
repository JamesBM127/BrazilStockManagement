using BrazilStockManagement.Business.Implementation;
using BrazilStockManagement.Entity;
using BrazilStockManagement.Enums;
using JBMDatabase.UnitOfWork;

namespace BrazilStockManagement.Service.Implementation
{
    public class EditVariableIncomeService : BaseService
    {
        public EditVariableIncomeService(BonusShareBusiness bonusShareBusiness, DividendBusiness dividendBusiness, FinancialInstitutionBusiness financialInstitutionBusiness, FixedDepositBusiness fixedDepositBusiness, OptionsBusiness optionsBusiness, PortfolioBusiness portfolioBusiness, ProfitBusiness profitBusiness, ReitBusiness reitBusiness, ShareHolderBusiness shareHolderBusiness, StockBusiness stockBusiness) : base(bonusShareBusiness, dividendBusiness, financialInstitutionBusiness, fixedDepositBusiness, optionsBusiness, portfolioBusiness, profitBusiness, reitBusiness, shareHolderBusiness, stockBusiness)
        {
        }

        public async Task<VariableIncome> GetAsync(Guid id, string investmentTypeString)
        {
            VariableIncome variableIncome = null;

            if (investmentTypeString == InvestmentType.Stock.ToString())
                variableIncome = await StockBusiness.GetAsync<Stock>(x => x.Id == id);
            else if (investmentTypeString == InvestmentType.Reit.ToString())
                variableIncome = await ReitBusiness.GetAsync<Reit>(x => x.Id == id);
            else
                variableIncome = await OptionsBusiness.GetAsync<Options>(x => x.Id == id);

            return variableIncome;
        }

        public async Task<FinancialInstitution> GetInstitutionAsync(string name)
        {
            FinancialInstitution financialInstitution = await FinancialInstitutionBusiness.GetAsync<FinancialInstitution>(x => x.Name == name);
            return financialInstitution;
        }

        public async Task<IReadOnlyCollection<FinancialInstitution>> GetInstitutionsListAsync()
        {
            return await FinancialInstitutionBusiness.ListAsync<FinancialInstitution>();
        }

        public async Task<bool> Update(VariableIncome variableIncome, VariableIncome oldEntity = null)
        {
            bool updated = false;

            if (variableIncome.InvestmentType == InvestmentType.Stock)
            {
                Stock stock = (Stock)variableIncome.Clone();
                //Stock stock = VariableIncome.ConvertToLegacy<Stock>(variableIncome);
                updated = await UpdateAsync(stock, oldEntity);
            }
            else if (variableIncome.InvestmentType == InvestmentType.Stock)
            {
                Reit reit = (Reit)variableIncome.Clone();
                //Reit reit = VariableIncome.ConvertToLegacy<Reit>(variableIncome);
                updated = await UpdateAsync(reit, oldEntity);
            }
            else
            {
                Options options = (Options)variableIncome.Clone();
                //Options options = VariableIncome.ConvertTo<Options>(variableIncome);
                
                updated = await UpdateAsync(options, oldEntity);
            }

            return updated;
        }

        private async Task<bool> UpdateAsync<TEntity>(TEntity entity, TEntity oldEntity = null) where TEntity : VariableIncome
        {
            bool updated = await PortfolioBusiness.EditShareAsync(entity, oldEntity);
            bool saved = false;
            if (updated)
                saved = await PortfolioBusiness.CommitAsync(updated);

            return saved;
        }

        public async Task<bool> Delete<TEntity>(Guid id, string code) where TEntity : VariableIncome
        {
            bool deleted = await PortfolioBusiness.DeleteShareAsync<TEntity>(id, code);
            
            return await PortfolioBusiness.CommitAsync(deleted);
        }

        public async Task<bool> Delete<TEntity>(VariableIncome variableIncome) where TEntity : VariableIncome
        {
            TEntity entity = (TEntity)variableIncome.Clone();
            //TEntity entity = VariableIncome.ConvertTo<TEntity>(income);
            
            bool deleted = await PortfolioBusiness.DeleteShareAsync(entity);
            Profit profit = await ProfitBusiness.GetAsync<Profit>(x => x.VariableIncomeId == entity.Id);
            bool profitIsOk = false;

            if (profit != null)
                profitIsOk = ProfitBusiness.Delete<Profit>(profit);
            else 
                profitIsOk = true;

            return await PortfolioBusiness.CommitAsync(deleted && profitIsOk);
        }

        public List<TEnum> GetEnumValues<TEnum>() where TEnum : struct
        {
            List<TEnum> enumValuesList = Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToList();

            return enumValuesList;
        }
    }
}
