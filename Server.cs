using System;

namespace App
{
	using System;
	using System.Collections.Generic;
	using System.Text.Json;
	using System.IO;

	class Server
	{
		private Dictionary<String, UserData> Users;
		public Dictionary<String, EscapeRoom> Rooms;

		private Dictionary<Guid, UserData> ActiveUsers;
		
		public Server()
		{
			ActiveUsers = new Dictionary<Guid, UserData>();
		}

		public void Start()
		{
			LoadData<Dictionary<String, UserData>>("data/users.json", out Users);
			LoadData<Dictionary<String, EscapeRoom>>("data/rooms.json", out Rooms);
		}

		public void Stop()
		{
			SaveData<Dictionary<String, UserData>>("data/users.json", Users);
			SaveData<Dictionary<String, EscapeRoom>>("data/rooms.json", Rooms);
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
	
		private void LoadData<T>(String file_name, out T obj)
			where T : new()
		{
			if (! File.Exists(file_name)) {
				obj = new T();
				return;
			}

			obj = JsonSerializer.Deserialize<T>(File.ReadAllBytes(file_name));

			return;
		}

		private void SaveData<T>(String file_name, in T obj)
		{
			if (! File.Exists(file_name)) {
				File.Create(file_name);
			}

			File.WriteAllBytes(file_name, JsonSerializer.SerializeToUtf8Bytes<T>(obj));

			return;
		}
	}
}