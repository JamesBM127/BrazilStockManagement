using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrazilStockManagementTests.SeedEncapsutaion
{
    public class SeedProfit
    {
        public static IEnumerable<Profit> GetProftList(Guid shareHolderId)
        {
            List<Profit> profitList = new List<Profit>();

            Random random = new Random();

            // Loop pelos meses de 2022
            for (int month = 1; month <= 12; month++)
            {
                // Criar 2 objetos Profit para cada mês
                Profit profit1 = new Profit()
                {
                    Id = Guid.NewGuid(),
                    Value = Convert.ToDecimal((random.NextDouble() - 0.5) * 2000), // Valor aleatório entre -1000 e 1000
                    TaxExempt = false,
                    OperationDate = new DateTime(2022, month, 3),
                    OperationType = OperationType.Sell,
                    InvestmentType = InvestmentType.Stock,
                    ShareHolderId = shareHolderId,
                };

                Profit profit2 = new Profit()
                {
                    Id = Guid.NewGuid(),
                    Value = Convert.ToDecimal((random.NextDouble() - 0.5) * 2000), // Valor aleatório entre -1000 e 1000
                    TaxExempt = false,
                    OperationDate = new DateTime(2022, month, 15),
                    OperationType = OperationType.Sell,
                    InvestmentType = InvestmentType.Stock,
                    ShareHolderId = shareHolderId,
                };

                // Adicionar os objetos Profit à lista
                profitList.Add(profit1);
                profitList.Add(profit2);
            }

            return profitList;
        }

    }
}
