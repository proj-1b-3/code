namespace App
{
	using System;

	class Product
	{
		public Int64 ProductId {get;set;}
		public String Name {get;set;}
		public String Description {get;set;}
		public Single Price {get;set;}
		public Boolean Available {get;set;}

		public Product() {}

		public Product(Int64 id, String name, String desc, Single price, Boolean avail)
		{
			ProductId = id;
			Name = name;
			Description = desc;
			Price = price;
			Available = avail;
		}
	}
}