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
			col = new DataColumn("Forename", typeof(String));
			userTable.Columns.Add(col);
			col = new DataColumn("Suraname", typeof(String));
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

			var consumableAttributeTable = new DataTable("ConsumableAttributes");
			primaryKeys = new DataColumn[1];
			col = new DataColumn("ProductId", typeof(Int64));
			primaryKeys[0] = col;
			consumableAttributeTable.Columns.Add(col);
			consumableAttributeTable.PrimaryKey = primaryKeys;

			var reservationTable = new DataTable("Reservations");
			primaryKeys = new DataColumn[1];
			col = new DataColumn("ReservationId", typeof(Int64));
			col.AutoIncrement = true;
			reservationTable.Columns.Add(col);
			primaryKeys[0] = col;
			col = new DataColumn("UserId", typeof(Int64));
			reservationTable.Columns.Add(col);
			col = new DataColumn("RoomId", typeof(Int64));
			reservationTable.Columns.Add(col);
			col = new DataColumn("TargetDateTime", typeof(DateTime));
			reservationTable.Columns.Add(col);
			col = new DataColumn("RoundNumber", typeof(Int32));
			reservationTable.Columns.Add(col);
			col = new DataColumn("GroupSize", typeof(Int32));
			reservationTable.Columns.Add(col);
			col = new DataColumn("OrderDateTime", typeof(DateTime));
			reservationTable.Columns.Add(col);
			reservationTable.PrimaryKey = primaryKeys;

			var consumableItemTable = new DataTable("ConsumableItems");
			primaryKeys = new DataColumn[1];
			col = new DataColumn("ConsumableItemId", typeof(Int64));
			col.AutoIncrement = true;
			consumableItemTable.Columns.Add(col);
			primaryKeys[0] = col;
			col = new DataColumn("ReservationId", typeof(Int64));
			consumableItemTable.Columns.Add(col);
			col = new DataColumn("ProductId", typeof(Int64));
			consumableItemTable.Columns.Add(col);
			col = new DataColumn("Amount", typeof(Int32));
			consumableItemTable.Columns.Add(col);
			consumableItemTable.PrimaryKey = primaryKeys;

			var reviewTable = new DataTable("Reviews");
			primaryKeys = new DataColumn[1];
			col = new DataColumn("ReviewId", typeof(Int64));
			col.AutoIncrement = true;
			primaryKeys[0] = col;
			reviewTable.Columns.Add(col);
			col = new DataColumn("UserId", typeof(Int64));
			reviewTable.Columns.Add(col);
			col = new DataColumn("RoomId", typeof(Int64));
			reviewTable.Columns.Add(col);
			col = new DataColumn("DateTime", typeof(DateTime));
			reviewTable.Columns.Add(col);
			col = new DataColumn("Text", typeof(String));
			reviewTable.Columns.Add(col);
			reviewTable.PrimaryKey = primaryKeys;

			DataBase.Tables.AddRange(new DataTable[]{
				userTable, productTable, roomAttributeTable, reservationTable, 
				consumableAttributeTable, consumableItemTable, reviewTable});

			var rel = new DataRelation("Product-RoomAttribute",
				productTable.Columns["ProductId"],
				roomAttributeTable.Columns["ProductId"]);
			DataBase.Relations.Add(rel);
			rel = new DataRelation("Product-ConsumableAttribute",
				productTable.Columns["ProductId"],
				consumableAttributeTable.Columns["ProductId"]);
			DataBase.Relations.Add(rel);
			rel = new DataRelation("Reservation-ConsumableItem",
				reservationTable.Columns["ReservationId"],
				consumableItemTable.Columns["ReservationId"]);
			DataBase.Relations.Add(rel);
			rel = new DataRelation("ConsumableAttribute-ConsumableItem",
				consumableAttributeTable.Columns["ProductId"],
				consumableItemTable.Columns["ProductId"]);
			DataBase.Relations.Add(rel);
			rel = new DataRelation("RoomAttribute-Reservation",
				roomAttributeTable.Columns["ProductId"],
				reservationTable.Columns["RoomId"]);
			DataBase.Relations.Add(rel);
			rel = new DataRelation("User-Review",
				userTable.Columns["UserId"],
				reviewTable.Columns["UserId"]);
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
			user = null;
			var userRow = this.GetUserRow(userName);
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

			var rows = this.DataBase.Tables["Products"].Select(
				"ProductName = '" + room.Name + "'");
			if (rows.Length != 0) {
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

		public Boolean TryEditRoom(Guid sessionToken, Room room)
		{
			var userRow = GetUserRow(sessionToken);
			if (userRow == null || (Role)userRow["Role"] != Role.Owner) {
				return false;
			}
	
			var rel = this.DataBase.Relations["Product-RoomAttribute"];
			var roomAttributeRow = rel.ChildTable.Rows.Find(room.ProductId);
			var productRow = roomAttributeRow.GetParentRow(rel);
			productRow["ProductName"] = room.Name;
			productRow["Description"] = room.Description;
			productRow["Price"] = room.Price;
			productRow["Available"] = room.Available;
			roomAttributeRow["Theme"] = room.Theme;
			roomAttributeRow["Capacity"] = room.Capacity;
			roomAttributeRow["NumberOfRounds"] = room.NumberOfRounds;
			roomAttributeRow["MaxDuration"] = room.MaxDuration;

			return true;
		}

		public Boolean TryFetchRooms(Guid sessionToken, MemoryStream stream)
		{
			var userRow = this.GetUserRow(sessionToken);
			if (userRow == null) {
				return false;
			}

			var rel = this.DataBase.Relations["Product-RoomAttribute"];
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
			var query = $"RoomId = {reservation.Room.ProductId}" +
				$" AND TargetDateTime = #{reservation.TargetDateTime}#" +
				$" AND RoundNumber = {reservation.RoundNumber}";
			var rows = this.DataBase.Tables["Reservations"].Select(query);

			Int32 n = 0;
			foreach (var row in rows) {
				n += (Int32)row["GroupSize"];
			}

			return n;
		}

		public Boolean TryAddConsumable(Guid sessionToken, Consumable consumable)
		{
			var userRow = this.GetUserRow(sessionToken);
			if (userRow == null || (Role)userRow["Role"] != Role.CafeManager) {
				return false;
			}

			var rows = this.DataBase.Tables["Products"].Select(
				"ProductName = '" + consumable.Name + "'");
			if (rows.Length != 0) {
				return false;
			}

			var productRow = this.DataBase.Tables["Products"].NewRow();
			productRow["ProductName"] = consumable.Name;
			productRow["Description"] = consumable.Description;
			productRow["Price"] = consumable.Price;
			productRow["Available"] = consumable.Available;
			this.DataBase.Tables["Products"].Rows.Add(productRow);
			var consumableAttributeRow = this.DataBase.Tables["ConsumableAttributes"].NewRow();
			consumableAttributeRow["ProductId"] = productRow["ProductId"];
			this.DataBase.Tables["ConsumableAttributes"].Rows.Add(consumableAttributeRow);

			return true;
		}

		public Boolean TryRemoveConsumable(Guid sessionToken, Consumable consumable)
		{
			var userRow = GetUserRow(sessionToken);
			if (userRow == null || (Role)userRow["Role"] != Role.CafeManager) {
				return false;
			}

			var productRow = this.DataBase.Tables["Products"].Rows.Find(
				consumable.ProductId);
			if (productRow == null) {
				return false;
			}

			this.DataBase.Tables["Products"].Rows.Remove(productRow);

			return true;
		}

		public Boolean TryEditConsumable(Guid sessionToken, Consumable consumable)
		{
			var userRow = this.GetUserRow(sessionToken);
			if (userRow == null || (Role)userRow["Role"] != Role.CafeManager) {
				return false;
			}
		
			var rel = this.DataBase.Relations["Product-ConsumableAttribute"];	
			var consumableRow = rel.ChildTable.Rows.Find(consumable.ProductId);
			var productRow = consumableRow.GetParentRow(rel);
			productRow["ProductName"] = consumable.Name;
			productRow["Description"] = consumable.Description;
			productRow["Price"] = consumable.Price;
			productRow["Available"] = consumable.Available;

			return true;
		}

		public Boolean TryFetchConsumables(Guid sessionToken, MemoryStream stream)
		{
			var userRow = this.GetUserRow(sessionToken);
			if (userRow == null) {
				return false;
			}

			var rel = this.DataBase.Relations["Product-ConsumableAttribute"];
			var productTable = rel.ParentTable;
			var consumableAttributeTable = rel.ChildTable;
			var consumables = new List<Consumable>();

			foreach (DataRow consumableRow in consumableAttributeTable.Rows) {
				var consumable = new Consumable();
				var productRow = consumableRow.GetParentRow(rel);
				consumable.ProductId = (Int64)productRow["ProductId"];
				consumable.Name = (String)productRow["ProductName"];
				consumable.Description = (String)productRow["Description"];
				consumable.Price = (Single)productRow["Price"];
				consumable.Available = (Boolean)productRow["Available"];
				consumables.Add(consumable);
			}

			var rawJson = JsonSerializer.SerializeToUtf8Bytes<List<Consumable>>(consumables);
			stream.Write(rawJson, 0, rawJson.Length);
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

			var reservation = JsonSerializer.Deserialize<Reservation>(stream.ToArray());

			var rel = this.DataBase.Relations["Reservation-ConsumableItem"];
			var reservationTable = rel.ParentTable;
			var consumableItemTable = rel.ChildTable;

			var reservationRow = reservationTable.NewRow();
			reservationRow["RoomId"] = reservation.Room.ProductId;
			reservationRow["UserId"] = (Int64)userRow["UserId"];
			reservationRow["TargetDateTime"] = reservation.TargetDateTime;
			reservationRow["RoundNumber"] = reservation.RoundNumber;
			reservationRow["GroupSize"] = reservation.GroupSize;
			reservationRow["OrderDateTime"] = DateTime.Now;
			reservationTable.Rows.Add(reservationRow);

			foreach (var consumableItem in reservation.ConsumableItems) {
				var consumableItemRow = consumableItemTable.NewRow();
				consumableItemRow["ReservationId"] = reservationRow["ReservationId"];
				consumableItemRow["ProductId"] = consumableItem.Consumable.ProductId;
				consumableItemRow["Amount"] = consumableItem.Amount;
				consumableItemTable.Rows.Add(consumableItemRow);
			}

			return true;
		}

		private List<Reservation> ReservationRowsToList(DataRow[] reservationRows)
		{
			var reservations = new List<Reservation>();
			var rel0 = this.DataBase.Relations["Reservation-ConsumableItem"];
			var rel1 = this.DataBase.Relations["ConsumableAttribute-ConsumableItem"];
			var rel2 = this.DataBase.Relations["Product-ConsumableAttribute"];
			var rel3 = this.DataBase.Relations["RoomAttribute-Reservation"];
			var rel4 = this.DataBase.Relations["Product-RoomAttribute"];
			foreach (var reservationRow in reservationRows) {
				var consumableItemRows = reservationRow.GetChildRows(rel0);
				var reservation = new Reservation(reservationRow);
				var roomAttrRow = reservationRow.GetParentRow(rel3);
				var roomProdRow = roomAttrRow.GetParentRow(rel4);
				reservation.Room = new Room(roomProdRow, roomAttrRow);
				var consumableItems = new List<ConsumableItem>();
				foreach (var consumableItemRow in consumableItemRows) {
					var consumableItem = new ConsumableItem(consumableItemRow);
					consumableItem.Consumable = new Consumable(
						consumableItemRow.GetParentRow(rel1).GetParentRow(rel2));
					consumableItems.Add(consumableItem);
				}

				reservation.ConsumableItems = consumableItems;
				reservations.Add(reservation);
			}

			return reservations;
		}

		public Boolean TryFetchUserReservations(Guid sessionToken, MemoryStream stream)
		{
			var userRow = GetUserRow(sessionToken);
			if (userRow == null) {
				return false;
			}

			var query = $"UserId = " + (Int64)userRow["UserId"];
			var reservationRows = this.DataBase.Tables["Reservations"].Select(query);
			var orders = this.ReservationRowsToList(reservationRows);

			var rawJson = JsonSerializer.SerializeToUtf8Bytes<List<Reservation>>(orders);
			stream.Write(rawJson, 0, rawJson.Length);
			stream.Position = 0;
			if (stream.Length == 0) {
				return false;
			}

			return true;
		}

		public Boolean TryFetchReservationsBetween(Guid sessionToken, MemoryStream stream,
			DateTime dateTimeStart, DateTime dateTimeEnd)
		{
			var userRow = GetUserRow(sessionToken);
			if (userRow == null || (Role)userRow["Role"] != Role.Owner) {
				return false;
			}

			var query = $"OrderDateTime >= #{dateTimeStart}# AND OrderDateTime < #{dateTimeEnd}#";
			var reservationRows = this.DataBase.Tables["Reservations"].Select(query);
			var reservations = this.ReservationRowsToList(reservationRows);

			var rawJson = JsonSerializer.SerializeToUtf8Bytes<List<Reservation>>(reservations);
			stream.Write(rawJson, 0, rawJson.Length);
			stream.Position = 0;
			if (stream.Length == 0) {
				return false;
			}

			return true;
		}

		public Boolean TryFetchReport(Guid sessionToken, out Report report, DateTime date)
		{
			report = null;
			var userRow = this.GetUserRow(sessionToken);
			if (userRow == null || (Role)userRow["Role"] != Role.Owner) {
				return false;
			}

			report = new Report();
			var startDate = date.Date;
			var endDate = startDate + new TimeSpan(1, 0, 0, 0);
			var query = $"OrderDateTime >= #{startDate}# AND OrderDateTime < #{endDate}#";
			var reservationRows = this.DataBase.Tables["Reservations"].Select(query);
			var reservations = this.ReservationRowsToList(reservationRows);
	
			foreach (var reservation in reservations) {
				var groupSize = reservation.GroupSize;
				report.TicketsSold += groupSize;
				report.Income += reservation.Room.Price * groupSize;
				foreach (var item in reservation.ConsumableItems) {
					var amount = item.Amount;
					report.ConsumablesSold += amount;
					report.Income += item.Consumable.Price * amount;
				}
			}
			
			return true;
		}

		public Boolean TryAddReview(Guid sessionToken, Review review)
		{
			var userRow = this.GetUserRow(sessionToken);
			if (userRow == null) {
				return false;
			}

			var reviewRow = this.DataBase.Tables["Reviews"].NewRow();
			reviewRow["UserId"] = (Int64)userRow["UserId"];
			reviewRow["RoomId"] = (Int64)review.RoomId;
			reviewRow["DateTime"] = DateTime.Now;
			reviewRow["Text"] = review.Text;
			reviewRow["Rating"] = review.Rating;
			this.DataBase.Tables["Reviews"].Rows.Add(reviewRow);
			
			return true;
		}

		public Boolean TryFetchReviews(Guid sessionToken, MemoryStream stream, Room room)
		{
			var userRow = this.GetUserRow(sessionToken);
			if (userRow == null) {
				return false;
			}

			var query = $"RoomId = {room.ProductId}";
			var reviewRows = this.DataBase.Tables["Reviews"].Select(query);
			var reviews = new List<Review>();
			var rel0 = this.DataBase.Relations["User-Review"];

			foreach (var reviewRow in reviewRows) {
				var review = new Review();
				review.RoomId = room.ProductId;
				review.RoomName = room.Name;
				var autherRow = reviewRow.GetParentRow(rel0);
				review.UserName = (String)autherRow["UserName"];
				review.DateTime = (DateTime)reviewRow["DateTime"];
				review.Rating = (Int32)reviewRow["Rating"];
				reviews.Add(review);
			}

			var rawJson = JsonSerializer.SerializeToUtf8Bytes<List<Review>>(reviews);
			stream.Write(rawJson, 0, (Int32)stream.Length);
			stream.Position = 0;
			if (stream.Length == 0) {
				return false;
			}

			return true;
		}
	}
}
