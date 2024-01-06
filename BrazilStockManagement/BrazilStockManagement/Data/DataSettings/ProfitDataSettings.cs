using BrazilStockManagement.Entity;
using Microsoft.EntityFrameworkCore;

namespace BrazilStockManagement.Data.DataSettings
{
    public static class ProfitDataSettings
    {
        public static void ProfitModelBuider(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Profit>(model =>
            {
                model.ToTable("Profit");

                model.Property(x => x.Value)
                     .HasColumnName("Value")
                     .HasColumnOrder(2)
                     .IsRequired();

                model.Property(x => x.TaxExempt)
                     .HasColumnName("Tax Exempt")
                     .HasColumnType("varchar(5)")
                     .HasColumnOrder(3)
                     .IsRequired();

                model.Property(x => x.TaxValue)
                     .HasColumnName("Tax Value")
                     .HasColumnOrder(4)
                     .IsRequired();

                model.Property(x => x.ProfitType)
                     .HasColumnName("Profit Type")
                     .HasColumnType("varchar(15)")
                     .HasColumnOrder(5)
                     .IsRequired();

                model.Property(x => x.TransactionType)
                     .HasColumnName("Transaction Type")
                     .HasColumnType("varchar(5)")
                     .HasColumnOrder(6)
                     .IsRequired();

                model.Property(x => x.Code)
                     .HasColumnName("Code")
                     .HasColumnType("varchar(15)")
                     .HasColumnOrder(7)
                     .IsRequired();

                model.Property(x => x.VariableIncomeId)
                     .HasColumnName("Variable Income Id")
                     .IsRequired();

                model.Property(x => x.ShareHolderId)
                     .HasColumnName("Share Holder Id")
                     .IsRequired();

                model.HasOne(x => x.ShareHolder)
                     .WithMany(x => x.Profits);
            });

            modelBuilder.OperationModelBuider<Profit>();
        }
    }
}
