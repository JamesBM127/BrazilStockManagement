using JBMDatabase.Repos;

namespace BrazilStockManagementTests.ConfigTests.ConfigServices
{
    public class OptionsServiceConfig : OptionsService
    {
        public OptionsServiceConfig(OptionsBusiness OptionsBusiness = null,
                                    PortfolioBusiness PortfolioBusiness = null,
                                    FinancialInstitutionBusiness FinancialInstitutionBusiness = null,
                                    ProfitBusiness ProfitBusiness = null)
                                    : base(OptionsBusiness, PortfolioBusiness, FinancialInstitutionBusiness, ProfitBusiness)
        {
        }

        public static async Task<Guid> AssertOptions(OperationType OperationType, DerivativeType DerivativeType, VariableIncomeServiceTests VariableIncomeService, IStockManagerRepository Repository)
        {
            char letter1;
            char letter2;
            if (DerivativeType == DerivativeType.Call)
            {
                letter1 = 'A'; letter2 = 'B';
            }
            else
            {
                letter1 = 'N'; letter2 = 'O';
            }


            FinancialInstitution financialInstitution = await TestsSettings.CreateAndGetFinancialInstitutionInDb(Repository);
            Options options1 = TestsSettings.GetOptions(financialInstitution);
            options1.DerivativeType = DerivativeType;
            options1.Code = "ABCD" + letter1 + "111";
            options1.OperationType = OperationType;
            bool addedSell1 = await VariableIncomeService.AddNewVariableIncome<Options>(options1);

            Options options2 = TestsSettings.GetOptions(financialInstitution);
            options2.DerivativeType = DerivativeType;
            options2.Code = "ABCD" + letter2 + "111";
            options2.OperationType = OperationType;
            options2.ExerciseDate = options1.ExerciseDate.AddMonths(1);
            bool addedSell2 = await VariableIncomeService.AddNewVariableIncome<Options>(options2);

            Options options3 = TestsSettings.GetOptions(financialInstitution);
            options3.DerivativeType = DerivativeType;
            options3.Strike = 14m;
            options3.OperationType = OperationType;
            options3.Code = options1.Code.Replace("1", "2"); ;
            bool addedSell3 = await VariableIncomeService.AddNewVariableIncome<Options>(options3);

            Options options4 = TestsSettings.GetOptions(financialInstitution);
            options4.DerivativeType = DerivativeType;
            options4.Strike = 15m;
            options4.OperationType = OperationType;
            options4.Code = options1.Code.Replace("1", "3"); ;
            bool addedSell4 = await VariableIncomeService.AddNewVariableIncome<Options>(options4);

            return financialInstitution.ShareHolderId;
        }
    }
}
