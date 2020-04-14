namespace App
{
	using System;
	using System.Collections.Generic;

	class Reservation
	{
		public Int64 RoomId {get;set;}
		public Int32 GroupSize {get;set;}
		public DateTime DateTime {get;set;}

		public Reservation()
		{
			RoomId = -1;
			GroupSize = 0;
			DateTime = new DateTime();
		}

		public Reservation(Int64 roomId, Int32 groupSize, DateTime dateTime)
		{
			RoomId = roomId;
			GroupSize = groupSize;
			DateTime = dateTime;
		}
	}
}
