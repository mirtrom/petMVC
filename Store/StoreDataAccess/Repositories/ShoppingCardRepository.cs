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
    public class ShoppingCardRepository : Repository<ShoppingCard>, IShoppingCardRepository
	{
		public ShoppingCardRepository(StoreDbContext context) : base(context)
		{
		}

        public async Task<IEnumerable<ShoppingCard>> GetShoppingCardListAsync(string userId)
        {
            return await dbSet.Where(s => s.StoreUserId == userId)
                .Include(s => s.Product)
                .ToListAsync();
        }
    }
}