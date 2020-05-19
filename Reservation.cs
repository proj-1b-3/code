namespace App
{
	using System;
	using System.Collections.Generic;
	using System.Data;

	class Reservation
	{
		public Int64 ReservationId {get;set;}
		public Int64 UserId {get;set;}
		public Room Room {get;set;}
		public Int32 GroupSize {get;set;}
		public DateTime OrderDateTime {get;set;}
		public DateTime TargetDateTime {get;set;}
		public Int32 RoundNumber {get;set;}
		public List<ConsumableItem> ConsumableItems {get;set;}

		public Reservation() {}

		public Reservation(Room room, DateTime targetDateTime, Int32 roundNumber, Int32 groupSize)
		{
			this.ReservationId = -1;
			this.UserId = -1;
			this.Room = room;
			this.GroupSize = groupSize;
			this.TargetDateTime = targetDateTime.Date;
			this.RoundNumber = roundNumber;
			this.ConsumableItems = new List<ConsumableItem>();
		}

		public Reservation(DataRow row)
		{
			this.ReservationId = (Int64)row["ReservationId"];
			this.UserId = (Int64)row["UserId"];
			this.Room = null;
			this.GroupSize = (Int32)row["GroupSize"];
			this.TargetDateTime = (DateTime)row["TargetDateTime"];
			this.OrderDateTime = (DateTime)row["OrderDateTime"];
			this.RoundNumber = (Int32)row["RoundNumber"];
			this.ConsumableItems = null;
		}
	}
}
