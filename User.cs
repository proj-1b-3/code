namespace App
{
	using System;

	class User
	{
		public String Name;

		public Guid SessionToken;

		public User (String name, Guid session_token)
		{
			Name = name;
			SessionToken = session_token;
		}
	}
}
