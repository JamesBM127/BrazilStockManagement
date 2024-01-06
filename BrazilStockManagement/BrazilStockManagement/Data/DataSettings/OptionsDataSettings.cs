using BrazilStockManagement.Entity;
using Microsoft.EntityFrameworkCore;

namespace BrazilStockManagement.Data.DataSettings
{
    public static class OptionsDataSettings
    {
        public static void OptionsModelBuider(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Options>(model =>
            {
                model.ToTable("Options");

                model.Property(x => x.Strike)
                     .HasColumnName("Strike")
                     .HasColumnOrder(6)
                     .IsRequired();

                model.Property(x => x.StockCode)
                     .HasColumnName("Stock Code")
                     .HasColumnOrder(4)
                     .IsRequired();
            });

            modelBuilder.DerivativeModelBuider<Options>();
        }
    }
}
