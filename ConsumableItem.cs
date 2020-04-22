namespace App
{
	using System;
	using System.Data;

	class ConsumableItem
	{
		public Consumable Consumable {get;set;}
		public Int32 Amount {get;set;}

		public ConsumableItem() {}

		public ConsumableItem(Consumable consumable, Int32 amount)
		{
			this.Consumable = consumable;
			this.Amount = amount;
		}
	}
}
