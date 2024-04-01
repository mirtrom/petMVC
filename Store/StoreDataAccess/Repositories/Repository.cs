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
	public class Repository<T> : IRepository<T> where T : AbstractModel
	{
		private readonly DbSet<T> dbSet;

        public Repository(StoreDbContext context)
        {
			dbSet = context.Set<T>();
        }
        public async Task Add(T entity)
		{
			await dbSet.AddAsync(entity);
		}

		public void Delete(T entity)
		{
			dbSet.Remove(entity);
		}

		public async Task Delete(int id)
		{
			var entity = await GetById(id);
			dbSet.Remove(entity);

		}

		public async Task<List<T>> GetAll()
		{
			return await dbSet.ToListAsync();
		}

		public async Task<T> GetById(int id)
		{
			var entity = await dbSet.FindAsync(id);
			if (entity == null)
			{
				throw new ArgumentException("Entity not found");
			}
			else 
				return entity;
		}

		public void Update(T entity)
		{
			dbSet.Update(entity);
		}
	}
}
