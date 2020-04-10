using System;

namespace App
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Data;
	using System.Text;
	using System.Text.Json;

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

			DataColumn col;
			DataColumn[] primaryKeys;

			var userTable = new DataTable("Users");
			primaryKeys = new DataColumn[1];
			col = new DataColumn("UserId", typeof(Int64));
			col.AutoIncrement = true;
			userTable.Columns.Add(col);
			primaryKeys[0] = col;
			col = new DataColumn("UserName", typeof(String));
			userTable.Columns.Add(col);
			col = new DataColumn("Email", typeof(String));
			col.Unique = true;
			userTable.Columns.Add(col);
			col = new DataColumn("Password", typeof(String));
			userTable.Columns.Add(col);
			col = new DataColumn("Role", typeof(Int32));
			userTable.Columns.Add(col);
			userTable.PrimaryKey = primaryKeys;

			var productTable = new DataTable("Products");
			primaryKeys = new DataColumn[1];
			col = new DataColumn("ProductId", typeof(Int64));
			col.AutoIncrement = true;
			productTable.Columns.Add(col);
			primaryKeys[0] = col;
			col = new DataColumn("ProductName", typeof(String));
			col.Unique = true;
			productTable.Columns.Add(col);
			col = new DataColumn("Description", typeof(String));
			productTable.Columns.Add(col);
			col = new DataColumn("Price", typeof(Single));
			productTable.Columns.Add(col);
			col = new DataColumn("Available", typeof(Boolean));
			productTable.Columns.Add(col);
			productTable.PrimaryKey = primaryKeys;

			var roomAttributeTable = new DataTable("RoomAttributes");
			primaryKeys = new DataColumn[1];
			col = new DataColumn("ProductId", typeof(Int64));
			primaryKeys[0] = col;
			roomAttributeTable.Columns.Add(col);
			col = new DataColumn("Theme", typeof(String));
			roomAttributeTable.Columns.Add(col);
			col = new DataColumn("Capacity", typeof(Int32));
			roomAttributeTable.Columns.Add(col);
			roomAttributeTable.PrimaryKey = primaryKeys;

			var orderTable = new DataTable("Orders");
			primaryKeys = new DataColumn[1];
			col = new DataColumn("OrderId", typeof(Int64));
			col.AutoIncrement = true;
			orderTable.Columns.Add(col);
			primaryKeys[0] = col;
			col=new DataColumn("UserId", typeof(Int64));
			orderTable.Columns.Add(col);
			col=new DataColumn("OrderDateTime", typeof(DateTime));
			orderTable.Columns.Add(col);
			orderTable.PrimaryKey = primaryKeys;

			var orderItemTable = new DataTable("OrderItems");
			primaryKeys = new DataColumn[2];
			col=new DataColumn("OrderId", typeof(Int64));
			orderItemTable.Columns.Add(col);
			primaryKeys[0] = col;
			col=new DataColumn("ProductId", typeof(Int64));
			orderItemTable.Columns.Add(col);
			primaryKeys[1] = col;
			col=new DataColumn("Amount", typeof(Int32));
			orderItemTable.Columns.Add(col);
			orderItemTable.PrimaryKey = primaryKeys;

			DataBase.Tables.AddRange(new DataTable[]{
				userTable, productTable, roomAttributeTable, orderTable, orderItemTable});

			var rel = new DataRelation("ProductRoomAttribute",
				productTable.Columns["ProductId"],
				roomAttributeTable.Columns["ProductId"]);
			DataBase.Relations.Add(rel);

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
			DataBase.WriteXmlSchema("Data/ServerSchema.xml");
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
			DataRow userRecord;
			
			user = null;
			userRecord = GetUserRecord(username);
			if (userRecord == null || (String)userRecord["Password"] != password) {
				return false;
			}

			Guid session_token = Guid.NewGuid();
			ActiveUsers.Add(session_token, (Int64)userRecord["UserId"]);
			user = new User(username, session_token, (Role)userRecord["Role"]);

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

			var productRow = DataBase.Tables["Products"].NewRow();
			productRow["ProductName"] = room.Name;
			productRow["Description"] = room.Description;
			productRow["Price"] = room.Price;
			productRow["Available"] = room.Available;
			DataBase.Tables["Products"].Rows.Add(productRow);
			var roomAttributeRow = DataBase.Tables["RoomAttributes"].NewRow();
			roomAttributeRow["ProductId"] = productRow["ProductId"];
			roomAttributeRow["Theme"] = room.Theme;
			roomAttributeRow["Capacity"] = room.Capacity;
			DataBase.Tables["RoomAttributes"].Rows.Add(roomAttributeRow);
			
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
			DataRow userRecord;
			
			userRecord = GetUserRecord(session_token);
			if (userRecord == null) {
				return false;
			}

			var rel = DataBase.Relations["ProductRoomAttribute"];
			var productTable = rel.ParentTable;
			var roomAttributeTable = rel.ChildTable;

			var rooms = new List<Room>();

			for (int i = 0; i < roomAttributeTable.Rows.Count; i += 1) {
				var roomAttributeRow = roomAttributeTable.Rows[i];
				var productRow = roomAttributeRow.GetParentRow(rel);
				var room = new Room();
				room.ProductId = (Int64)productRow["ProductId"];
				room.Name = (String)productRow["ProductName"];
				room.Description = (String)productRow["Description"];
				room.Price = (Single)productRow["Price"];
				room.Theme = (String)roomAttributeRow["Theme"];
				room.Capacity = (Int32)roomAttributeRow["Capacity"];
				rooms.Add(room);
			}

			var raw_json = JsonSerializer.SerializeToUtf8Bytes<List<Room>>(rooms);
			stream.Write(raw_json, 0, raw_json.Length);
			stream.Position = 0;
			if (stream.Length == 0) {
				return false;
			}

			return true;
		}
		
		private Boolean TryPay(Guid session_token, MemoryStream stream) 
		{
			DataRow userRecord;
			List<OrderItem> orderItems;

			userRecord = GetUserRecord(session_token);
			if (userRecord == null) {
				return false;
			}

			orderItems = JsonSerializer.Deserialize<List<OrderItem>>(stream.ToArray());

			var orderRow = DataBase.Tables["Orders"].NewRow();
			orderRow["OrderDateTime"] = DateTime.Now;
			foreach (OrderItem item in orderItems) {
				var orderItemRow = DataBase.Tables["OrderItems"].NewRow();
				orderItemRow["OrderId"] = orderRow["OrderId"];
				orderItemRow["ProductId"] = item.ProductId;
				orderItemRow["Amount"] = item.Amount;
				DataBase.Tables["OrderItems"].Rows.Add(orderRow);
			}

			DataBase.Tables["Orders"].Rows.Add(orderRow);

			return true;
		}
	}
}