namespace App
{
	using System;
	using System.Data;

	class OrderItem
	{
		public Int64 ProductId {get; set;}
		public Int32 Amount {get;set;}

		public OrderItem() {}

		public OrderItem(Int64 productId, Int32 amount)
		{
			ProductId = productId;
			Amount = amount;
		}
		
		public OrderItem(DataRow row)
		{
			this.ProductId = (Int64)row["ProductId"];
			this.Amount = (Int32)row["Amount"];
		}
	}
}
