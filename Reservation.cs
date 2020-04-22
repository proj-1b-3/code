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

		public Reservation() {}

		public Reservation(Room room, DateTime targetDate, Int32 roundNumber, Int32 groupSize)
		{
			this.OrderId = -1;
			this.UserId = -1;
			this.Reservations = room;
			this.GroupSize = groupSize;
			this.TargetDate = targetDateTime.Date;
			this.RoundNumber = roundNumber;
		}
	}
}
