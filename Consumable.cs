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

		public Consumable(DataRow prodRow)
		{
			this.ProductId = (Int64)prodRow["ProductId"];
			this.Name = (String)prodRow["ProductName"];
			this.Description = (String)prodRow["Description"];
			this.Price = (Single)prodRow["Price"];
			this.Available = (Boolean)prodRow["Available"];
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
