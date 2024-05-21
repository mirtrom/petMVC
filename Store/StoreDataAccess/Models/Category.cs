using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreDataAccess.Models
{
	public class Category : AbstractModel
	{
		[Required]
		[MaxLength(30)]
		[Display(Name = "Category Name")]
		public string Name { get; set; }
		[Display(Name = "Display Order")]
		[Range(0, 1000, ErrorMessage ="Display order must be between 1-100")]
		public int DisplayOrder { get; set; }
		[Display(Name = "Image URL")]
        public string ImageUrl { get; set; } = string.Empty;
        public IEnumerable<Product> Products { get; set; } = new List<Product>();
	}
}
