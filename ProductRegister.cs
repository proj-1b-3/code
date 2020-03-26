using System;

namespace App
{
		class Product
	{
		private Int32 _Id;
		public Int32 Id {
			get { return _Id; }
		}

		private Int32 _Price;
		public Int32 Price {
			get { return _Price; }
		}

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
	
	class ProductRegister
	{
		
	}
}