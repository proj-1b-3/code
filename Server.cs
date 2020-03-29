using System;

namespace App
{
	using System;
	using System.Collections.Generic;

	class Server
	{
		private static Dictionary<String, UserData> Users =
			new Dictionary<string, UserData>();
		
		private static Dictionary<Guid, UserData> ActiveUsers =
			new Dictionary<Guid, UserData>();
		
		public static Boolean TryLogin(String username, String password,
			out User user)
		{
			UserData userdata;

			user = null;

			if (username == "" || password == "") {
				return false;
			}

			if (! Users.TryGetValue (username, out userdata) ||
					userdata.Password != password) {
				return false;
			}

			Guid session_token = Guid.NewGuid();
			ActiveUsers.Add(session_token, userdata);
			user = new User(username, session_token);

			return true;
		}

		public static Boolean TryLogout(Guid session_token)
		{
			if (! ActiveUsers.ContainsKey(session_token)) {
				return false;
			}

			ActiveUsers.Remove(session_token);

			return true;
		}

		public static Boolean TryRegister(String username, String password)
		{
			if (username == "" || password == "") {
				return false;
			}

			if (Users.ContainsKey(username)) {
				return false;
			}

			Users.Add(username, new UserData(username, password));

			return true;
		}

		public static Boolean TryDeregister(Guid session_token, String password)
		{
			UserData userdata;

			if (password == "") {
				return false;
			}

			if (! ActiveUsers.TryGetValue(session_token, out userdata) ||
					userdata.Password != password) {
				return false;
			}

			ActiveUsers.Remove(session_token);
			Users.Remove(userdata.Name);

			return true;
		}
	}
}