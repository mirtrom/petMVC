using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreDataAccess.Constants
{
	public class PaymentConstants
	{
		public const string PaymentStatus_Pending = "Pending";
		public const string PaymentStatus_Approved = "Approved";
		public const string PaymentStatus_DelayedPayment = "Approved For Delayed Payment";
		public const string PaymentStatus_Rejected = "Rejected";
	}
}
