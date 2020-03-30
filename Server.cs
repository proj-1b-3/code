using System;

namespace App
{
	using System;
	using System.Collections.Generic;
	using System.Text.Json;
	using System.IO;

	using StringToUserData = System.Collections.Generic.Dictionary<String, UserData>;
	using GuidToUserData = System.Collections.Generic.Dictionary<Guid, UserData>;

	class Server
	{
		private StringToUserData Users;
		private GuidToUserData ActiveUsers;
		
		public Server()
		{
			ActiveUsers = new GuidToUserData();
		}

		public void Connect()
		{
			LoadData();
		}

		public void Disconnect()
		{
			SaveData();
		}

		public Boolean TryLogin(String username, String password, out User user)
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

		public Boolean TryLogout(Guid session_token)
		{
			if (! ActiveUsers.ContainsKey(session_token)) {
				return false;
			}

			ActiveUsers.Remove(session_token);

			return true;
		}

		public Boolean TryRegister(String username, String password)
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

		public Boolean TryDeregister(Guid session_token, String password)
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
	
		private void LoadData()
		{
			String file_name = "data/users.json";
			
			if (! File.Exists(file_name)) {
				return;
			}

			ReadOnlySpan<Byte> users_json = File.ReadAllBytes(file_name);

			Users = JsonSerializer.Deserialize<StringToUserData>(users_json);

			return;
		}

		private void SaveData()
		{
			String file_name = "data/users.json";
			Byte[] users_json = JsonSerializer.SerializeToUtf8Bytes(Users);

			if (! File.Exists(file_name)) {
				File.Create(file_name);
			}

			File.WriteAllBytes(file_name, users_json);

			return;
		}
	}
}