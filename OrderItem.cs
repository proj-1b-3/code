namespace App
{
	using System;

	class OrderItem
	{
		public Int64 ProductId {get; set;}
		public Int32 Amount {get;set;}

		public OrderItem() {}

		public OrderItem(Int64 productId, Int32 amout)
		{
			ProductId = productId;
			Amount = amout;
		}
	}
}
