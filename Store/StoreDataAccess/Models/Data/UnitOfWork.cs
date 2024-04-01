using StoreDataAccess.Interfaces;
using StoreDataAccess.Repositories;

namespace StoreDataAccess.Models.Data
{
	public class UnitOfWork: IUnitOfWork
	{
		private readonly StoreDbContext _context;
		public IProductRepository ProductRepository {get; set; }

		public ICategoryRepository CategoryRepository { get; set; }

		public UnitOfWork(StoreDbContext context)
		{
			_context = context;
			ProductRepository = new ProductRepository(_context);
			CategoryRepository = new CategoryRepository(_context);
		}
		public void Save()
		{
			_context.SaveChanges();
		}
	}
}
