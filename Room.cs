namespace App
{
	using System;

	class Room : Product
	{
		public String Theme { get; set; }
		public Int32 Capacity { get; set; }

		public Room()
		{
		}

		public Room(String name, String theme, String discription, Int32 capacity, Single price)
		{
			Name = name;
			Theme = theme;
			Capacity = capacity;
			Price = price;
		}
	}
}