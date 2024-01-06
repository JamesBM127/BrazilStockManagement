using BrazilStockManagement.Entity;
using Microsoft.EntityFrameworkCore;

namespace BrazilStockManagement.Data.DataSettings
{
    public static class ReceivedDataSettings
    {
        public static void ReceivedModelBuider<TEntity>(this ModelBuilder modelBuilder) where TEntity : Received
        {
            modelBuilder.Entity<TEntity>(model =>
            {
                model.HasKey(x => x.Id);

                model.Property(x => x.Id)
                    .HasColumnName("Id")
                    .HasColumnOrder(1)
                    .IsRequired();

                model.Property(x => x.Amount)
                    .HasColumnName("Amount")
                    .HasColumnOrder(2)
                    .IsRequired();

                model.Property(x => x.Value)
                    .HasColumnName("Value")
                    .HasColumnOrder(3)
                    .IsRequired();

                model.Property(x => x.RecordDate)
                    .HasColumnName("Record Date")
                    .HasColumnType("datetime2")
                    .HasColumnOrder(5)
                    .IsRequired();

                model.Property(x => x.ExDate)
                    .HasColumnName("Ex Date")
                    .HasColumnType("datetime2")
                    .HasColumnOrder(6)
                    .IsRequired();

                model.Property(x => x.PaymentDate)
                    .HasColumnName("Payment Date")
                    .HasColumnType("datetime2")
                    .HasColumnOrder(7)
                    .IsRequired(false);

                model.Property(x => x.PortfolioId)
                    .HasColumnName("Portfolio Id")
                    .IsRequired();
            });
        }
    }
}
