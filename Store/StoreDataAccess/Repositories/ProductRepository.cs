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
	public class ProductRepository : Repository<Product>, IProductRepository
	{
		public ProductRepository(StoreDbContext context): base(context)
		{
		}
		public async Task<IEnumerable<Product>> GetAllWithDetailsAsync() {
			return await dbSet.
				Include(p => p.Category)
				.ToListAsync();
		}

		public async Task<Product> GetByIdWithDetailsAsync(int id)
		{
			return await dbSet
				.Include(p => p.Category)
				.FirstOrDefaultAsync(p => p.Id == id);
		}
	}
}
