using StoreDataAccess.Interfaces;
using StoreDataAccess.Repositories;

namespace StoreDataAccess.Models.Data
{
	public class UnitOfWork: IUnitOfWork
	{
		private readonly StoreDbContext _context;
		public IProductRepository ProductRepository {get; set; }

		public ICategoryRepository CategoryRepository { get; set; }
		public ICompanyRepository CompanyRepository { get; set; }
        public IShoppingCardRepository ShoppingCardRepository { get; set; }
        public IStoreUserRepository StoreUserRepository { get; set; }
		public IOrderHeaderRepository OrderHeaderRepository { get; set; }
		public IOrderDetailRepository OrderDetailRepository { get; set; }

        public UnitOfWork(StoreDbContext context)
		{
			_context = context;
			StoreUserRepository = new StoreUserRepository(_context);
			ProductRepository = new ProductRepository(_context);
			CategoryRepository = new CategoryRepository(_context);
			CompanyRepository = new CompanyRepository(_context);
			ShoppingCardRepository = new ShoppingCardRepository(_context);
			OrderHeaderRepository = new OrderHeaderRepository(_context);
			OrderDetailRepository = new OrderDetailRepository(_context);
		}
		public async Task SaveAsync()
		{
			await _context.SaveChangesAsync();
		}
	}
}
