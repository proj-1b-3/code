namespace App
{
	using System;
	using System.Collections.Generic;

	class Reservation
	{
		public Int64 ReservationId {get;set;}
		public Int64 UserId {get;set;}
		public Room Room {get;set;}
		public Int32 GroupSize {get;set;}
		public List<Consumable> Consumables {get;set;}
		public DateTime OrderDateTime {get;set;}
		public DateTime TargetDateTime {get;set;}
		public DateTime RoundNumber {get;set;}

		public Order() {}

		public Order(Room room, List<Consumable> consumables, DateTime targetDate, Int32 roundNumber)
		{
			this.OrderId = -1;
			this.UserId = -1;
			this.Reservations = room;
			this.Consumables = consumable;
			this.TargetDate = targetDateTime.Date;
			this.RoundNumber = roundNumber;
		}
	}
}
