namespace App {
	using System;

	class Product {
		public long ProductId {get;set;}
		public string Name {get;set;}
		public string Description {get;set;}
		public float Price {get;set;}
		public bool Available {get;set;}

		public Product() {}

		public Product(Int64 id, String name, String desc, Single price, Boolean avail) {
			this.ProductId = id;
			this.Name = name;
			this.Description = desc;
			this.Price = price;
			this.Available = avail;
		}
	}
}
