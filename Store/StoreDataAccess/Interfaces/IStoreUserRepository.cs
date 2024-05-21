using StoreDataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreDataAccess.Interfaces
{
	public interface IStoreUserRepository : IRepository<StoreUser>
	{
        Task<StoreUser> GetByIdAsync(string id);
    }
}
