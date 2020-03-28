namespace App
{
	using System;
	using System.Collections.Generic;

	enum Role {
		Owner,
		CafeManager,
		Manager,
		Consumer
	}

	class UserData
	{
		public String Name;
		public String Password;
		public Role Perms;

		public UserData (String username, String password) {
			Name = username;
			Password = password;
			Perms = Role.Consumer;
		}
	}
}
