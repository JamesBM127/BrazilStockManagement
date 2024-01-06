using BrazilStockManagement.Entity;
using BrazilStockManagement.Enums;
using System.Security.Principal;

namespace BrazilStockManagement.Service.Implementation
{
    public static class PortfolioService
    {
        public static Portfolio UpdatePortfolio<TEntity>(this Portfolio portfolio, TEntity entity) where TEntity : VariableIncome
        {
            if (entity.OperationType == OperationType.Buy)
            {
                //This is the buy of an inexistent portfolio
                if (portfolio == null)
                {
                    portfolio = new()
                    {
                        Code = entity.Code,
                        Company = entity.Company,
                        Quantity = entity.Amount,
                        Total = entity.Total,
                        InvestmentType = entity.InvestmentType,
                        ShareHolderId = entity.FinancialInstitution.ShareHolderId
                    };
                }

                //Not the first buy
                else
                {
                    //If quantity is smaller than 0, means that the holder sell first to buy later
                    if (portfolio.Quantity < 0)
                    {
                        decimal averagePrice = 0;
                        try
                        {
                            averagePrice = portfolio.Total / portfolio.Quantity;
                        }
                        catch (DivideByZeroException ex)
                        {
                            averagePrice = 0;
                        }

                        int portfolioQuantity = portfolio.Quantity;
                        portfolio.Quantity += entity.Amount;
                        portfolio.Total = portfolio.Quantity * averagePrice;

                        //If you buy more than your seller debt
                        if (entity.Amount >= Math.Abs(portfolioQuantity))
                            portfolio.Total = (entity.Amount + portfolioQuantity) * entity.Price;
                    }

                    //If is bigger than 0, and the is just buying more
                    else
                    {
                        portfolio.Quantity += entity.Amount;
                        portfolio.Total += entity.Total;
                    }
                }
            }
            else
            {
                //This is the sell of an inexistent portfolio
                if (portfolio == null)
                {
                    portfolio = new()
                    {
                        Code = entity.Code,
                        Company = entity.Company,
                        Quantity = (entity.Amount) * -1,
                        Total = (entity.Total) * -1,
                        InvestmentType = entity.InvestmentType,
                        ShareHolderId = entity.FinancialInstitution.ShareHolderId
                    };
                }

                //Not the first sell
                else
                {
                    decimal averagePrice = 0;
                    try
                    {
                        averagePrice = portfolio.Total / portfolio.Quantity;
                    }
                    catch (DivideByZeroException ex)
                    {
                        averagePrice = 0;
                    }

                    bool soldMoreThanHadBefore = false;
                    int tempQuantity = portfolio.Quantity;

                    portfolio.Quantity -= entity.Amount;

                    if(tempQuantity > 0 && portfolio.Quantity < 0)
                        soldMoreThanHadBefore = true;

                    if (portfolio.Quantity < 0 && soldMoreThanHadBefore)
                        portfolio.Total = portfolio.Quantity * entity.Price;
                    
                    else if(portfolio.Quantity < 0 && !soldMoreThanHadBefore)
                        portfolio.Total -= entity.Total;

                    else
                        portfolio.Total = portfolio.Quantity * averagePrice;
                }
            }

            return portfolio;
        }

        public static void EditPortfolio<TEntity>(this Portfolio portfolio, TEntity oldEntity, TEntity newEntity) where TEntity : VariableIncome
        {
            portfolio.DeleteVariableIncomeFromPortfolio(oldEntity);
            switch (newEntity.OperationType)
            {
                case OperationType.Buy:
                    portfolio.Quantity += newEntity.Amount;
                    portfolio.Total += newEntity.Total;
                    break;

                case OperationType.Sell:
                    decimal averagePrice = 0;
                    try
                    {
                        averagePrice = portfolio.Total / portfolio.Quantity;
                    }
                    catch (DivideByZeroException ex)
                    {
                        averagePrice = 0;
                    }
                    portfolio.Quantity -= newEntity.Amount;
                    portfolio.Total = averagePrice * portfolio.Quantity;
                    break;
            }

            //Disable the option to change stock order to reit order,
            //to change, please delete the order and create a brand new order.
            //portfolio.InvestmentType = newEntity.InvestmentType;
        }

        public static void DeleteVariableIncomeFromPortfolio<TEntity>(this Portfolio portfolio, TEntity entity) where TEntity : VariableIncome
        {
            switch (entity.OperationType)
            {
                case OperationType.Buy:
                    portfolio.Quantity -= entity.Amount;
                    portfolio.Total -= entity.Total;
                    break;

                case OperationType.Sell:
                    decimal averagePrice = 0;
                    try
                    {
                        averagePrice = portfolio.Total / portfolio.Quantity;
                    }
                    catch (DivideByZeroException ex)
                    {
                        averagePrice = 0;
                    }
                    portfolio.Quantity += entity.Amount;
                    portfolio.Total = averagePrice * portfolio.Quantity;
                    break;
            }
        }

        public static void AddBonusShareToPortfolio(this Portfolio portfolio, BonusShare bonusShare)
        {
            portfolio.Total += bonusShare.Value * bonusShare.Amount;
            portfolio.Quantity += bonusShare.Amount;
        }

        public static void RoolbackUpdateBonusShare(this Portfolio portfolio, BonusShare oldBonusShare)
        {
            portfolio.Total -= oldBonusShare.Value * oldBonusShare.Amount;
            portfolio.Quantity -= oldBonusShare.Amount;
        }
    }
}
