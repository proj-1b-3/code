namespace App
{
	using System;
	using System.Collections.Generic;

	class Order
	{
		public Int64 OrderId {get;set;}
		public Int64 UserId {get;set;}
		public Int64 RoomId {get;set;}
		public List<OrderItem> Items {get;set;}
		public DateTime DateTime {get;set;}

		public Order()
		{
			OrderId = -1;
			UserId = -1;
			RoomId = -1;
			Items = null;
			DateTime = new DateTime();
		}

		public Order(Int64 roomId, List<OrderItem> items, DateTime dateTime)
		{
			OrderId = -1;
			UserId = -1;
			RoomId = roomId;
			Items = items;
			DateTime = dateTime;
		}

		public Order(Int64 orderId, Int64 userId, Int64 roomId, List<OrderItem> items, DateTime dateTime)
		{
			OrderId = orderId;
			UserId = userId;
			RoomId = roomId;
			Items = items;
			DateTime = dateTime;
		}
	}
}
