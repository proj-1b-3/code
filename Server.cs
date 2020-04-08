using System;

namespace App
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Data;
	using System.Linq;

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

		private DataSet DataBase;
		
		public Server()
		{
			DataBase = new DataSet("DataBase");
			DataBase.ReadXmlSchema("Data/ServerSchema.xml");

			// DataTable table;
			// DataColumn col;
			// DataColumn[] keys;
			// DataRelation rel;

			// table = new DataTable("Products");

			// DataBase.Tables.Remove("Rooms");
			// var table = new DataTable("Rooms");
			// var keys = new DataColumn[1];
			// var col = new DataColumn("Id");
			// col.DataType = typeof(Int64);
			// col.AutoIncrement = true;
			// table.Columns.Add(col);
			// keys[0] = col;
			// col = new DataColumn("Name");
			// col.DataType = typeof(String);
			// table.Columns.Add(col);
			// col = new DataColumn("Theme");
			// col.DataType = typeof(String);
			// table.Columns.Add(col);
			// col = new DataColumn("Desc");
			// col.DataType = typeof(String);
			// table.Columns.Add(col);
			// col = new DataColumn("Capacity");
			// col.DataType = typeof(Int32);
			// table.Columns.Add(col);
			// col = new DataColumn("Price");
			// col.DataType = typeof(Single);
			// table.Columns.Add(col);
			// col = new DataColumn("Available");
			// col.DataType = typeof(Boolean);
			// table.Columns.Add(col);
			// table.PrimaryKey = keys;
			// DataBase.Tables.Add(table);

			ActiveUsers = new Dictionary<Guid, String>();
		}

		public void LoadData()
		{
			if (!File.Exists("Data/Data.xml")) {
				return;
			}

			DataBase.ReadXml("Data/Data.xml");
		}

		public void SaveData()
		{
			DataBase.WriteXml("Data/Data.xml");
			// DataBase.WriteXmlSchema("Data/ServerSchema.xml");
		}

		private DataRow GetUserRecord(String username)
		{
			return DataBase.Tables["Users"].Rows.Find(username);
		}

		private DataRow GetUserRecord(Guid session_token)
		{
			String username;

			if (!ActiveUsers.TryGetValue(session_token, out username)) {
				return null;
			}

			return DataBase.Tables["Users"].Rows.Find(username);
		}

		// COMMANDS
		// user commands

		public Boolean TryLogin(String username, String password, out User user)
		{
			DataRow row;
			
			user = null;
			row = GetUserRecord(username);
			if (row == null || (String)row["Password"] != password) {
				return false;
			}

			Guid session_token = Guid.NewGuid();
			ActiveUsers.Add(session_token, username);
			user = new User(username, session_token, (Role)row["Role"]);

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
			DataRow row;
			
			if (username == "" || password == "") {
				return false;
			}

			if (DataBase.Tables["Users"].Rows.Contains(username)) {
				return false; 
			}

			row = DataBase.Tables["Users"].NewRow();
			row["UserName"] = username;
			row["Password"] = password;
			row["Role"] = Role.Consumer;
			DataBase.Tables["Users"].Rows.Add(row);

			return true;
		}

		public Boolean TryDeregister(Guid session_token, String password)
		{
			DataRow row;

			if (password == "") {
				return false;
			}
			
			row = GetUserRecord(session_token);
			if (row == null || (String)row["Password"] != password) {
				return false;
			}

			ActiveUsers.Remove(session_token);
			DataBase.Tables["Users"].Rows.Remove(row);

			return true;
		}

		// room commands

		public Boolean TryAddRoom(Guid session_token, Room room)
		{
			DataRow row;

			row = GetUserRecord(session_token);
			if (row == null || (Role)row["Role"] != Role.Owner) {
				return false;
			}

			row = DataBase.Tables["Rooms"].NewRow();
			room.WriteDataRow(row);
			DataBase.Tables["Rooms"].Rows.Add(row);

			return true;
		}

		// session_token: the user session token.
		// roomName: the other the name of the room the user want remove.
		// return: a boolean indicating wether the method failed or passed
		//
		// this method finds if the user wanting to remove the room has the
		// right permissions to do so, then it checks if the room name is
		// registered in the Products table and removes it if it is found and
		// the user has the right permissions.
		public Boolean TryRemoveRoom(Guid session_token, Int64 roomId)
		{
			var userRecord = GetUserRecord(session_token);
			if (userRecord == null || (Role)userRecord["Role"] != Role.Owner) {
				return false;
			}

			var query = $"Name = '{roomId}'";
			var roomRecords = DataBase.Tables["Rooms"].Select(query);
			if (roomRecords == null || roomRecords[0] == null) {
				return false;
			}

			DataBase.Tables["Rooms"].Rows.Remove(roomRecords[0]);
			
			return true;
		}

		// this method is passed 2 arguments one being the user session token
		// and the other a stream to which the table schema and data is written
		// so that the client can read from it and get the needed data
		public Boolean TryGetRoomData(Guid session_token, MemoryStream stream)
		{
			DataRow row;
			
			row = GetUserRecord(session_token);
			if (row == null) {
				return false;
			}

			DataBase.Tables["Rooms"].WriteXml(stream, XmlWriteMode.WriteSchema);
			stream.Position = 0;

			if (stream.Length == 0) {
				return false;
			}

			return true;
		}
	}
}