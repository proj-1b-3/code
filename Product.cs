namespace App
{
	using System;
	using System.Data;
	using System.Collections.Generic;

	class Product
	{
		public String Type;
		public String Name;
		public String Discription;
		public Single Price;
		public Boolean Available;

		public Dictionary<String, String> Tags;

		private Product()
		{
		}

		public Product(String type, String name, String discription, Single price)
		{
			Type = type;
			Name = name;
			Discription = discription;
			Price = price;
			Available = true;
		}

		public Product(String type, String name, String discription, Single price, Boolean available)
			: this(type, name, discription, price)
		{
			Available = available;
		}

		public void FillDataRow(DataRow row)
		{
			row["Type"] = Type;
			row["Name"] = Name;
			row["Discription"] = Discription;
			row["Price"] = Price;
			row["Available"] = Available;
			row["Tags"] = $"{{\"Theme\":\"{Tags["Theme"]}\",\"Capacity\":\"{Tags["Capacity"]}\"}}";
		}
	}

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