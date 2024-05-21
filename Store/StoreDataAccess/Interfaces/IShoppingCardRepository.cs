using StoreDataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreDataAccess.Interfaces
{
	public interface IShoppingCardRepository : IRepository<ShoppingCard>
	{
		Task<IEnumerable<ShoppingCard>> GetShoppingCardListAsync(string userId);
	}
}
