namespace App
{
	using System;

	class Product
	{
		public Int32 Id;

		public Int32 Price;

		public String Name;

		public String Discription;
	}

	class EscapeRoom : Product
	{
		public String Theme;

		public Int32 Capacity;

		public Int32 Booked;

		public Boolean IsFull()
		{
			return Capacity > Booked;			
		}
	}

	class Consumable : Product
	{
		public Consumable[] Items;
	}

	class ProductRegister
	{
	}
}