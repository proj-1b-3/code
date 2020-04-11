namespace App
{
	using System;
	using System.Data;

	class Room : Product
	{
		public String Theme { get; set; }
		public Int32 Capacity { get; set; }

		public Room() {}

		public Room(String name, String theme, String description, Int32 capacity, Single price)
		{
			Name = name;
			Theme = theme;
			Description = description;
			Capacity = capacity;
			Price = price;
			Available = true;
		}
	}
}