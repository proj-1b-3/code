namespace App
{
	using System;

	class Room : Product
	{
		public String Theme { get; set; }
		public Int32 Capacity { get; set; }
		public Single Price { get; set; }
		public Boolean Available { get; set; }

		public Room()
		{
		}

		public Room(String theme, Int32 capacity, Single price, Boolean available)
		{
			Theme = theme;
			Capacity = capacity;
			Price = price;
			Available = available;
		}
	}
}