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
    public class StoreUserRepository : Repository<StoreUser>, IStoreUserRepository
    {
		public StoreUserRepository(StoreDbContext context) : base(context)
		{
		}

        public async Task<StoreUser> GetByIdAsync(string id)
        {
            var entity = await dbSet.FindAsync(id);
            if (entity == null)
            {
                throw new ArgumentException("Entity not found");
            }
            else
                return entity;
        }
    }
}