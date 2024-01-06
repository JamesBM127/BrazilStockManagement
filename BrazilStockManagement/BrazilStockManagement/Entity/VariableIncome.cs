using BrazilStockManagement.Enums;

namespace BrazilStockManagement.Entity
{
    public class VariableIncome : Investment, ICloneable
    {
        public string Code { get; set; }
        public decimal Total { get; set; }
        public decimal OtherCosts { get; set; }

        public static TEntity ConvertTo<TEntity>(VariableIncome variableIncome) where TEntity : VariableIncome
        {
            VariableIncome? variableToCast = null;

            switch (variableIncome.InvestmentType)
            {
                case InvestmentType.Stock:
                    variableToCast = new Stock();
                    break;

                case InvestmentType.Reit:
                    variableToCast = new Reit();
                    break;

                case InvestmentType.Derivative:
                    variableToCast = new Options();
                    break;
            }

            variableToCast.Id = variableIncome.Id;
            variableToCast.Company = variableIncome.Company;
            variableToCast.Code = variableIncome.Code.ToUpper();
            variableToCast.Amount = variableIncome.Amount;
            variableToCast.Price = variableIncome.Price;
            variableToCast.OtherCosts = variableIncome.OtherCosts;
            variableToCast.Total = variableIncome.Total;
            variableToCast.OperationDate = variableIncome.OperationDate;
            variableToCast.OperationType = variableIncome.OperationType;
            variableToCast.InvestmentType = variableIncome.InvestmentType;
            variableToCast.FinancialInstitution = variableIncome.FinancialInstitution;
            variableToCast.FinancialInstitutionId = variableIncome.FinancialInstitutionId;

            return variableToCast as TEntity;
        }

        public static VariableIncome CloneLegacy(VariableIncome variableIncome)
        {
            VariableIncome? newVariableIncome = new()
            {
                Id = variableIncome.Id,
                Company = variableIncome.Company,
                Code = variableIncome.Code.ToUpper(),
                Amount = variableIncome.Amount,
                Price = variableIncome.Price,
                Total = variableIncome.Total,
                OperationDate = variableIncome.OperationDate,
                OperationType = variableIncome.OperationType,
                InvestmentType = variableIncome.InvestmentType,
                FinancialInstitution = variableIncome.FinancialInstitution,
                FinancialInstitutionId = variableIncome.FinancialInstitutionId
            };

            return newVariableIncome;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
