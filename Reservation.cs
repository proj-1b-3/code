namespace App
{
	using System;
	using System.Data;
	using System.Collections.Generic;

	class Reservation
	{
		public Int64 RoomId {get;set;}
		public Int32 GroupSize {get;set;}
		public DateTime DateTime {get;set;}
		public Int32 RoundNumber {get;set;}

		public Reservation()
		{
			this.RoomId = -1;
			this.GroupSize = 0;
			this.DateTime = new DateTime();
		}

		public Reservation(DataRow row)
		{
			this.RoomId = (Int64)row["RoomId"];
			this.GroupSize = (Int32)row["GroupSize"];
			this.DateTime = (DateTime)row["Date"];
			this.RoundNumber = (Int32)row["RoundNumber"];
		}

		public Reservation(Int64 roomId, Int32 groupSize, DateTime dateTime, Int32 RoundNumber)
		{
			this.RoomId = roomId;
			this.GroupSize = groupSize;
			this.DateTime = dateTime;
			this.RoundNumber = RoundNumber;
		}
	}
}
