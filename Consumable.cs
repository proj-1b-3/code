namespace App
{
	using System;
	using System.Data;

	class Consumable : Product
	{
		public Consumable() {}

		public Consumable(String name, String desc, Single price, Boolean available)
			: base(-1, name, desc, price, available)
		{
		}

		public Consumable(DataRow row)
		{
			this.ProductId = (Int64)row["ProductId"];
			this.Name = (String)row["ProductName"];
			this.Description = (String)row["Description"];
			this.Price = (Single)row["Price"];
			this.Available = (Boolean)row["Available"];
		}

		public Consumable Clone()
		{
			var obj = new Consumable();
			obj.ProductId = this.ProductId;
			obj.Name = this.Name;
			obj.Description = this.Description;
			obj.Price = this.Price;
			obj.Available = this.Available;
			return obj;
		}
	}
}
