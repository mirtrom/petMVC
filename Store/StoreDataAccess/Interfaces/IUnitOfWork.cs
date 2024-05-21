using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreDataAccess.Interfaces
{
    public interface IUnitOfWork
    {
        IProductRepository ProductRepository { get; set; }
        ICategoryRepository CategoryRepository { get; set; }
        ICompanyRepository CompanyRepository { get; set; }
        IShoppingCardRepository ShoppingCardRepository { get; set; }
        IStoreUserRepository StoreUserRepository { get; set; }
        IOrderHeaderRepository OrderHeaderRepository { get; set; }
        IOrderDetailRepository OrderDetailRepository { get; set; }
        Task SaveAsync();
    }
}
