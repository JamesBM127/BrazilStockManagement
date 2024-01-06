using BrazilStockManagement.Entity;
using Microsoft.EntityFrameworkCore;

namespace BrazilStockManagement.Data.DataSettings
{
    public static class FixedDepositDataSettings
    {
        public static void FixedDepositModelBuider(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FixedDeposit>(model =>
            {
                model.ToTable("Fixed Deposit");

                model.Property(x => x.FixedDepositsType)
                    .HasColumnName("Fixed Deposits Type")
                    .HasColumnType("varchar(3)")
                    .HasColumnOrder(3)
                    .IsRequired();

                model.Property(x => x.Profitability)
                    .HasColumnName("Profitability")
                    .HasColumnOrder(6)
                    .IsRequired();

                model.Property(x => x.IndexType)
                    .HasColumnName("Index Type")
                    .HasColumnType("varchar(15)")
                    .HasColumnOrder(7)
                    .IsRequired();

                model.Property(x => x.LiquidityDate)
                    .HasColumnName("Liquidity Date")
                    .HasColumnOrder(8)
                    .IsRequired(false);

                model.Property(x => x.RecordDate)
                    .HasColumnName("Record Date")
                    .HasColumnOrder(9)
                    .IsRequired(false);

                model.Property(x => x.DueDate)
                    .HasColumnName("Due Date")
                    .HasColumnOrder(10)
                    .IsRequired();
            });

            modelBuilder.InvestmentModelBuider<FixedDeposit>();
        }
    }
}
