namespace App
{
	class Order
	{
		public Int64 OrderId {get;set;}
		public Int64 UserId {get;set;}
		public Int64 RoomId {get;set;}
		public List<OrderItem> Items {get;set}

		public Order()
		{
			OrderId = -1;
			UserId = -1;
			RoomId = -1;
			Items = null;
		}

		public Order(Int64 roomId, List<OrderItem> items)
		{
			OrderId = -1;
			UserId = -1;
			RoomId = roomId;
			Items = items;
		}

		public Order(Int64 orderId, Int64 userId, Int64 roomId, List<OrderItem> items)
		{
			OrderId = orderId;
			UserId = userId;
			RoomId = roomId;
			Items = items;
		}
	}
}
