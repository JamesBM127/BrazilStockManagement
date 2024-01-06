using BrazilStockManagement.Business.Implementation;
using BrazilStockManagement.Entity;
using BrazilStockManagement.Enums;

namespace BrazilStockManagement.Service.Implementation
{
    public class ProfitService
    {
        private readonly ProfitBusiness ProfitBusiness;

        public ProfitService(ProfitBusiness profitBusiness)
        {
            ProfitBusiness = profitBusiness;
        }

        /// <summary>
        /// Get the profit at the end of the operation (swing trade, day trade, and sold operation).
        /// This get just the gross profit, without de taxes and interests.
        /// </summary>
        public static Profit GetOperationProfit<TEntity>(ref TEntity entity, ref Portfolio portfolio) where TEntity : VariableIncome
        {
            decimal averagePrice = 0;
            decimal profitValue = 0;
            TransactionType transactionType = 0;

            try
            {
                averagePrice = portfolio.Total / portfolio.Quantity;
            }
            catch (DivideByZeroException ex)
            {
                averagePrice = 0;
            }

            // Esse if verifica o profit na venda comum, quando já possue o ativo
            if (portfolio.Quantity > 0 && entity.OperationType == OperationType.Sell)
            {
                if (entity.Price == averagePrice)
                    profitValue = 0;

                else
                    profitValue = (entity.Price - averagePrice) * entity.Amount;

                transactionType = TransactionType.Long;
            }

            //Esse if verifica o profit na compra do ativo, no encerramento das operações vendidas
            else if (portfolio.Quantity < 0 && entity.OperationType == OperationType.Buy)
            {
                if (entity.Price == averagePrice)
                    profitValue = 0;

                else
                    profitValue = (averagePrice - entity.Price) * entity.Amount;

                transactionType = TransactionType.Short;
            }

            else if (entity.OperationType == OperationType.Delisting)
            {
                //if(portfolio.InvestmentType == InvestmentType.Derivative)
                //{
                //    entity.de
                //}
                profitValue -= portfolio.Total;
                transactionType = TransactionType.Short;
            }


            if (profitValue != 0)
            {
                return new Profit()
                {
                    Value = profitValue,
                    Code = entity.Code,
                    OperationDate = entity.OperationDate,
                    OperationType = entity.OperationType,
                    TransactionType = transactionType,
                    InvestmentType = entity.InvestmentType,
                    VariableIncomeId = entity.Id,
                    ShareHolderId = entity.FinancialInstitution.ShareHolderId
                };
            }
            else
                return null;
        }

        public async Task<IReadOnlyCollection<Profit>> GetProfits(bool taxExempt = false)
        {
            return await ProfitBusiness.ListAsync<Profit>(x => x.TaxExempt == taxExempt);
        }

        public async Task<IReadOnlyCollection<Profit>> GetProfits(int year, int month, bool taxExempt = false)
        {
            return await ProfitBusiness.ListAsync<Profit>(x => x.TaxExempt == taxExempt &&
                                                          x.OperationDate.Year == year &&
                                                          x.OperationDate.Month == month);
        }
    }
}
