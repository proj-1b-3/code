using System;

namespace App
{
	using System;
	using System.Collections.Generic;
	using System.Text.Json;
	using System.IO;
	using System.Data;

	class Server
	{
		private Dictionary<Guid, String> ActiveUsers;

		public DataSet DataBase;
		
		public Server()
		{
			// DataTable table;
			// DataColumn col;
			// DataColumn[] keys;

			DataBase = new DataSet("DataBase");
			DataBase.ReadXmlSchema("data_schema.xml");
			// keys = new DataColumn[1];
			
			// table = new DataTable();
			// table.TableName = "Rooms";

			// col = new DataColumn();
			// col.ColumnName = "RoomName";
			// col.DataType = typeof(String);
			// col.Unique = true;

			// keys[0] = col;
			// table.Columns.Add(col);

			// table.PrimaryKey = keys;
			// DataBase.Tables.Add(table);
			
			ActiveUsers = new Dictionary<Guid, String>();
		}

		public void Start()
		{
			if (!File.Exists("data.xml")) {
				return;
			}

			DataBase.ReadXml("data.xml");
		}

		public void Stop()
		{
			FileStream file;

			if (!File.Exists("data.xml")) {
				file = File.Create("data.xml");
			} else {
				file = File.OpenWrite("data.xml");
			}

			DataBase.WriteXml(file);
			DataBase.WriteXmlSchema("data_schema.xml");
		}

		public Boolean TryLogin(String username, String password, out User user)
		{
			user = null;

			var row = DataBase.Tables["Users"].Rows.Find(username);
			if (row == null || (String)row["Password"] != password) {
				return false;
			}

			Guid session_token = Guid.NewGuid();
			ActiveUsers.Add(session_token, username);
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

			var row = DataBase.Tables["Users"].Rows.Find(username);
			if (row != null) {
				return false;
			}

			row = DataBase.Tables["Users"].NewRow();
			row["UserName"] = username;
			row["Password"] = password;
			DataBase.Tables["Users"].Rows.Add(row);

			return true;
		}

		public Boolean TryDeregister(Guid session_token, String password)
		{
			String username;

			if (password == "") {
				return false;
			}

			if (! ActiveUsers.TryGetValue(session_token, out username)) {
				return false;
			}

			var row = DataBase.Tables["Users"].Rows.Find(username);
			if (row == null || (String)row["Password"] != password) {
				return false;
			}

			ActiveUsers.Remove(session_token);
			DataBase.Tables["Users"].Rows.Remove(row);

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