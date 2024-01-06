using BrazilStockManagement.Entity;
using Microsoft.EntityFrameworkCore;

namespace BrazilStockManagement.Data.DataSettings
{
    public static class StockDataSettings
    {
        public static void StockModelBuider(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Stock>(model =>
            {
                model.ToTable("Stock");
            });

            modelBuilder.VariableIncomeModelBuider<Stock>();
        }
    }
}
