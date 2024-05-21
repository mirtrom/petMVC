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
	public class Repository<T> : IRepository<T> where T : class
	{
		public readonly DbSet<T> dbSet;

        public Repository(StoreDbContext context)
        {
			dbSet = context.Set<T>();
        }
        public async Task AddAsync(T entity)
		{
			await dbSet.AddAsync(entity);
		}

		public void Delete(T entity)
		{
			dbSet.Remove(entity);
		}

		public async Task DeleteAsync(int id)
		{
			var entity = await GetByIdAsync(id);
			dbSet.Remove(entity);

		}

		public async Task<IEnumerable<T>> GetAllAsync()
		{
			return await dbSet.ToListAsync();
		}

		public async Task<T> GetByIdAsync(int id)
		{
			var entity = await dbSet.FindAsync(id);
			return entity;
		}

		public void Update(T entity)
		{
			dbSet.Update(entity);
		}
	}
}
