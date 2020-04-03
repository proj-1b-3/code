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
			// DataTable table;
			// DataColumn col;
			// DataColumn[] keys;
			// DataRelation rel;

			DataBase = new DataSet("DataBase");
			DataBase.ReadXmlSchema("Data/ServerSchema.xml");
			// keys = new DataColumn[1];
			
			// table = new DataTable();
			// table.TableName = "Tickets";

			// col = new DataColumn();
			// col.ColumnName = "TicketId";
			// col.DataType = typeof(Guid);
			// col.Unique = true;
			// table.Columns.Add(col);

			// keys[0] = col;

			// col = new DataColumn();
			// col.ColumnName = "UserName";
			// col.DataType = typeof(String);
			// col.Unique = true;
			// table.Columns.Add(col);

			// col = new DataColumn();
			// col.ColumnName = "RoomName";
			// col.DataType = typeof(String);
			// col.Unique = true;
			// table.Columns.Add(col);

			// table.PrimaryKey = keys;
			// DataBase.Tables.Add(table);

			// rel = new DataRelation("TicketOwner",
			// 	DataBase.Tables["Users"].Columns["UserName"],
			// 	DataBase.Tables["Tickets"].Columns["UserName"]);
			// DataBase.Relations.Add(rel);

			// rel = new DataRelation("TicketRoom",
			// 	DataBase.Tables["Rooms"].Columns["RoomName"],
			// 	DataBase.Tables["Tickets"].Columns["RoomName"]);
			// DataBase.Relations.Add(rel);
			
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

		/* command */

		public Boolean TryLogin(String username, String password, out User user)
		{
			DataRow row;
			
			user = null;
			row = GetUserRecord(username);
			if (row == null || row.Field<String>("Password") != password) {
				return false;
			}

			Guid session_token = Guid.NewGuid();
			ActiveUsers.Add(session_token, username);
			user = new User(username, session_token, row.Field<Role>("Role"));

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
			if (row == null || row.Field<String>("Password") != password) {
				return false;
			}

			ActiveUsers.Remove(session_token);
			DataBase.Tables["Users"].Rows.Remove(row);

			return true;
		}

		public Boolean TryAddRoom(Guid session_token, Room room)
		{
			DataRow row;

			row = GetUserRecord(session_token);
			if (row == null || row.Field<Role>("Role") != Role.Owner) {
				return false;
			}

			row = DataBase.Tables["Rooms"].NewRow();
			row["RoomName"] = room.Name;
			row["Theme"] = room.Theme;
			row["Discription"] = room.Discription;
			row["Capacity"] = room.Capacity;
			row["Price"] = room.Price;
			DataBase.Tables["Rooms"].Rows.Add(row);

			return true;
		}

		public Boolean TryRemoveRoom(Guid session_token, String roomname)
		{
			DataRow row;

			row = GetUserRecord(session_token);
			if (row == null || row.Field<Role>("Role") != Role.Owner) {
				return false;
			}

			row = DataBase.Tables["Rooms"].Rows.Find(roomname);
			if (row == null) {
				return false;
			}

			DataBase.Tables["Rooms"].Rows.Remove(row);
			
			return true;
		}

		public Boolean TryGetRoomData(Guid session_token, MemoryStream tabledata)
		{
			DataRow row;
			
			row = GetUserRecord(session_token);
			if (row == null) {
				return false;
			}

			DataBase.Tables["Rooms"].WriteXml(tabledata, XmlWriteMode.WriteSchema);
			if (tabledata.Length == 0) {
				return false;
			}

			tabledata.Position = 0;

			return true;
		}
	}
}