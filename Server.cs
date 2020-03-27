using System;

namespace App
{
	class Server
	{
		public static Boolean TryLogin (
			String username, String password, out User response)
		{
			Int32 id;
			UserData userdata;

			response = null;

			if (username == "" || password == "") {
				return false;
			}

			id = username.GetHashCode();

			if (! UserRegister.Users.TryGetValue (id, out userdata) ||
					userdata.Password != password) {
				return false;
			}

			userdata.Active = true;
			response = new User(id, username);

			return true;
		}

		public static Boolean TryLogout (Int32 id)
		{
			UserData userdata;

			if (! UserRegister.Users.TryGetValue (id, out userdata)) {
				return false;
			}

			userdata.Active = false;

			return true;
		}

		public static Boolean TryRegister (
			String username, String password)
		{
			Int32 id;

			if (username == "" || password == "") {
				return false;
			}

			id = username.GetHashCode();

			if (UserRegister.Users.ContainsKey(id)) {
				return false;
			}

			UserRegister.Users.Add(id, new UserData(username, password));

			return true;
		}

		public static Boolean TryDeregister (Int32 id, String password)
		{
			UserData userdata;

			if (! UserRegister.Users.TryGetValue(id, out userdata) ||
					userdata.Password != password) {
				return false;
			}

			UserRegister.Users.Remove(id);

			return true;
		}
	}
}