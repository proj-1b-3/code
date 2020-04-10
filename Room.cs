namespace App
{
	using System;
	using System.Data;

	class Room
	{
		public Int64 ProductId { get; set; }
		public String Name { get; set; }
		public String Theme { get; set; }
		public String Description { get; set; }
		public Int32 Capacity { get; set; }
		public Single Price { get; set; }
		public Boolean Available { get; set; }

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