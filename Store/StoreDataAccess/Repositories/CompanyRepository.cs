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
	public class CompanyRepository : Repository<Company>, ICompanyRepository
	{
		public CompanyRepository(StoreDbContext context) : base(context)
		{
		}
	}
}