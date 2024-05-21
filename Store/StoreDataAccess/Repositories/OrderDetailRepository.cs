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
	public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
	{
		public OrderDetailRepository(StoreDbContext context) : base(context)
		{
		}

		public async Task<IEnumerable<OrderDetail>> GetAllByOrderHeaderAsync(int id)
		{
			return await dbSet
				.Include(d => d.Product)
				.Where(o => o.OrderHeaderId == id).ToListAsync();
		}
	}
}