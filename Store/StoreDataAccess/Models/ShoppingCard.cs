using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreDataAccess.Models
{
    public class ShoppingCard: AbstractModel
    {
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; }
        [Range(1, 1000, ErrorMessage = "Please enter a value between 1 and 1000")]
        public int Quantity { get; set; }
        public string StoreUserId { get; set; }
        [ForeignKey("StoreUserId")]
        [ValidateNever]
        public StoreUser StoreUser { get; set; }
    }
}
