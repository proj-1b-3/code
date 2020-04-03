namespace App
{
	using System;
	using System.Collections.Generic;

	class Product
	{
		public String Type;
		public String Name;
		public String Discription;
		public Single Price;
		public Dictionary<String, String> Tags;

		public Product()
		{
		}

		public Product(String name, String discription, Single price)
		{
			Name = name;
			Discription = discription;
			Price = price;
		}
	}
}