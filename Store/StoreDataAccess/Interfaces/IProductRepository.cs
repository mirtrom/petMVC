using StoreDataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreDataAccess.Interfaces
{
	public interface IProductRepository: IRepository<Product>
	{
		public Task<IEnumerable<Product>> GetAllWithDetailsAsync();
		public Task<Product> GetByIdWithDetailsAsync(int id);
	}
}
