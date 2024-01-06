using BrazilStockManagement.Entity;
using Microsoft.EntityFrameworkCore;

namespace BrazilStockManagement.Data.DataSettings
{
    public static class FinancialInstitutionDataSettings
    {
        public static void FinancialInstitutionModelBuider(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FinancialInstitution>(model =>
            {
                model.ToTable("Financial Institution");

                model.HasKey(x => x.Id);

                model.Property(x => x.Id)
                     .HasColumnName("Id")
                     .HasColumnOrder(1)
                     .IsRequired();

                model.Property(x => x.Name)
                     .HasColumnName("Name")
                     .HasColumnOrder(2)
                     .IsRequired();

                model.Property(x => x.Agency)
                     .HasColumnName("Agency")
                     .HasColumnOrder(3)
                     .IsRequired(false);

                model.Property(x => x.Account)
                     .HasColumnName("Account")
                     .HasColumnOrder(4)
                     .IsRequired(false);

                model.Property(x => x.Digit)
                     .HasColumnName("Digit")
                     .HasColumnOrder(5)
                     .IsRequired(false);

                model.Property(x => x.ShareHolderId)
                     .HasColumnName("Share Holder Id")
                     .HasColumnOrder(6)
                     .IsRequired();

                model.HasOne(x => x.ShareHolder)
                     .WithMany(x => x.FinancialInstitutions);
            });
        }
    }
}
