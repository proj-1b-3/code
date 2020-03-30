namespace App
{
	using System;
	using System.Collections.Generic;

	enum Role
	{
		Owner,
		CafeManager,
		Manager,
		Consumer
	}

	class Token
	{
	}

	class UserData
	{
		public String Name { get; set; }
		public String Password { get; set; }
		public Role Perms { get; set; }

		public UserData()
		{
		}

		public UserData(String username, String password)
		{
			Name = username;
			Password = password;
			Perms = Role.Consumer;
		}
	}
}
