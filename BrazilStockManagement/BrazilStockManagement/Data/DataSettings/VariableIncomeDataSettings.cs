using BrazilStockManagement.Entity;
using Microsoft.EntityFrameworkCore;

namespace BrazilStockManagement.Data.DataSettings
{
    public static class VariableIncomeDataSettings
    {
        public static void VariableIncomeModelBuider<TEntity>(this ModelBuilder modelBuilder) where TEntity : VariableIncome
        {
            modelBuilder.Entity<TEntity>(model =>
            {
                model.Property(x => x.Code)
                    .HasColumnName("Code")
                    .HasColumnOrder(3)
                    .IsRequired();

                model.Property(x => x.OtherCosts)
                    .HasColumnName("Other Costs")
                    .HasColumnOrder(6)
                    .IsRequired();

                model.Property(x => x.Total)
                    .HasColumnName("Total")
                    .HasColumnOrder(7)
                    .IsRequired();
            });

            modelBuilder.InvestmentModelBuider<TEntity>();
        }
    }
}
