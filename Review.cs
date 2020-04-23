namespace App
{
	using System;

	class Review
	{
		public Int64 ReviewId {get;set;}
		public Int64 UserId {get;set;}
		public Int64 RoomId {get;set;}
		public DateTime DateTime {get;set;}
		public String Text {get;set;}

		public Review() {}
	}
}
