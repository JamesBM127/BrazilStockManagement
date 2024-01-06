using BrazilStockManagement.Entity;
using Microsoft.EntityFrameworkCore;

namespace BrazilStockManagement.Data.DataSettings
{
    public static class DividendDataSettings
    {
        public static void DividendModelBuider(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Dividend>(model =>
            {
                model.ToTable("Dividend");

                model.Property(x => x.DividendType)
                    .HasColumnName("Dividend Type")
                    .HasColumnType("varchar(9)")
                    .HasColumnOrder(4)
                    .IsRequired();
            });

            modelBuilder.ReceivedModelBuider<Dividend>();
        }
    }
}
