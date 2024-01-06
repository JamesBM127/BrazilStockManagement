namespace BrazilStockManagement.Entity
{
    public abstract class Investment : Operation
    {
        public string Company { get; set; }
        public int Amount { get; set; }
        public decimal Price { get; set; }
        public Guid FinancialInstitutionId { get; set; }
        public FinancialInstitution FinancialInstitution { get; set; }
    }
}
