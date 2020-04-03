namespace App
{
	using System;

	class Room
	{
		public String Name { get; set; }
		public String Theme { get; set; }
		public String Discription { get; set; }
		public Int32 Capacity { get; set; }
		public Single Price { get; set; }
		public Boolean Available { get; set; }

		public Room()
		{
		}

		public Room(String name, String theme, String discription, Int32 capacity, Single price, Boolean available)
		{
			Name = name;
			Theme = theme;
			Discription = discription;
			Capacity = capacity;
			Price = price;
			Available = available;
		}
	}
}