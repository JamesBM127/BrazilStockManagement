using BrazilStockManagement.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrazilStockManagement.Data.DataSettings
{
    public static class PortfolioDataSettings
    {
        public static void PortfolioModelBuilder(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Portfolio>(model =>
            {
                model.ToTable("Portfolio");

                model.Property(x => x.Id)
                     .HasColumnName("Id")
                     .HasColumnOrder(1)
                     .IsRequired();

                model.Property(x => x.Company)
                     .HasColumnName("Company")
                     .HasColumnOrder(2)
                     .IsRequired();

                model.Property(x => x.Code)
                     .HasColumnName("Code")
                     .HasColumnOrder(3)
                     .IsRequired();

                model.Property(x => x.Quantity)
                     .HasColumnName("Quantity")
                     .HasColumnOrder(4)
                     .IsRequired();

                model.Property(x => x.Total)
                     .HasColumnName("Total")
                     .HasColumnOrder(5)
                     .IsRequired();

                model.Property(x => x.InvestmentType)
                     .HasColumnName("Investment Type")
                     .HasColumnType("varchar(15)")
                     .HasColumnOrder(6)
                     .IsRequired();

                model.Property(x => x.ShareHolderId)
                     .HasColumnName("Share Holder Id")
                     .HasColumnOrder(7)
                     .IsRequired();

                model.HasOne(x => x.ShareHolder)
                     .WithMany(x => x.Portfolios)
                     .HasForeignKey(x => x.ShareHolderId);
            });
        }
    }
}
