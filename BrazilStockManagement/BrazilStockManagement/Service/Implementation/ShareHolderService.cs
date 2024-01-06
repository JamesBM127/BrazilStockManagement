using BrazilStockManagement.Business.Implementation;
using BrazilStockManagement.Entity;

namespace BrazilStockManagement.Service.Implementation
{
    public class ShareHolderService
    {
        private readonly ShareHolderBusiness ShareHolderBusiness;

        public ShareHolderService(ShareHolderBusiness shareHolderBusiness)
        {
            ShareHolderBusiness = shareHolderBusiness;
        }

        public async Task<bool> AddAsync(ShareHolder holder)
        {
            bool saved = false;
            try
            {
                saved = await ShareHolderBusiness.AddAsync(holder);
                if (saved)
                {
                    saved = await ShareHolderBusiness.CommitAsync(saved);
                    ShareHolderBusiness.Detach(holder);
                }
            }
            catch (Exception ex)
            {
                //ShareHolderBusiness.Rollback();
            }
            return saved;
        }
    }
}
