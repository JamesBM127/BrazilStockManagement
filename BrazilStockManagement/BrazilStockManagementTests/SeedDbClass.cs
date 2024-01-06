using BrazilStockManagement.Enums;
using BrazilStockManagementTests.SeedEncapsutaion;

namespace BrazilStockManagementTests
{
    public static class SeedDbClass
    {
        public static void SeedDb(this ModelBuilder modelBuilder)
        {
            int amount = 100;
            decimal price = 10m;
            DateTime now = DateTime.Now;

            ShareHolder holder1 = new() { Id = Guid.NewGuid(), Name = "Holder 1" };
            ShareHolder holder2 = new() { Id = Guid.NewGuid(), Name = "Holder 2", CpfCnpj = "12345678910" };

            FinancialInstitution FinancialInstitution1 = new()
            {
                Id = Guid.NewGuid(),
                Name = "Bank 1",
                Agency = "1",
                Account = "234",
                Digit = "5",
                ShareHolderId = holder1.Id
            };

            FinancialInstitution FinancialInstitution2 = new()
            {
                Id = Guid.NewGuid(),
                Name = "Bank 2",
                Agency = "1",
                Account = "234",
                Digit = null,
                ShareHolderId = holder1.Id
            };

            FinancialInstitution FinancialInstitution3 = new()
            {
                Id = Guid.NewGuid(),
                Name = "Bank 2",
                Agency = "1",
                Account = "234",
                Digit = string.Empty,
                ShareHolderId = holder2.Id
            };

            FixedDeposit fixedDeposit1 = new()
            {
                Profitability = 102.5m,
                IndexType = IndexType.CDI,
                LiquidityDate = now,
                RecordDate = now,
                DueDate = now.AddYears(2),
                FixedDepositsType = FixedDepositsType.DEB,

                Amount = 2,
                Price = 1234.56m,
                Company = "Company 1",

                OperationDate = now,
                OperationType = OperationType.Buy,

                FinancialInstitutionId = FinancialInstitution1.Id,
                Id = Guid.NewGuid(),
            };

            FixedDeposit fixedDeposit2 = new()
            {
                Profitability = 3.5m,
                IndexType = IndexType.IPCA_Pre,
                LiquidityDate = now,
                RecordDate = now,
                DueDate = now.AddYears(1),
                FixedDepositsType = FixedDepositsType.LCI,

                Amount = amount,
                Price = price,
                Company = "Company 1",

                OperationDate = now,
                OperationType = OperationType.Buy,
                InvestmentType = InvestmentType.FixedDeposit,

                FinancialInstitutionId = FinancialInstitution1.Id,
                Id = Guid.NewGuid()
            };

            Stock stock1 = new()
            {
                Code = "ABCD3",
                Total = amount * price,

                Amount = amount,
                Price = price,
                Company = "ABCD S.A",

                OperationDate = now,
                OperationType = OperationType.Buy,
                InvestmentType = InvestmentType.Stock,

                FinancialInstitutionId = FinancialInstitution1.Id,
                Id = Guid.NewGuid()
            };

            Stock stock2 = new()
            {
                Code = "ABCD3",
                Total = (amount + 50) * (price + 0.15m),

                Amount = amount + 50,
                Price = price + 0.15m,
                Company = "ABCD S.A",

                OperationDate = now,
                OperationType = OperationType.Buy,
                InvestmentType = InvestmentType.Stock,

                FinancialInstitutionId = FinancialInstitution1.Id,
                Id = Guid.NewGuid()
            };

            Stock stock3 = new()
            {
                Code = "ABCD4",
                Total = (amount + 50) * (price + 0.15m),

                Amount = amount + 50,
                Price = price + 0.15m,
                Company = "ABCD S.A",

                OperationDate = now,
                OperationType = OperationType.Buy,
                InvestmentType = InvestmentType.Stock,

                FinancialInstitutionId = FinancialInstitution1.Id,
                Id = Guid.NewGuid()
            };

            Stock stock4 = new()
            {
                Code = "XPTO3",
                Total = amount * price,

                Amount = amount,
                Price = price,
                Company = "XPTO S.A",

                OperationDate = now,
                OperationType = OperationType.Buy,
                InvestmentType = InvestmentType.Stock,

                FinancialInstitutionId = FinancialInstitution1.Id,
                Id = Guid.NewGuid()
            };

            Stock stock5 = new()
            {
                Code = "XPTO3",
                Total = (amount + 50) * (price + 0.15m),

                Amount = amount + 50,
                Price = price + 0.15m,
                Company = "XPTO S.A",

                OperationDate = now,
                OperationType = OperationType.Sell,
                InvestmentType = InvestmentType.Stock,

                FinancialInstitutionId = FinancialInstitution1.Id,
                Id = Guid.NewGuid()
            };

            Reit reit1 = new()
            {
                Code = "FIIA11",
                Total = amount * price,

                Amount = amount,
                Price = price,
                Company = "Fundo de Investimento Imobiliário ABCD",

                OperationDate = now,
                OperationType = OperationType.Buy,
                InvestmentType = InvestmentType.Reit,

                FinancialInstitutionId = FinancialInstitution1.Id,
                Id = Guid.NewGuid()
            };

            Reit reit2 = new()
            {
                Code = "FIIA11",
                Total = (amount + 50) * (price + 0.15m),

                Amount = amount + 50,
                Price = price + 0.15m,
                Company = "Fundo de Investimento Imobiliário ABCD",

                OperationDate = now,
                OperationType = OperationType.Buy,
                InvestmentType = InvestmentType.Reit,

                FinancialInstitutionId = FinancialInstitution1.Id,
                Id = Guid.NewGuid()
            };

            Options options1 = new()
            {
                Strike = 13m,
                Code = "ABCDA111",
                ExerciseDate = new DateTime(2023, 4, 20),
                DerivativeType = DerivativeType.Call,

                StockCode = "ABCD3",
                Total = amount * (price * 0.1m),

                Amount = amount,
                Price = price * 0.1m,
                Company = "ABCD S.A",

                OperationDate = now,
                OperationType = OperationType.Buy,
                InvestmentType = InvestmentType.Derivative,

                FinancialInstitutionId = FinancialInstitution1.Id,
                Id = Guid.NewGuid()
            };

            Options options2 = new()
            {
                Strike = 13m,
                Code = "ABCDA111",
                ExerciseDate = new DateTime(2023, 4, 20),
                DerivativeType = DerivativeType.Put,

                StockCode = "ABCD3",
                Total = amount * (price * 0.1m),

                Amount = amount,
                Price = price * 0.1m,
                Company = "ABCD S.A",

                OperationDate = now,
                OperationType = OperationType.Buy,
                InvestmentType = InvestmentType.Derivative,

                FinancialInstitutionId = FinancialInstitution1.Id,
                Id = Guid.NewGuid()
            };

            Portfolio portfolio1 = new()
            {
                Id = Guid.NewGuid(),
                ShareHolderId = holder1.Id,
                Code = "ABCD3",
                Company = "ABCD SA",
                Quantity = 250,
                Total = 2522.5m,
                InvestmentType = InvestmentType.Stock
            };

            Portfolio portfolio2 = new()
            {
                Id = Guid.NewGuid(),
                ShareHolderId = holder1.Id,
                Code = "ABCD4",
                Company = "ABCD SA",
                Quantity = 150,
                Total = 1522.5m,
                InvestmentType = InvestmentType.Stock
            };

            Portfolio portfolio3 = new()
            {
                Id = Guid.NewGuid(),
                ShareHolderId = holder1.Id,
                Code = "XPTO3",
                Company = "XPTO SA",
                Quantity = 100,
                Total = 1000m,
                InvestmentType = InvestmentType.Stock
            };

            Portfolio portfolio4 = new()
            {
                Id = Guid.NewGuid(),
                ShareHolderId = holder1.Id,
                Code = "FIIA11",
                Company = "Fundo de Investimento Imobiliário A",
                Quantity = 250,
                Total = 2522.5m,
                InvestmentType = InvestmentType.Reit
            };

            Portfolio portfolio5 = new()
            {
                Id = Guid.NewGuid(),
                ShareHolderId = holder2.Id,
                Code = "ABCD3",
                Company = "ABCD SA",
                Quantity = 250,
                Total = 2522.5m,
                InvestmentType = InvestmentType.Stock
            };

            Portfolio portfolio6 = new()
            {
                Id = Guid.NewGuid(),
                ShareHolderId = holder2.Id,
                Code = "FIIA11",
                Company = "Fundo de Investimento Imobiliário A",
                Quantity = 250,
                Total = 2522.5m,
                InvestmentType = InvestmentType.Reit
            };

            Portfolio portfolio7 = new()
            {
                Id = Guid.NewGuid(),
                ShareHolderId = holder1.Id,
                Code = "ABCDA111",
                Company = "ABCD SA",
                Quantity = 250,
                Total = 2522.5m,
                InvestmentType = InvestmentType.Derivative
            };

            Portfolio portfolio8 = new()
            {
                Id = Guid.NewGuid(),
                ShareHolderId = holder1.Id,
                Code = "ABCDN111",
                Company = "ABCD SA",
                Quantity = 250,
                Total = 2522.5m,
                InvestmentType = InvestmentType.Derivative
            };


            modelBuilder.Entity<ShareHolder>().HasData(holder1, holder2);
            modelBuilder.Entity<FinancialInstitution>().HasData(FinancialInstitution1, FinancialInstitution2, FinancialInstitution3);
            modelBuilder.Entity<FixedDeposit>().HasData(fixedDeposit1, fixedDeposit2);
            modelBuilder.Entity<Stock>().HasData(stock1, stock2, stock3, stock4, stock5);
            modelBuilder.Entity<Reit>().HasData(reit1, reit2);
            modelBuilder.Entity<Options>().HasData(options1, options2);
            modelBuilder.Entity<Portfolio>().HasData(portfolio1, portfolio2, portfolio3, portfolio4, portfolio5, portfolio6, portfolio7, portfolio8);
            modelBuilder.Entity<Profit>().HasData(SeedProfit.GetProftList(holder1.Id));
        }
    }
}
