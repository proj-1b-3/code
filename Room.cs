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

		public Room() {}
		public Room(String name, String theme, String discription, Int32 capacity)
		{
			Name = name;
			Theme = theme;
			Discription = discription;
			Capacity = capacity;
		}

		override public String ToString()
		{
			return "Name: " + Name.ToString()
				+ "\nTheme: " + Theme.ToString()
				+ "\nDiscription: " + Discription.ToString()
				+ "\nCapacity: " + Capacity.ToString()
				+ "\nPrice: " + Price.ToString();
		}
	}
}