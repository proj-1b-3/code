namespace App
{
	using System;
	using System.Data;

	class Room
	{
		public String Name 
		{ get; set; }

		public String Theme
		{ get; set; }

		public String Discription
		{ get; set; }

		public Int32 Capacity
		{ get; set; }

		public Single Price
		{ get; set; }

		public Boolean Available
		{ get; set; }

		public Room() {}

		public Room(String name, String theme, String discription, Int32 capacity, Single price)
		{
			Name = name;
			Theme = theme;
			Discription = discription;
			Capacity = capacity;
			Price = price;
			Available = true;
		}

		// public void ReadDataRow(DataRow row)
		// {
		// 	Name = (String)row["Name"];
		// 	Theme = (String)row["Theme"];
		// 	Discription = (String)row["Discription"];
		// 	Capacity = (Int32)row["Capacity"];
		// 	Price = (Single)row["Price"];
		// 	Available = (Boolean)row["Available"];
		// }

		public void WriteDataRow(DataRow row)
		{
			row["Name"] = Name;
			row["Theme"] = Theme;
			row["Desc"] = Discription;
			row["Capacity"] = Capacity;
			row["Price"] = Price;
			row["Available"] = Available;
		}
	}
}