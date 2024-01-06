using BrazilStockManagement.Entity;
using Microsoft.EntityFrameworkCore;

namespace BrazilStockManagement.Data.DataSettings
{
    public static class BonusShareDataSettings
    {
        public static void BonusShareModelBuider(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BonusShare>(model =>
            {
                model.ToTable("Bonus Share");

                model.Property(x => x.Proportion)
                    .HasColumnName("Proportion")
                    .HasColumnOrder(4)
                    .IsRequired();
            });

            modelBuilder.ReceivedModelBuider<BonusShare>();
        }
    }
}
