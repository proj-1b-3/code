namespace App
{
	using System;
	using System.Collections.Generic;

	class Product
	{
		public String Name;
		public String Type;
		public Single Price;
		public Dictionary<String, String> Tags;

		public Product()
		{
		}

		public Product(String name, String discription, Single price)
		{
			Name = name;
			Price = price;
		}
	}
}