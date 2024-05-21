using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreDataAccess.Models
{
    public class Company: AbstractModel
    {
        [Required]
        public string Name { get; set; }
        [Display(Name = "Street Address")]
        public string? StreetAddress { get; set; }
        public string? City { get; set; }
        public string? Region { get; set; }
		[Display(Name = "Postal code")]
		public string? PostalCode { get; set; }
		[Display(Name = "Phone number")]
		public string? PhoneNumber { get; set; }
        public IEnumerable<StoreUser> Employees { get; set; }
    }
}
