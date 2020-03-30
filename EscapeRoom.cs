namespace App
{
	using System;

	class EscapeRoom
	{
		public String Name { get; set; }
		public String Theme { get; set; }
		public String Discription { get; set; }
		public Int32 Capacity { get; set; }

		public EscapeRoom() {}
		public EscapeRoom(String name, String theme, String discription, Int32 capacity)
		{
			Name = name;
			Theme = theme;
			Discription = discription;
			Capacity = capacity;
		}
	}
}