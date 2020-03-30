namespace App
{
	using System;

	class User
	{
		private String _Name;
		public String Name { get { return _Name; } }

		public Guid SessionToken;

		public User(String name, Guid session_token)
		{
			_Name = name;
			SessionToken = session_token;
		}
	}
}
