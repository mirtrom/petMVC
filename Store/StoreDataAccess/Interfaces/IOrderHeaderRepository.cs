using StoreDataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreDataAccess.Interfaces
{
	public interface IOrderHeaderRepository : IRepository<OrderHeader>
	{
		void UpdateStatus(int id, string? orderStatus, string?  paymentStatus = null);
		void UpdateStripePaymentId(int id, string sessionId, string paymentIntentId);
		Task<IEnumerable<OrderHeader>> GetAllWithDetailsAsync();
        Task<OrderHeader> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<OrderHeader>> GetByUserIdAsync(string userId);
    }
}
