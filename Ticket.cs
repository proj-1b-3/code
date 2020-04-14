namespace App
{
	using System;
	using System.Collections.Generic;

	class Order
	{
		public Int64 OrderId {get;set;}
		public Int64 UserId {get;set;}
		public List<Reservation> Rooms {get;set;}
		public List<OrderItem> Items {get;set;}
		public DateTime OrderDateTime {get;set;}

		public Order()
		{
			OrderId = -1;
			UserId = -1;
			RoomId = null;
			Items = null;
			OrderDateTime = new DateTime();
		}

		public Order(List<Reservation> rooms, List<OrderItem> items, DateTime dateTime)
		{
			OrderId = -1;
			UserId = -1;
			RoomId = rooms;
			Items = items;
			OrderDateTime = dateTime;
		}

		public Order(Int64 orderId, Int64 userId, List<Reservation> rooms, List<OrderItem> items, 
			DateTime dateTime)
		{
			OrderId = orderId;
			UserId = userId;
			RoomId = rooms;
			Items = items;
			OrderDateTime = dateTime;
		}
	}
}
