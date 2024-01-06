using BrazilStockManagement.Entity;
using Microsoft.EntityFrameworkCore;

namespace BrazilStockManagement.Data.DataSettings
{
    public static class ShareHolderDataSettings
    {
        public static void ShareHolderModelBuider(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ShareHolder>(model =>
            {
                model.ToTable("Share Holder");

                model.HasKey(x => x.Id);

                model.Property(x => x.Id)
                    .HasColumnName("Id")
                    .HasColumnOrder(1)
                    .IsRequired();

                model.Property(x => x.Name)
                    .HasColumnName("Name")
                    .HasColumnOrder(2)
                    .IsRequired();

                model.Property(x => x.CpfCnpj)
                    .HasColumnName("CPF/CNPJ")
                    .HasColumnOrder(3)
                    .IsRequired(false);

                model.HasMany(x => x.FinancialInstitutions)
                    .WithOne(x => x.ShareHolder)
                    .HasForeignKey(x => x.ShareHolderId);

                model.HasMany(x => x.Profits)
                    .WithOne(x => x.ShareHolder)
                    .HasForeignKey(x => x.ShareHolderId);
            });
        }
    }
}
