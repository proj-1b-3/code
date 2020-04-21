namespace App
{
	using System;

	class Consumable : Product
	{
		public Consumable() {}

		public Consumable(String name, String desc, Single price, Boolean available)
			: base(-1, name, desc, price, available)
		{
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
