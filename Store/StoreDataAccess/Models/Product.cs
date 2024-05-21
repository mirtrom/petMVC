using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreDataAccess.Models
{
    public class Product: AbstractModel
    {
        [Required]
        [MaxLength(30)]
        public string Name { get; set; }
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        [MaxLength(5000)]
        public string Description { get; set; }
        [Display(Name = "Image URL")]
		[ValidateNever]
		public string? ImageUrl { get; set; } = string.Empty;
		[ValidateNever]
		public Category Category { get; set; }
	}
}
