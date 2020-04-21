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
	}
}
