using StoreDataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreDataAccess.ViewModels
{
    public class ShoppingCardVM
    {
        public IEnumerable<ShoppingCard> ShoppingCardList { get; set; }
        public OrderHeader OrderHeader { get; set; }
    }
}
