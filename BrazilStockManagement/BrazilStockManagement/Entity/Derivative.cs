using BrazilStockManagement.Enums;

namespace BrazilStockManagement.Entity
{
    public abstract class Derivative : VariableIncome
    {
        public DateTime ExerciseDate { get; set; }
        public DerivativeType DerivativeType { get; set; }
    }
}
