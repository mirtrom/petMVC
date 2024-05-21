using Microsoft.EntityFrameworkCore;
using StoreDataAccess.Interfaces;
using StoreDataAccess.Models;
using StoreDataAccess.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreDataAccess.Repositories
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
	{
		public OrderHeaderRepository(StoreDbContext context) : base(context)
		{
		}

        public async Task<IEnumerable<OrderHeader>> GetAllWithDetailsAsync()
        {
            return await dbSet
                .Include(d => d.StoreUser)
                .ToListAsync();

        }

        public async Task<OrderHeader> GetByIdWithDetailsAsync(int id)
        {
            return await dbSet
                .Include(d => d.StoreUser)
                .FirstOrDefaultAsync(p => p.Id == id);

        }

        public async Task<IEnumerable<OrderHeader>> GetByUserIdAsync(string userId)
        {
            return await dbSet
                .Include(d => d.StoreUser)
                .Where(p => p.StoreUserId == userId).ToListAsync();
        }

        public void UpdateStatus(int id, string? orderStatus, string? paymentStatus = null)
        {
            var order = dbSet.Find(id);
            if (order == null)
            {
                throw new Exception("Order not found");
            }
            else
            {
                order.OrderStatus = orderStatus;
                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    order.PaymentStatus = paymentStatus;
                }
            }
        }

        public void UpdateStripePaymentId(int id, string sessionId, string paymentIntentId)
        {
            var order = dbSet.Find(id);
            if(!string.IsNullOrEmpty(sessionId))
            {
                order.SessionId = sessionId;
            }
            if(!string.IsNullOrEmpty(paymentIntentId))
            {
                order.PaymentIntendId = paymentIntentId;
                order.PaymentDate = DateTime.Now;
            }
        }
    }
}