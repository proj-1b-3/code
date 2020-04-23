namespace App
{
	using System;

	class Review
	{
		public Int64 ReviewId {get;set;}
		public Int64 RoomId {get;set;}
		public DateTime DateTime {get;set;}
		public String Text {get;set;}
		public Int32 Rating {get;set;}

		public String UserName {get;set;}
		public String RoomName {get;set;}

		public Review() {}
	}
}
