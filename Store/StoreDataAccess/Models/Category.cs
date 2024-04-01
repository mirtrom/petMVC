using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreDataAccess.Models
{
	public class Category : AbstractModel
	{
		public string Name { get; set; }
		public IEnumerable<Product> Products { get; set; }
	}
}
