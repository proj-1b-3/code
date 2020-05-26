namespace App
{
	using System;

	class Review
	{
		public long ReviewId {get;set;}
		public long RoomId {get;set;}
		public DateTime DateTime {get;set;}
		public string Text {get;set;}
		public int Rating {get;set;}

		public string UserName {get;set;}
		public string RoomName {get;set;}

		public Review() {}
	}
}
