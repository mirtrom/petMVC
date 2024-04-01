using StoreDataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreDataAccess.Interfaces
{
	public interface IRepository<T> where T : AbstractModel
	{
		Task Add(T entity);
		void Update(T entity);
		void Delete(T entity);
		Task Delete(int id);
		Task<T> GetById(int id);
		Task<List<T>> GetAll();
	}
}
