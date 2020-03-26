using System;

namespace App
{
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
}
