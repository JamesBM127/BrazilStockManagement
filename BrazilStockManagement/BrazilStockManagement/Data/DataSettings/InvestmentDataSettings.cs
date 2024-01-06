using BrazilStockManagement.Entity;
using Microsoft.EntityFrameworkCore;

namespace BrazilStockManagement.Data.DataSettings
{
    public static class InvestmentDataSettings
    {
        public static void InvestmentModelBuider<TEntity>(this ModelBuilder modelBuilder) where TEntity : Investment
        {
            modelBuilder.Entity<TEntity>(model =>
            {
                model.Property(x => x.Company)
                    .HasColumnName("Company")
                    .HasColumnOrder(2)
                    .IsRequired();

                model.Property(x => x.Amount)
                    .HasColumnName("Amount")
                    .HasColumnOrder(4)
                    .IsRequired();

                model.Property(x => x.Price)
                    .HasColumnName("Price")
                    .HasColumnOrder(5)
                    .IsRequired();
                
                model.Property(x => x.FinancialInstitutionId)
                    .HasColumnName("Financial Institution Id")
                    .IsRequired();
            });

            modelBuilder.OperationModelBuider<TEntity>();
        }
    }
}
