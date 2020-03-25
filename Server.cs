using System;

namespace App
{
	enum Role {
		Owner,
		CafeManager,
		Manager,
		Consumer
	}

	class User
	{
		private Int32 _Id;
		public Int32 Id {
			get { return _Id; }
		}

		private String _Name;
		public String Name {
			get { return _Name; }
		}

		private Byte[] _Token;
		public Byte[] Token {
			get { return _Token; }
		}
	}

	class UserRecord
	{
		public String Name;
		public String Password;
		public Role Perms;
		public Boolean Active;
	}

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

	class Consumable : Product
	{
		public Consumable[] Items;
	}
}
