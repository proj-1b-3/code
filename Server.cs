using System;

namespace App
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Data;

	enum Role
	{
		Owner,
		CafeManager,
		Manager,
		Consumer
	}

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
			// col.ColumnName = "Role";
			// col.DataType = typeof(Role);
			// col.Unique = true;

			// keys[0] = col;
			// table.Columns.Add(col);

			// table.PrimaryKey = keys;
			// DataBase.Tables.Add(table);
			
			ActiveUsers = new Dictionary<Guid, String>();
		}

		public void LoadData()
		{
			if (!File.Exists("data.xml")) {
				return;
			}

			DataBase.ReadXml("data.xml");
		}

		public void SaveData()
		{
			DataBase.WriteXml("data.xml");
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

		public Boolean TryAddRoom(Guid session_token, Room room)
		{
			String username;
			if (!ActiveUsers.TryGetValue(session_token, out username)) {
				return false;
			}

			var user_row = DataBase.Tables["Users"].Rows.Find(username);
			if (user_row == null || (Role)user_row["Role"] != Role.Owner) {
				return false;
			}

			var room_row = DataBase.Tables["Rooms"].NewRow();
			room_row["RoomName"] = room.RoomName;
			room_row["Theme"] = room.Theme;
			room_row["Discription"] = room.Discription;
			room_row["Capacity"] = room.Capacity;
			room_row["Price"] = room.Price;
			DataBase.Tables["Rooms"].Rows.Add(room_row);

			return true;
		}

		public Boolean TryRemoveRoom(Guid session_token, String roomname)
		{
			String username;
			if (!ActiveUsers.TryGetValue(session_token, out username)) {
				return false;
			}

			var row = DataBase.Tables["Users"].Rows.Find(username);
			if (row == null || (Role)row["Role"] != Role.Owner) {
				return false;
			}

			row = DataBase.Tables["Rooms"].Rows.Find(roomname);
			if (row == null) {
				return false;
			}

			DataBase.Tables["Rooms"].Rows.Remove(row);
			
			return true;
		}
	}
}