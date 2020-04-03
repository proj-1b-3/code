namespace App
{
	using System;

	class Room : Product
	{
		public String Theme { get; set; }
		public Int32 Capacity { get; set; }
		public Boolean Available { get; set; }

		public Room()
		{
		}

		public Room(String name, String discription, Single price, String theme, Int32 capacity)
			: base(name, discription, price)
		{
				Theme = theme;
				Capacity = capacity;
				Available = true;
		}
	}
}