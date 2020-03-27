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
		public Role Perms = Role.Consumer;
		public Boolean Active = false;

		public UserData (String username, String password) {
			Name = username;
			Password = password;
		}
	}

	class UserRegister
	{
		public static Dictionary<Int32, UserData> Users =
			new Dictionary<Int32, UserData>();
		
		public static Boolean IsUserActive (Int32 id)
		{
			UserData user;

			if (Users.TryGetValue (id, out user)) {
				return user.Active;
			} else {
				return false;
			}
		}
	}
}
