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

	enum ProductType
	{
		EscapeRoom,
		Consumable
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
			col = new DataColumn("NumberOfRounds", typeof(Int32));
			roomAttributeTable.Columns.Add(col);
			col = new DataColumn("MaxDuration", typeof(Int32));
			roomAttributeTable.Columns.Add(col);
			roomAttributeTable.PrimaryKey = primaryKeys;

			var consumableAttributeTable = new DataTable("ConsumableAttribute");
			primaryKeys = new DataColumn[1];
			col = new DataColumn("ProductId", typeof(Int64));
			primaryKeys[0] = col;
			consumableAttributeTable.Columns.Add(col);
			consumableAttributeTable.PrimaryKey = primaryKeys;

			var orderTable = new DataTable("Orders");
			primaryKeys = new DataColumn[1];
			col = new DataColumn("OrderId", typeof(Int64));
			col.AutoIncrement = true;
			orderTable.Columns.Add(col);
			primaryKeys[0] = col;
			col = new DataColumn("UserId", typeof(Int64));
			orderTable.Columns.Add(col);
			col = new DataColumn("OrderDateTime", typeof(DateTime));
			orderTable.Columns.Add(col);
			orderTable.PrimaryKey = primaryKeys;

			var orderItemTable = new DataTable("OrderItems");
			primaryKeys = new DataColumn[1];
			col = new DataColumn("OrderItemId", typeof(Int64));
			col.AutoIncrement = true;
			orderItemTable.Columns.Add(col);
			primaryKeys[0] = col;
			col = new DataColumn("OrderId", typeof(Int64));
			orderItemTable.Columns.Add(col);
			col = new DataColumn("ProductId", typeof(Int64));
			orderItemTable.Columns.Add(col);
			col = new DataColumn("Amount", typeof(Int32));
			orderItemTable.Columns.Add(col);
			orderItemTable.PrimaryKey = primaryKeys;

			var reservationTable = new DataTable("Reservations");
			primaryKeys = new DataColumn[1];
			col = new DataColumn("ReservationId", typeof(Int64));
			col.AutoIncrement = true;
			reservationTable.Columns.Add(col);
			primaryKeys[0] = col;
			col = new DataColumn("OrderId", typeof(Int64));
			reservationTable.Columns.Add(col);
			col = new DataColumn("RoomId", typeof(Int64));
			reservationTable.Columns.Add(col);
			col = new DataColumn("GroupSize", typeof(Int32));
			reservationTable.Columns.Add(col);
			col = new DataColumn("Date", typeof(DateTime));
			reservationTable.Columns.Add(col);
			col = new DataColumn("RoundNumber", typeof(Int32));
			reservationTable.Columns.Add(col);
			reservationTable.PrimaryKey = primaryKeys;

			DataBase.Tables.AddRange(new DataTable[]{
				userTable, productTable, roomAttributeTable, orderTable, orderItemTable,
				reservationTable, consumableAttributeTable});

			var rel = new DataRelation("ProductRoomAttribute", productTable.Columns["ProductId"],
				roomAttributeTable.Columns["ProductId"]);
			DataBase.Relations.Add(rel);
			rel = new DataRelation("ProductConsumableAttribute", productTable.Columns["ProductId"],
				consumableAttributeTable.Columns["ProductId"]);
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

		private DataRow GetUserRow(String email)
		{
			var query = $"Email = '{email}'";
			var userRows = this.DataBase.Tables["Users"].Select(query);
			if (userRows.Length == 0) {
				return null;
			}

			return userRows[0];
		}

		private DataRow GetUserRow(Guid sessionToken)
		{
			Int64 userId;

			if (!this.ActiveUsers.TryGetValue(sessionToken, out userId)) {
				return null;
			}

			return this.DataBase.Tables["Users"].Rows.Find(userId);
		}

		public Boolean TryLogin(String userName, String password, out User user)
		{
			DataRow userRow;
			
			user = null;
			userRow = this.GetUserRow(userName);
			if (userRow == null || (String)userRow["Password"] != password) {
				return false;
			}

			Guid session_token = Guid.NewGuid();
			ActiveUsers.Add(session_token, (Int64)userRow["UserId"]);
			user = new User(userName, session_token, (Role)userRow["Role"]);

			return true;
		}

		public Boolean TryLogout(Guid sessionToken)
		{
			if (!this.ActiveUsers.ContainsKey(sessionToken)) {
				return false;
			}

			this.ActiveUsers.Remove(sessionToken);

			return true;
		}

		public Boolean TryRegister(String userName, String email, String password)
		{
			if (userName == "" || email == "" || password == "") {
				return false;
			}

			var query = $"Email = '{email}'";
			var userRows = this.DataBase.Tables["Users"].Select(query);
			if (userRows.Length != 0) {
				return false; 
			}

			var userRow = this.DataBase.Tables["Users"].NewRow();
			userRow["UserName"] = userName;
			userRow["Email"] = email;
			userRow["Password"] = password;
			userRow["Role"] = Role.Consumer;
			this.DataBase.Tables["Users"].Rows.Add(userRow);

			return true;
		}

		public Boolean TryDeregister(Guid sessionToken, String password)
		{
			if (password == "") {
				return false;
			}
			
			var userRow = this.GetUserRow(sessionToken);
			if (userRow == null || (String)userRow["Password"] != password) {
				return false;
			}

			this.ActiveUsers.Remove(sessionToken);
			this.DataBase.Tables["Users"].Rows.Remove(userRow);

			return true;
		}

		public Boolean TryAddRoom(Guid sessionToken, Room room)
		{
			var userRow = GetUserRow(sessionToken);
			if (userRow == null || (Role)userRow["Role"] != Role.Owner) {
				return false;
			}

			// TODO: CHECK IF THE ROOM IS ALREADY IN THE TABLE
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
			roomAttributeRow["NumberOfRounds"] = room.NumberOfRounds;
			roomAttributeRow["MaxDuration"] = room.MaxDuration;
			DataBase.Tables["RoomAttributes"].Rows.Add(roomAttributeRow);
			
			return true;
		}

		public Boolean TryRemoveRoom(Guid sessionToken, Int64 productId)
		{
			var userRow = this.GetUserRow(sessionToken);
			if (userRow == null || (Role)userRow["Role"] != Role.Owner) {
				return false;
			}

			var query = $"ProductId = '{productId}'";
			var roomRow = this.DataBase.Tables["Products"].Select(query);
			if (roomRow.Length == 0) {
				return false;
			}

			DataBase.Tables["Products"].Rows.Remove(roomRow[0]);
			DataBase.Tables["RoomAttributes"].Rows.Remove(roomRow[0]);
			
			return true;
		}

		public Boolean TryFetchRooms(Guid sessionToken, MemoryStream stream)
		{
			var userRow = this.GetUserRow(sessionToken);
			if (userRow == null) {
				return false;
			}

			var rel = this.DataBase.Relations["ProductRoomAttribute"];
			var productTable = rel.ParentTable;
			var roomAttributeTable = rel.ChildTable;
			var rooms = new List<Room>();

			foreach (DataRow roomAttributeRow in roomAttributeTable.Rows) {
				var room = new Room();
				var productRow = roomAttributeRow.GetParentRow(rel);
				room.ProductId = (Int64)productRow["ProductId"];
				room.Name = (String)productRow["ProductName"];
				room.Description = (String)productRow["Description"];
				room.Price = (Single)productRow["Price"];
				room.Available = (Boolean)productRow["Available"];
				room.Theme = (String)roomAttributeRow["Theme"];
				room.Capacity = (Int32)roomAttributeRow["Capacity"];
				room.NumberOfRounds = (Int32)roomAttributeRow["NumberOfRounds"];
				room.MaxDuration = (Int32)roomAttributeRow["MaxDuration"];
				rooms.Add(room);
			}

			var rawJson = JsonSerializer.SerializeToUtf8Bytes<List<Room>>(rooms);
			stream.Write(rawJson, 0, rawJson.Length);
			stream.Position = 0;
			if (stream.Length == 0) {
				return false;
			}

			return true;
		}
		
		public Int32 CheckReservation(Reservation reservation)
		{
			var query = $"RoomId = {reservation.RoomId}" +
				$" AND Date = #{reservation.DateTime.Date}#" +
				$" AND RoundNumber = {reservation.RoundNumber}";
			var rows = this.DataBase.Tables["Reservations"].Select(query);
			if (rows.Length == 0) {
				return -1;
			}

			var n = 0;
			foreach (var row in rows) {
				n += (Int32)row["GroupSize"];
			}

			return n;
		}

		public Boolean TryAddConsumable(Guid sessionToken, Product consumable)
		{
			var userRow = this.GetUserRow(sessionToken);
			if (userRow == null || (Role)userRow["Role"] != Role.CafeManager) {
				return false;
			}

			// TODO: CHECK IF THE ROW IS ALREADY IN THE TABLE
			var productRow = this.DataBase.Tables["Products"].NewRow();
			productRow["ProductName"] = consumable.Name;
			productRow["Description"] = consumable.Description;
			productRow["Price"] = consumable.Price;
			productRow["Available"] = consumable.Available;
			this.DataBase.Tables["Product"].Rows.Add(productRow);
			var consumableAttributeRow = this.DataBase.Tables["Products"].NewRow();
			consumableAttributeRow["ProductId"] = productRow["ProductId"];
			this.DataBase.Tables["ConsumableAttributes"].Rows.Add(consumableAttributeRow);

			return true;
		}

		public Boolean TryRemoveConsumable(Guid sessionToken, Product consumable)
		{
			var userRow = GetUserRow(sessionToken);
			if (userRow == null || (Role)userRow["Role"] != Role.CafeManager) {
				return false;
			}

			var query = $"ProductId = {consumable.ProductId}";
			var rows = this.DataBase.Tables["Products"].Select(query);
			if (rows.Length == 0) {
				return false;
			}

			this.DataBase.Tables["Products"].Rows.Remove(rows[0]);
			this.DataBase.Tables["ConsumableAttributes"].Rows.Remove(rows[0]);

			return true;
		}

		public Boolean TryFetchConsumables(Guid sessionToken, MemoryStream stream)
		{
			var userRow = this.GetUserRow(sessionToken);
			if (userRow == null || (Role)userRow["Role"] != Role.CafeManager) {
				return false;
			}

			var rel = this.DataBase.Relations["ProductConsumableAttribute"];
			var productTable = rel.ParentTable;
			var consumableAttributeTable = rel.ChildTable;
			var consumables = new List<Product>();

			foreach (DataRow consumableRow in consumableAttributeTable.Rows) {
				var consumable = new Product();
				var productRow = consumableRow.GetParentRow(rel);
				consumable.ProductId = (Int64)productRow["ProductId"];
				consumable.Name = (String)productRow["ProductName"];
				consumable.Description = (String)productRow["Description"];
				consumable.Price = (Single)productRow["Price"];
				consumable.Available = (Boolean)productRow["Available"];
				consumables.Add(consumable);
			}

			var raw_json = JsonSerializer.SerializeToUtf8Bytes<List<Product>>(consumables);
			stream.Write(raw_json, 0, raw_json.Length);
			stream.Position = 0;
			if (stream.Length == 0) {
				return false;
			}

			return true;
		}

		public Boolean TryPay(Guid sessionToken, MemoryStream stream) 
		{
			var userRow = this.GetUserRow(sessionToken);
			if (userRow == null) {
				return false;
			}

			var order = JsonSerializer.Deserialize<Order>(stream.ToArray());

			var orderTable = this.DataBase.Tables["Orders"];
			var reservationTable = this.DataBase.Tables["Reservations"];
			var orderItemTable = this.DataBase.Tables["OrderItems"];

			var orderRow = orderTable.NewRow();
			orderRow["UserId"] = (Int64)userRow["UserId"];
			orderRow["OrderDateTime"] = DateTime.Now;
			orderTable.Rows.Add(orderRow);

			foreach (var reservation in order.Reservations) {
				var reservationRow = reservationTable.NewRow();
				reservationRow["OrderId"] = orderRow["OrderId"];
				reservationRow["RoomId"] = reservation.RoomId;
				reservationRow["GroupSize"] = reservation.GroupSize;
				reservationRow["Date"] = reservation.DateTime.Date;
				reservationRow["RoundNumber"] = reservation.RoundNumber;
				reservationTable.Rows.Add(reservationRow);
			}

			foreach (var item in order.Items) {
				var orderItemRow = orderItemTable.NewRow();
				orderItemRow["OrderId"] = orderRow["OrderId"];
				orderItemRow["ProductId"] = item.ProductId;
				orderItemRow["Amount"] = item.Amount;
				orderItemTable.Rows.Add(orderItemRow);
			}

			return true;
		}
	}
}
