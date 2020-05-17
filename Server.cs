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
	private String DataBaseFile;
	
	public Server()
	{
		DataRelation rel;
		this.DataBaseFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data.xml");
		DataBase = new DataSet("DataBase");
		// table for users
		var userTable = new DataTable("Users");
		userTable.Columns.AddRange(new DataColumn[] {
			new DataColumn("UserId", typeof(Int64)) { AutoIncrement = true },
			new DataColumn("UserName", typeof(String)),
			new DataColumn("Forename", typeof(String)),
			new DataColumn("Surname", typeof(String)),
			new DataColumn("Email", typeof(String)) { Unique = true },
			new DataColumn("Password", typeof(String)),
			new DataColumn("Role", typeof(Int32)) });
		userTable.PrimaryKey = new DataColumn[] { userTable.Columns["UserId"] };
		// table for products
		var productTable = new DataTable("Products");
		productTable.Columns.AddRange(new DataColumn[] {
			new DataColumn("ProductId", typeof(Int64)) { AutoIncrement = true },
			new DataColumn("ProductName", typeof(String)) { Unique = true },
			new DataColumn("Description", typeof(String)),
			new DataColumn("Price", typeof(Single)),
			new DataColumn("Available", typeof(Boolean)) });
		productTable.PrimaryKey = new DataColumn[] { productTable.Columns["ProductId"] };
		// table for room-attributes
		var roomAttrTable = new DataTable("RoomAttrs");
		roomAttrTable.Columns.AddRange(new DataColumn[] {
			new DataColumn("ProductId", typeof(Int64)),
			new DataColumn("Theme", typeof(String)),
			new DataColumn("Capacity", typeof(Int32)),
			new DataColumn("NumberOfRounds", typeof(Int32)),
			new DataColumn("MaxDuration", typeof(Int32)) });
		roomAttrTable.PrimaryKey = new DataColumn[] { roomAttrTable.Columns["ProductId"] };
		// table for consumable-attributes
		var consumableAttrTable = new DataTable("ConsumableAttrs");
		consumableAttrTable.Columns.AddRange(new DataColumn[] {
			new DataColumn("ProductId", typeof(Int64)) });
		consumableAttrTable.PrimaryKey = new DataColumn[] {
			consumableAttrTable.Columns["ProductId"] };
		// table for reservations
		var reservationTable = new DataTable("Reservations");
		reservationTable.Columns.AddRange(new DataColumn[] {
			new DataColumn("ReservationId", typeof(Int64)) {
				AutoIncrement = true },
			new DataColumn("UserId", typeof(Int64)),
			new DataColumn("RoomId", typeof(Int64)),
			new DataColumn("RoundNumber", typeof(Int32)),
			new DataColumn("GroupSize", typeof(Int32)),
			new DataColumn("TargetDateTime", typeof(DateTime)),
			new DataColumn("OrderDateTime", typeof(DateTime)) });
		reservationTable.PrimaryKey = new DataColumn[] {
			reservationTable.Columns["ReservationId"] };
		// table for reservation consumables
		var consumableItemTable = new DataTable("ConsumableItems");
		consumableItemTable.Columns.AddRange(new DataColumn[] {
			new DataColumn("ConsumableItemId", typeof(Int64)) { AutoIncrement = true },
			new DataColumn("ReservationId", typeof(Int64)),
			new DataColumn("ProductId", typeof(Int64)),
			new DataColumn("Amount", typeof(Int32)) });
		consumableItemTable.PrimaryKey = new DataColumn[] {
			consumableItemTable.Columns["ConsumableItemId"] };
		// table for reviews
		var reviewTable = new DataTable("Reviews");
		reviewTable.Columns.AddRange(new DataColumn[] {
			new DataColumn("ReviewId", typeof(Int64)) { AutoIncrement = true },
			new DataColumn("UserId", typeof(Int64)),
			new DataColumn("RoomId", typeof(Int64)),
			new DataColumn("Rating", typeof(Int32)),
			new DataColumn("DateTime", typeof(DateTime)),
			new DataColumn("Text", typeof(String)) });
		reviewTable.PrimaryKey = new DataColumn[] { reviewTable.Columns["ReviewId"] };
		// add all the tables to the dataset
		DataBase.Tables.AddRange(new DataTable[] { userTable, productTable, roomAttrTable, 
			reservationTable, consumableAttrTable,consumableItemTable, reviewTable });
		// making the relations between the tables
		DataBase.Relations.AddRange(new DataRelation[] {
			new DataRelation("Product-RoomAttr", productTable.Columns["ProductId"],
				roomAttrTable.Columns["ProductId"]),
			new DataRelation("Product-ConsumableAttr",
				productTable.Columns["ProductId"],
				consumableAttrTable.Columns["ProductId"]),
			new DataRelation("Reservation-ConsumableItem",
				reservationTable.Columns["ReservationId"],
				consumableItemTable.Columns["ReservationId"]),
			new DataRelation("ConsumableAttr-ConsumableItem",
				consumableAttrTable.Columns["ProductId"],
				consumableItemTable.Columns["ProductId"]),
			new DataRelation("RoomAttr-Reservation", roomAttrTable.Columns["ProductId"],
				reservationTable.Columns["RoomId"]),
			new DataRelation("User-Review", userTable.Columns["UserId"],
				reviewTable.Columns["UserId"]) });
		this.ActiveUsers = new Dictionary<Guid, Int64>();
	}

	public void LoadData()
	{
		Console.WriteLine(this.DataBaseFile);
		if (File.Exists(this.DataBaseFile))
			DataBase.ReadXml(this.DataBaseFile);
	}

	public void SaveData()
	{
		if (!File.Exists(this.DataBaseFile))
			File.Create(this.DataBaseFile).Close();
		DataBase.WriteXml(this.DataBaseFile);
	}

	private DataRow GetUserRow(String email)
	{
		var query = String.Format("Email = '{0}'", email);
		var userRows = this.DataBase.Tables["Users"].Select(query);
		if (userRows.Length == 0)
			return null;
		return userRows[0];
	}

	private DataRow GetUserRow(Guid sessionToken)
	{
		Int64 userId;
		if (!this.ActiveUsers.TryGetValue(sessionToken, out userId))
			return null;
		return this.DataBase.Tables["Users"].Rows.Find(userId);
	}

	public Boolean TryLogin(String userName, String password, out User user)
	{
		user = null;
		var userRow = this.GetUserRow(userName);
		if (userRow == null || (String)userRow["Password"] != password)
			return false;
		Guid session_token = Guid.NewGuid();
		ActiveUsers.Add(session_token, (Int64)userRow["UserId"]);
		user = new User(userName, session_token, (Role)userRow["Role"]);
		return true;
	}

	public Boolean TryLogout(Guid sessionToken)
	{
		if (!this.ActiveUsers.ContainsKey(sessionToken))
			return false;
		this.ActiveUsers.Remove(sessionToken);
		return true;
	}

	public Boolean TryAddUser(String userName, String email, String password)
	{
		if (userName == "" || email == "" || password == "")
			return false;
		var query = String.Format("Email = '{0}'", email);
		var userRows = this.DataBase.Tables["Users"].Select(query);
		if (userRows.Length != 0)
			return false; 
		var userRow = this.DataBase.Tables["Users"].NewRow();
		userRow["UserName"] = userName;
		userRow["Email"] = email;
		userRow["Password"] = password;
		userRow["Role"] = Role.Consumer;
		this.DataBase.Tables["Users"].Rows.Add(userRow);
		return true;
	}

	public Boolean TryRemoveUser(Guid sessionToken, String password)
	{
		if (password == "")
			return false;
		var userRow = this.GetUserRow(sessionToken);
		if (userRow == null || (String)userRow["Password"] != password)
			return false;
		this.ActiveUsers.Remove(sessionToken);
		this.DataBase.Tables["Users"].Rows.Remove(userRow);
		return true;
	}

	public Boolean TryAddRoom(Guid sessionToken, Room room)
	{
		var userRow = this.GetUserRow(sessionToken);
		if (userRow == null || (Role)userRow["Role"] != Role.Owner)
			return false;
		var query = String.Format("ProductName = '{0}'", room.Name);
		var rows = this.DataBase.Tables["Products"].Select(query);
		if (rows.Length != 0)
			return false;
		var productRow = DataBase.Tables["Products"].NewRow();
		productRow["ProductName"] = room.Name;
		productRow["Description"] = room.Description;
		productRow["Price"] = room.Price;
		productRow["Available"] = room.Available;
		DataBase.Tables["Products"].Rows.Add(productRow);
		var roomAttrRow = DataBase.Tables["RoomAttrs"].NewRow();
		roomAttrRow["ProductId"] = productRow["ProductId"];
		roomAttrRow["Theme"] = room.Theme;
		roomAttrRow["Capacity"] = room.Capacity;
		roomAttrRow["NumberOfRounds"] = room.NumberOfRounds;
		roomAttrRow["MaxDuration"] = room.MaxDuration;
		DataBase.Tables["RoomAttrs"].Rows.Add(roomAttrRow);
		return true;
	}

	public Boolean TryRemoveRoom(Guid sessionToken, Int64 productId)
	{
		var userRow = this.GetUserRow(sessionToken);
		if (userRow == null || (Role)userRow["Role"] != Role.Owner)
			return false;
		var query = String.Format("ProductId = '{0}'", productId);
		var roomRow = this.DataBase.Tables["Products"].Select(query);
		if (roomRow.Length == 0)
			return false;
		DataBase.Tables["Products"].Rows.Remove(roomRow[0]);
		DataBase.Tables["RoomAttrs"].Rows.Remove(roomRow[0]);
		return true;
	}

	public Boolean TryEditRoom(Guid sessionToken, Room room)
	{
		var userRow = GetUserRow(sessionToken);
		if (userRow == null || (Role)userRow["Role"] != Role.Owner)
			return false;
		var rel = this.DataBase.Relations["Product-RoomAttr"];
		var roomAttrRow = rel.ChildTable.Rows.Find(room.ProductId);
		if (roomAttrRow == null)
			return false;
		var productRow = roomAttrRow.GetParentRow(rel);
		if (productRow == null)
			return false;
		productRow["ProductName"] = room.Name;
		productRow["Description"] = room.Description;
		productRow["Price"] = room.Price;
		productRow["Available"] = room.Available;
		roomAttrRow["Theme"] = room.Theme;
		roomAttrRow["Capacity"] = room.Capacity;
		roomAttrRow["NumberOfRounds"] = room.NumberOfRounds;
		roomAttrRow["MaxDuration"] = room.MaxDuration;
		return true;
	}

	public Boolean TryFetchRooms(Guid sessionToken, MemoryStream stream)
	{
		var userRow = this.GetUserRow(sessionToken);
		if (userRow == null)
			return false;
		var rel = this.DataBase.Relations["Product-RoomAttr"];
		var productTable = rel.ParentTable;
		var roomAttrTable = rel.ChildTable;
		var rooms = new List<Room>();
		foreach (DataRow roomAttrRow in roomAttrTable.Rows) {
			var room = new Room();
			var productRow = roomAttrRow.GetParentRow(rel);
			room.ProductId = (Int64)productRow["ProductId"];
			room.Name = (String)productRow["ProductName"];
			room.Description = (String)productRow["Description"];
			room.Price = (Single)productRow["Price"];
			room.Available = (Boolean)productRow["Available"];
			room.Theme = (String)roomAttrRow["Theme"];
			room.Capacity = (Int32)roomAttrRow["Capacity"];
			room.NumberOfRounds = (Int32)roomAttrRow["NumberOfRounds"];
			room.MaxDuration = (Int32)roomAttrRow["MaxDuration"];
			rooms.Add(room);
		}
		var rawJson = JsonSerializer.SerializeToUtf8Bytes<List<Room>>(rooms);
		stream.Write(rawJson, 0, rawJson.Length);
		stream.Position = 0;
		if (stream.Length == 0)
			return false;
		return true;
	}
	
	public Int32 CheckReservation(Reservation reservation)
	{
		var query = String.Format(
			"RoomId = {0} AND TargetDateTime = #{1}# AND RoundNumber = {2}",
			reservation.Room.ProductId, reservation.TargetDateTime,
			reservation.RoundNumber);
		var rows = this.DataBase.Tables["Reservations"].Select(query);
		Int32 n = 0;
		foreach (var row in rows)
			n += (Int32)row["GroupSize"];
		return n;
	}

	public Boolean TryAddConsumable(Guid sessionToken, Consumable consumable)
	{
		var userRow = this.GetUserRow(sessionToken);
		if (userRow == null || (Role)userRow["Role"] != Role.CafeManager)
			return false;
		var query = String.Format("ProductName = '{0}'", consumable.Name);
		var productRows = this.DataBase.Tables["Products"].Select(query);
		if (productRows.Length != 0)
			return false;
		var productRow = this.DataBase.Tables["Products"].NewRow();
		productRow["ProductName"] = consumable.Name;
		productRow["Description"] = consumable.Description;
		productRow["Price"] = consumable.Price;
		productRow["Available"] = consumable.Available;
		this.DataBase.Tables["Products"].Rows.Add(productRow);
		var consumableAttrRow = this.DataBase.Tables["ConsumableAttrs"].NewRow();
		consumableAttrRow["ProductId"] = productRow["ProductId"];
		this.DataBase.Tables["ConsumableAttrs"].Rows.Add(consumableAttrRow);
		return true;
	}

	public Boolean TryRemoveConsumable(Guid sessionToken, Consumable consumable)
	{
		var userRow = GetUserRow(sessionToken);
		if (userRow == null || (Role)userRow["Role"] != Role.CafeManager)
			return false;
		var productRow = this.DataBase.Tables["Products"].Rows.Find(consumable.ProductId);
		if (productRow == null)
			return false;
		this.DataBase.Tables["Products"].Rows.Remove(productRow);
		return true;
	}

	public Boolean TryEditConsumable(Guid sessionToken, Consumable consumable)
	{
		var userRow = this.GetUserRow(sessionToken);
		if (userRow == null || (Role)userRow["Role"] != Role.CafeManager)
			return false;
		var rel = this.DataBase.Relations["Product-ConsumableAttr"];	
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
		if (userRow == null)
			return false;
		var rel = this.DataBase.Relations["Product-ConsumableAttr"];
		var productTable = rel.ParentTable;
		var consumableAttrTable = rel.ChildTable;
		var consumables = new List<Consumable>();
		foreach (DataRow consumableRow in consumableAttrTable.Rows) {
			var productRow = consumableRow.GetParentRow(rel);
			consumables.Add(new Consumable(productRow));
		}
		var rawJson = JsonSerializer.SerializeToUtf8Bytes<List<Consumable>>(consumables);
		stream.Write(rawJson, 0, rawJson.Length);
		stream.Position = 0;
		if (stream.Length == 0)
			return false;
		return true;
	}

	public Boolean TryPay(Guid sessionToken, MemoryStream stream) 
	{
		var userRow = this.GetUserRow(sessionToken);
		if (userRow == null)
			return false;
		var reservation = JsonSerializer.Deserialize<Reservation>(stream.ToArray());
		if (reservation == null || reservation.Room == null) {
			Console.WriteLine("in func TryPay, Server side...");
			return false; 
		}
		var rel = this.DataBase.Relations["Reservation-ConsumableItem"];
		var reservationTable = rel.ParentTable;
		var consumableItemTable = rel.ChildTable;
		var reservationRow = reservationTable.NewRow();
		reservationRow["RoomId"] = reservation.Room.ProductId;
		reservationRow["UserId"] = userRow["UserId"];
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
		var rel1 = this.DataBase.Relations["ConsumableAttr-ConsumableItem"];
		var rel2 = this.DataBase.Relations["Product-ConsumableAttr"];
		var rel3 = this.DataBase.Relations["RoomAttr-Reservation"];
		var rel4 = this.DataBase.Relations["Product-RoomAttr"];
		foreach (DataRow reservationRow in reservationRows) {
			var consumableItemRows = reservationRow.GetChildRows(rel0);
			var reservation = new Reservation(reservationRow);
			var roomAttrRow = reservationRow.GetParentRow(rel3);
			var roomProdRow = roomAttrRow.GetParentRow(rel4);
			reservation.Room = new Room(roomProdRow, roomAttrRow);
			var consumableItems = new List<ConsumableItem>();
			foreach (var consumableItemRow in consumableItemRows) {
				var consumableItem = new ConsumableItem(consumableItemRow);
				var prodRow = consumableItemRow
					.GetParentRow(rel1)
					.GetParentRow(rel2);
				consumableItem.Consumable = new Consumable(prodRow);
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
		if (userRow == null)
			return false;
		var query = String.Format("UserId = {0}", (Int64)userRow["UserId"]);
		var reservationRows = this.DataBase.Tables["Reservations"].Select(query);
		var orders = this.ReservationRowsToList(reservationRows);
		var rawJson = JsonSerializer.SerializeToUtf8Bytes<List<Reservation>>(orders);
		stream.Write(rawJson, 0, rawJson.Length);
		stream.Position = 0;
		if (stream.Length == 0)
			return false;
		return true;
	}

	public Boolean TryFetchReservationsBetween(Guid sessionToken, MemoryStream stream,
		DateTime dateTimeStart, DateTime dateTimeEnd)
	{
		var userRow = GetUserRow(sessionToken);
		if (userRow == null || (Role)userRow["Role"] != Role.Owner)
			return false;
		var query = String.Format("OrderDateTime >= #{0}# AND OrderDateTime < #{1}#",
			dateTimeStart, dateTimeEnd);
		var reservationRows = this.DataBase.Tables["Reservations"].Select(query);
		var reservations = this.ReservationRowsToList(reservationRows);
		var rawJson = JsonSerializer.SerializeToUtf8Bytes<List<Reservation>>(reservations);
		stream.Write(rawJson, 0, rawJson.Length);
		stream.Position = 0;
		if (stream.Length == 0)
			return false;
		return true;
	}

	public Boolean TryFetchReport(Guid sessionToken, out Report report, DateTime date)
	{
		report = null;
		var userRow = this.GetUserRow(sessionToken);
		if (userRow == null || (Role)userRow["Role"] != Role.Owner)
			return false;
		String endDate = date.Date.AddDays(1.0).ToString("O");
		String startDate = date.Date.ToString("O");
		var query = String.Format("OrderDateTime >= #{0}# AND OrderDateTime < #{1}#",
			startDate, endDate);
		var reservationRows = this.DataBase.Tables["Reservations"].Select(query);
		var reservations = this.ReservationRowsToList(reservationRows);
		Int32 ticketsSold = 0, consumablesSold = 0;
		Single income = 0;
		foreach (var reservation in reservations) {
			ticketsSold += reservation.GroupSize;
			income += reservation.Room.Price * reservation.GroupSize;
			foreach (var item in reservation.ConsumableItems) {
				consumablesSold += item.Amount;
				income += item.Consumable.Price * item.Amount;
			}
		}
		report = new Report(ticketsSold, consumablesSold, income);
		return true;
	}

	public Boolean TryAddReview(Guid sessionToken, Review review)
	{
		var userRow = this.GetUserRow(sessionToken);
		if (userRow == null)
			return false;
		var reviewTable = this.DataBase.Tables["Reviews"];
		var query = String.Format("ProductId = {0}", review.RoomId);
		var rows = this.DataBase.Tables["RoomAttrs"].Select(query);
		if (rows.Length == 0)
			return false;
		var reviewRow = reviewTable.NewRow();
		reviewRow["UserId"] = userRow["UserId"];
		reviewRow["RoomId"] = review.RoomId;
		reviewRow["DateTime"] = DateTime.Now;
		reviewRow["Text"] = review.Text;
		reviewRow["Rating"] = review.Rating;
		reviewTable.Rows.Add(reviewRow);
		return true;
	}

	public Boolean TryFetchReviews(Guid sessionToken, MemoryStream stream, Room room)
	{
		var userRow = this.GetUserRow(sessionToken);
		if (userRow == null)
			return false;
		var query = String.Format("RoomId = {0}", room.ProductId);
		var reviewRows = this.DataBase.Tables["Reviews"].Select(query);
		var reviews = new List<Review>();
		var rel0 = this.DataBase.Relations["User-Review"];
		foreach (var reviewRow in reviewRows) {
			var autherRow = reviewRow.GetParentRow(rel0);
			reviews.Add(new Review() { RoomId = room.ProductId, RoomName = room.Name,
				UserName = (String)autherRow["UserName"],
				DateTime = (DateTime)reviewRow["UserName"],
				Text = (String)reviewRow["Text"],
				Rating = (Int32)reviewRow["Rating"] });
		}
		var rawJson = JsonSerializer.SerializeToUtf8Bytes<List<Review>>(reviews);
		stream.Write(rawJson, 0, rawJson.Length);
		stream.Position = 0;
		if (stream.Length == 0)
			return false;
		return true;
	}
}
} // namespace App
