using BrazilStockManagement.Entity;
using Microsoft.EntityFrameworkCore;

namespace BrazilStockManagement.Data.DataSettings
{
    public static class ReitDataSettings
    {
        public static void ReitModelBuider(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Reit>(model =>
            {
                model.ToTable("Reit");
            });

            modelBuilder.VariableIncomeModelBuider<Reit>();
        }
    }
}
