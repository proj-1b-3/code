namespace App
{
	using System;
	using System.Data;

	class Room : Product
	{
		public String Theme {get;set;}
		public Int32 Capacity {get;set;}
		public Int32 NumberOfRounds {get;set;}
		public Int32 MaxDuration {get;set;}

		public Room() {}

		public Room(String name, String theme, String desc, Int32 cap, Single price,
			Int32 numberOfRounds, Int32 maxDuration)
		{
			Name = name;
			Theme = theme;
			Description = desc;
			Capacity = cap;
			Price = price;
			NumberOfRounds = numberOfRounds;
			MaxDuration = maxDuration;
			Available = true;
		}
	}
}
