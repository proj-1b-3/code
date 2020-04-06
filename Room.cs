namespace App
{
	using System;
	using System.Data;
	using System.Collections.Generic;

	class Room
	{
		public String Name;
		public String Theme;
		public String Discription;
		public Int32 Capacity;
		public Single Price;
		public Boolean Available = true;

		private Room() {}

		public Room(String name, String theme, String discription, Int32 capacity, Single price)
		{
			Name = name;
			Theme = theme;
			Discription = discription;
			Capacity = capacity;
			Price = price;
		}

		public void FillDataRow(DataRow row)
		{
			row["Name"] = Name;
			row["Theme"] = Theme;
			row["Discription"] = Discription;
			row["Capacity"] = Capacity;
			row["Price"] = Price;
			row["Available"] = Available;
		}
	}
}