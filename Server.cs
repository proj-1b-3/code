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
		private Dictionary<Guid, Int64> ActiveUsers;

		private DataSet DataBase;
		
		public Server()
		{
			DataBase = new DataSet("DataBase");
			// DataBase.ReadXmlSchema("Data/ServerSchema.xml");

			var table = new DataTable("Users");
			var keys = new DataColumn[1];
			var col = new DataColumn("UserId");
			col.DataType = typeof(Int64);
			col.AutoIncrement = true;
			table.Columns.Add(col);
			keys[0] = col;
			col = new DataColumn("UserName");
			col.DataType = typeof(String);
			table.Columns.Add(col);
			col = new DataColumn("Email");
			col.DataType = typeof(String);
			col.Unique = true;
			table.Columns.Add(col);
			col = new DataColumn("Password");
			col.DataType = typeof(String);
			table.Columns.Add(col);
			col = new DataColumn("Role");
			col.DataType = typeof(Int32);
			table.Columns.Add(col);
			table.PrimaryKey = keys;
			DataBase.Tables.Add(table);

			table = new DataTable("Products");
			col = new DataColumn("ProductId");
			col.DataType = typeof(Int64);
			col.AutoIncrement = true;
			table.Columns.Add(col);
			keys[0] = col;
			col = new DataColumn("ProductName");
			col.DataType = typeof(String);
			col.Unique = true;
			table.Columns.Add(col);
			col = new DataColumn("Description");
			col.DataType = typeof(String);
			table.Columns.Add(col);
			col = new DataColumn("Price");
			col.DataType = typeof(Single);
			table.Columns.Add(col);
			col = new DataColumn("Available");
			col.DataType = typeof(Boolean);
			table.Columns.Add(col);
			table.PrimaryKey = keys;
			DataBase.Tables.Add(table);

			table = new DataTable("ProductRooms");
			col = new DataColumn("ProductId", typeof(Int64));
			table.Columns.Add(col);
			col = new DataColumn("Theme", typeof(String));
			table.Columns.Add(col);
			col = new DataColumn("Capacity", typeof(Int32));
			table.Columns.Add(col);
			DataBase.Tables.Add(table);
			var rel = new DataRelation("RoomProducts",
				DataBase.Tables["Products"].Columns["ProductId"],
				DataBase.Tables["ProductRooms"].Columns["ProductId"]);

			ActiveUsers = new Dictionary<Guid, Int64>();
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

		private DataRow GetUserRecord(String email)
		{
			var query = $"Email = '{email}'";
			var rows = DataBase.Tables["Users"].Select(query);
			if (rows.Length == 0) {
				return null;
			}

			return rows[0];
		}

		private DataRow GetUserRecord(Guid session_token)
		{
			Int64 userId;

			if (!ActiveUsers.TryGetValue(session_token, out userId)) {
				return null;
			}

			return DataBase.Tables["Users"].Rows.Find(userId);
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
			ActiveUsers.Add(session_token, (Int64)row["UserId"]);
			user = new User(username, session_token, (Role)row["Role"]);

			return true;
		}

		public Boolean TryLogout(Guid session_token)
		{
			if (!ActiveUsers.ContainsKey(session_token)) {
				return false;
			}

			ActiveUsers.Remove(session_token);

			return true;
		}

		public Boolean TryRegister(String userName, String email, String password)
		{
			DataRow row;
			
			if (userName == "" || password == "") {
				return false;
			}

			if (DataBase.Tables["Users"].Rows.Contains(userName)) {
				return false; 
			}

			row = DataBase.Tables["Users"].NewRow();
			row["UserName"] = userName;
			row["Email"] = email;
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

			row = DataBase.Tables["Products"].NewRow();
			row["ProductName"] = room.Name;
			row["Description"] = room.Discription;
			row["Price"] = room.Price;
			row["Available"] = room.Available;
			DataBase.Tables["Products"].Rows.Add(row);

			return true;
		}

		public Boolean TryRemoveRoom(Guid session_token, Int64 productId)
		{
			var userRecord = GetUserRecord(session_token);
			if (userRecord == null || (Role)userRecord["Role"] != Role.Owner) {
				return false;
			}

			var query = $"ProductId = '{productId}'";
			var roomRecords = DataBase.Tables["Products"].Select(query);
			if (roomRecords.Length == 0) {
				return false;
			}

			DataBase.Tables["Products"].Rows.Remove(roomRecords[0]);
			
			return true;
		}

		public Boolean TryGetRoomData(Guid session_token, MemoryStream stream)
		{
			DataRow row;
			
			row = GetUserRecord(session_token);
			if (row == null) {
				return false;
			}

			DataBase.Tables["Products"].WriteXml(stream, XmlWriteMode.WriteSchema);
			stream.Position = 0;

			if (stream.Length == 0) {
				return false;
			}

			return true;
		}
		
		public Boolean TryPay(Guid session_token, MemoryStream stream) 
		{
			// DataTable selection;

			return true;
		}
	}
}