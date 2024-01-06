using BrazilStockManagement.Entity;
using JBMDatabase;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace BrazilStockManagement.Data.DataSettings
{
    public static class OperationDataSettings
    {
        public static void OperationModelBuider<TEntity>(this ModelBuilder modelBuilder) where TEntity : Operation
        {
            modelBuilder.Entity<TEntity>(model =>
            {
                model.Property(x => x.Id)
                     .HasColumnName("Id")
                     .HasColumnOrder(1);

                model.Property(x => x.OperationDate)
                     .HasColumnName("Operation Date")
                     .HasColumnType("datetime2")
                     .HasColumnOrder(97)
                     .IsRequired();

                model.Property(x => x.OperationType)
                     .HasColumnName("Operation Type")
                     .HasColumnType("varchar(9)")
                     .HasColumnOrder(98)
                     .IsRequired();

                model.Property(x => x.InvestmentType)
                     .HasColumnName("Investment Type")
                     .HasColumnType("varchar(15)")
                     .HasColumnOrder(99)
                     .IsRequired();
            });
        }
    }
}
