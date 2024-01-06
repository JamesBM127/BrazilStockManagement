using BrazilStockManagement.Entity;
using Microsoft.EntityFrameworkCore;

namespace BrazilStockManagement.Data.DataSettings
{
    public static class DerivativeDataSettings
    {
        public static void DerivativeModelBuider<TEntity>(this ModelBuilder modelBuilder) where TEntity : Derivative
        {
            modelBuilder.Entity<TEntity>(model =>
            {
                model.Property(x => x.ExerciseDate)
                     .HasColumnName("Exercise Date")
                     .HasColumnType("datetime2")
                     .HasColumnOrder(7)
                     .IsRequired();

                model.Property(x => x.DerivativeType)
                     .HasColumnName("Derivative Type")
                     .HasColumnOrder(8)
                     .HasColumnType("varchar(25)")
                     .IsRequired();
            });

            modelBuilder.VariableIncomeModelBuider<TEntity>();
        }
    }
}
