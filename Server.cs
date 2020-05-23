namespace App
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Data; using System.Text;
	using System.Text.Json;

	enum Role
	{
		Owner,
		CafeManager,
		Manager,
		Consumer
	}

	class Server {
		private Dictionary<Guid, Int64> ActiveUsers;
		private DataSet DataBase;
		private String DataBaseFile;
		
		public Server()
		{
			this.DataBaseFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data.xml");
			this.ActiveUsers = new Dictionary<Guid, Int64>();
			this.DataBase = new DataSet("DataBase");
			// table for users
			var userTable = new DataTable("Users");
			userTable.Columns.AddRange(new DataColumn[] {
				new DataColumn("UserId", typeof(Int64)) { AutoIncrement = true },
				new DataColumn("UserName", typeof(String)),
				new DataColumn("Forename", typeof(String)),
				new DataColumn("Surname", typeof(String)),
				new DataColumn("Email", typeof(String)) { Unique = true },
				new DataColumn("Password", typeof(String)),
				new DataColumn("Role", typeof(Int32))
			});
			userTable.PrimaryKey = new DataColumn[] { userTable.Columns["UserId"] };

			var productTable = new DataTable("Products");
			productTable.Columns.AddRange(new DataColumn[] {
				new DataColumn("ProductId", typeof(Int64)) { AutoIncrement = true },
				new DataColumn("ProductName", typeof(String)) { Unique = true },
				new DataColumn("Description", typeof(String)),
				new DataColumn("Price", typeof(Single)),
				new DataColumn("Available", typeof(Boolean))
			});
			productTable.PrimaryKey = new DataColumn[] { productTable.Columns["ProductId"] };

			var roomAttrTable = new DataTable("RoomAttrs");
			roomAttrTable.Columns.AddRange(new DataColumn[] {
				new DataColumn("ProductId", typeof(Int64)),
				new DataColumn("Theme", typeof(String)),
				new DataColumn("Capacity", typeof(Int32)),
				new DataColumn("NumberOfRounds", typeof(Int32)),
				new DataColumn("MaxDuration", typeof(Int32))
			});
			roomAttrTable.PrimaryKey = new DataColumn[] { roomAttrTable.Columns["ProductId"] };

			var consumableAttrTable = new DataTable("ConsumableAttrs");
			consumableAttrTable.Columns.AddRange(new DataColumn[] {
				new DataColumn("ProductId", typeof(Int64))
			});
			consumableAttrTable.PrimaryKey = new DataColumn[] {
				consumableAttrTable.Columns["ProductId"]
			};

			var reservationTable = new DataTable("Reservations");
			reservationTable.Columns.AddRange(new DataColumn[] {
				new DataColumn("ReservationId", typeof(Int64)) {
					AutoIncrement = true
				},
				new DataColumn("UserId", typeof(Int64)),
				new DataColumn("RoomId", typeof(Int64)),
				new DataColumn("RoundNumber", typeof(Int32)),
				new DataColumn("GroupSize", typeof(Int32)),
				new DataColumn("TargetDateTime", typeof(DateTime)),
				new DataColumn("OrderDateTime", typeof(DateTime))
			});
			reservationTable.PrimaryKey = new DataColumn[] {
				reservationTable.Columns["ReservationId"]
			};

			var consumableItemTable = new DataTable("ConsumableItems");
			consumableItemTable.Columns.AddRange(new DataColumn[] {
				new DataColumn("ConsumableItemId", typeof(Int64)) { AutoIncrement = true },
				new DataColumn("ReservationId", typeof(Int64)),
				new DataColumn("ProductId", typeof(Int64)),
				new DataColumn("Amount", typeof(Int32))
			});
			consumableItemTable.PrimaryKey = new DataColumn[] {
				consumableItemTable.Columns["ConsumableItemId"]
			};

			var reviewTable = new DataTable("Reviews");
			reviewTable.Columns.AddRange(new DataColumn[] {
				new DataColumn("ReviewId", typeof(Int64)) { AutoIncrement = true },
				new DataColumn("UserId", typeof(Int64)),
				new DataColumn("RoomId", typeof(Int64)),
				new DataColumn("Rating", typeof(Int32)),
				new DataColumn("DateTime", typeof(DateTime)),
				new DataColumn("Text", typeof(String))
			});
			reviewTable.PrimaryKey = new DataColumn[] { reviewTable.Columns["ReviewId"] };

			DataBase.Tables.AddRange(new DataTable[] { userTable, productTable, roomAttrTable, 
				reservationTable, consumableAttrTable,consumableItemTable, reviewTable
			});

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
					reviewTable.Columns["UserId"])
			});
		}

		public void LoadData()
		{
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
			var user_rows = this.DataBase.Tables["Users"].Select(query);
			if (user_rows.Length == 0)
				return null;
			else 
				return user_rows[0];
		}

		private DataRow GetUserRow(Guid session_token)
		{
			Int64 userId;
			if (!this.ActiveUsers.TryGetValue(session_token, out userId))
				return null;
			else
				return this.DataBase.Tables["Users"].Rows.Find(userId);
		}

		public Boolean TryLogin(String userName, String password, out User user)
		{
			user = null;
			var user_row = this.GetUserRow(userName);
			if (user_row == null || (String)user_row["Password"] != password)
				return false;
			else {
				Guid session_token = Guid.NewGuid();
				ActiveUsers.Add(session_token, (Int64)user_row["UserId"]);
				user = new User(userName, session_token, (Role)user_row["Role"]);
				return true;
			}
		}

		public Boolean TryLogout(Guid session_token)
		{
			if (!this.ActiveUsers.ContainsKey(session_token))
				return false;
			else {
				this.ActiveUsers.Remove(session_token);
				return true;
			}
		}

		public Boolean TryAddUser(String userName, String email, String password)
		{
			if (userName == "" || email == "" || password == "")
				return false;
			else {
				var query = String.Format("Email = '{0}'", email);
				var user_rows = this.DataBase.Tables["Users"].Select(query);
				if (user_rows.Length != 0)
					return false; 
				else {
					var user_row = this.DataBase.Tables["Users"].NewRow();
					user_row["UserName"] = userName;
					user_row["Email"] = email;
					user_row["Password"] = password;
					user_row["Role"] = Role.Consumer;
					this.DataBase.Tables["Users"].Rows.Add(user_row);
					return true;
				}
			}
		}

		public Boolean TryRemoveUser(Guid session_token, String password)
		{
			if (password == "")
				return false;
			else {
				var user_row = this.GetUserRow(session_token);
				if (user_row == null || (String)user_row["Password"] != password)
					return false;
				else {
					this.ActiveUsers.Remove(session_token);
					this.DataBase.Tables["Users"].Rows.Remove(user_row);
					return true;
				}
			}
		}

		public Boolean TryAddRoom(Guid session_token, Room room)
		{
			var user_row = this.GetUserRow(session_token);
			if (user_row == null || (Role)user_row["Role"] != Role.Owner)
				return false;
			else {
				var query = String.Format("ProductName = '{0}'", room.Name);
				var rows = this.DataBase.Tables["Products"].Select(query);
				if (this.DataBase.Tables["Products"].Select(query).Length != 0)
					return false;
				else {
					var product_row = DataBase.Tables["Products"].NewRow();
					product_row["ProductName"] = room.Name;
					product_row["Description"] = room.Description;
					product_row["Price"] = room.Price;
					product_row["Available"] = room.Available;
					DataBase.Tables["Products"].Rows.Add(product_row);
					var room_attr_row = DataBase.Tables["RoomAttrs"].NewRow();
					room_attr_row["ProductId"] = product_row["ProductId"];
					room_attr_row["Theme"] = room.Theme;
					room_attr_row["Capacity"] = room.Capacity;
					room_attr_row["NumberOfRounds"] = room.NumberOfRounds;
					room_attr_row["MaxDuration"] = room.MaxDuration;
					DataBase.Tables["RoomAttrs"].Rows.Add(room_attr_row);
					return true;
				}
			}
		}

		public Boolean TryRemoveRoom(Guid session_token, String roomName)
		{
			var user_row = this.GetUserRow(session_token);
			if (user_row == null || (Role)user_row["Role"] != Role.Owner)
				return false;
			else {
				var rel = this.DataBase.Relations["Product-RoomAttr"];
				var query = String.Format("ProductName = '{0}'", roomName);
				var roomRow = this.DataBase.Tables["Products"].Select(query);
				if (roomRow.Length < 1)
					return false;
				else {
					DataBase.Tables["RoomAttrs"].Rows.Remove(roomRow[0].GetChildRows(rel)[0]);
					DataBase.Tables["Products"].Rows.Remove(roomRow[0]);
					return true;
				}
			}
		}

		public Boolean TryEditRoom(Guid session_token, Room room)
		{
			var user_row = GetUserRow(session_token);
			if (user_row == null || (Role)user_row["Role"] != Role.Owner)
				return false;
			else {
				var rel = this.DataBase.Relations["Product-RoomAttr"];
				var roomAttrRow = rel.ChildTable.Rows.Find(room.ProductId);
				if (roomAttrRow == null)
					return false;
				else {
					var productRow = roomAttrRow.GetParentRow(rel);
					if (productRow == null)
						return false;
					else {
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
				}
			}
		}

		public Boolean TryFetchRooms(Guid session_token, MemoryStream stream)
		{
			var user_row = this.GetUserRow(session_token);
			if (user_row == null)
				return false;
			else {
				var rel = this.DataBase.Relations["Product-RoomAttr"];
				var roomAttrTable = rel.ChildTable;
				var rooms = new List<Room>();
				foreach (DataRow roomAttrRow in roomAttrTable.Rows) {
					var prodRow = roomAttrRow.GetParentRow(rel);
					rooms.Add(new Room(prodRow, roomAttrRow));
				}
				var rawJson = JsonSerializer.SerializeToUtf8Bytes<List<Room>>(rooms);
				stream.Write(rawJson, 0, rawJson.Length);
				stream.Position = 0;
				if (stream.Length == 0)
					return false;
				else
					return true;
			}
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

		public Boolean TryAddConsumable(Guid session_token, Consumable consumable)
		{
			var user_row = this.GetUserRow(session_token);
			if (user_row == null || (Role)user_row["Role"] != Role.CafeManager)
				return false;
			else {
				var query = String.Format("ProductName = '{0}'", consumable.Name);
				var productRows = this.DataBase.Tables["Products"].Select(query);
				if (productRows.Length != 0)
					return false;
				else {
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
			}
		}

		public Boolean TryRemoveConsumable(Guid session_token, Consumable consumable)
		{
			var user_row = GetUserRow(session_token);
			if (user_row == null || (Role)user_row["Role"] != Role.CafeManager)
				return false;
			else {
				var productRow = this.DataBase.Tables["Products"].Rows.Find(consumable.ProductId);
				if (productRow == null)
					return false;
				else {
					this.DataBase.Tables["Products"].Rows.Remove(productRow);
					return true;
				}
			}
		}

		public Boolean TryEditConsumable(Guid session_token, Consumable consumable)
		{
			var user_row = this.GetUserRow(session_token);
			if (user_row == null || (Role)user_row["Role"] != Role.CafeManager)
				return false;
			else {
				var rel = this.DataBase.Relations["Product-ConsumableAttr"];	
				var consumableRow = rel.ChildTable.Rows.Find(consumable.ProductId);
				var productRow = consumableRow.GetParentRow(rel);
				productRow["ProductName"] = consumable.Name;
				productRow["Description"] = consumable.Description;
				productRow["Price"] = consumable.Price;
				productRow["Available"] = consumable.Available;
				return true;
			}
		}

		public Boolean TryFetchConsumables(Guid session_token, MemoryStream stream)
		{
			var user_row = this.GetUserRow(session_token);
			if (user_row == null)
				return false;
			else {
				var rel = this.DataBase.Relations["Product-ConsumableAttr"];
				var productTable = rel.ParentTable;
				var consumableAttrTable = rel.ChildTable;
				var consumables = new List<Consumable>();
				foreach (DataRow consumableRow in consumableAttrTable.Rows)
					consumables.Add(new Consumable(consumableRow.GetParentRow(rel)));
				var rawJson = JsonSerializer.SerializeToUtf8Bytes<List<Consumable>>(consumables);
				stream.Write(rawJson, 0, rawJson.Length);
				stream.Position = 0;
				if (stream.Length == 0)
					return false;
				else
					return true;
			}
		}

		public Boolean TryPay(Guid session_token, MemoryStream stream) 
		{
			var user_row = this.GetUserRow(session_token);
			if (user_row == null)
				return false;
			else {
				var reservation = JsonSerializer.Deserialize<Reservation>(stream.ToArray());
				if (reservation == null || reservation.Room == null)
					return false; 
				else {
					var rel = this.DataBase.Relations["Reservation-ConsumableItem"];
					var reservation_table = rel.ParentTable;
					var consumable_item_table = rel.ChildTable;
					var reservation_row = reservation_table.NewRow();
					reservation_row["RoomId"] = reservation.Room.ProductId;
					reservation_row["UserId"] = user_row["UserId"];
					reservation_row["TargetDateTime"] = reservation.TargetDateTime;
					reservation_row["RoundNumber"] = reservation.RoundNumber;
					reservation_row["GroupSize"] = reservation.GroupSize;
					reservation_row["OrderDateTime"] = DateTime.Now;
					reservation_table.Rows.Add(reservation_row);
					foreach (var consumable_item in reservation.ConsumableItems) {
						var consumable_item_row = consumable_item_table.NewRow();
						consumable_item_row["ReservationId"] = reservation_row["ReservationId"];
						consumable_item_row["ProductId"] = consumable_item.Consumable.ProductId;
						consumable_item_row["Amount"] = consumable_item.Amount;
						consumable_item_table.Rows.Add(consumable_item_row);
					}
					return true;
				}
			}
		}

		private List<Reservation> ReservationRowsToList(DataRow[] res_rows)
		{
			var ress = new List<Reservation>();
			var res_to_cons_item = this.DataBase.Relations["Reservation-ConsumableItem"];
			var cons_attr_to_cons_item = this.DataBase.Relations["ConsumableAttr-ConsumableItem"];
			var prod_to_cons_attr = this.DataBase.Relations["Product-ConsumableAttr"];
			var room_attr_to_res = this.DataBase.Relations["RoomAttr-Reservation"];
			var prod_to_room_attr = this.DataBase.Relations["Product-RoomAttr"];
			foreach (DataRow res_row in res_rows) {
				var res = new Reservation(res_row);
				var cons_item_rows = res_row.GetChildRows(res_to_cons_item);
				var room_attr_row = res_row.GetParentRow(room_attr_to_res);
				var room_prod_row = room_attr_row.GetParentRow(prod_to_room_attr);
				res.Room = new Room(room_prod_row, room_attr_row);
				var cons_items = new List<ConsumableItem>();
				foreach (var cons_item_row in cons_item_rows) {
					var cons_item = new ConsumableItem(cons_item_row);
					var cons_prod_row = cons_item_row
						.GetParentRow(cons_attr_to_cons_item)
						.GetParentRow(prod_to_cons_attr);
					cons_item.Consumable = new Consumable(cons_prod_row);
					cons_items.Add(cons_item);
				}
				res.ConsumableItems = cons_items;
				ress.Add(res);
			}
			return ress;
		}

		public Boolean TryFetchUserReservations(Guid session_token, MemoryStream stream)
		{
			var user_row = GetUserRow(session_token);
			if (user_row == null)
				return false;
			else {
				var query = String.Format("UserId = {0}", (Int64)user_row["UserId"]);
				var reservation_rows = this.DataBase.Tables["Reservations"].Select(query);
				var reservations = this.ReservationRowsToList(reservation_rows);
				var json_bytes = JsonSerializer
					.SerializeToUtf8Bytes<List<Reservation>>(reservations);
				stream.Write(json_bytes, 0, json_bytes.Length);
				stream.Position = 0;
				if (stream.Length == 0)
					return false;
				else
					return true;
			}
		}

		public Boolean TryFetchReservationsBetween(Guid session_token, MemoryStream stream,
			DateTime dateTimeStart, DateTime dateTimeEnd)
		{
			var user_row = GetUserRow(session_token);
			if (user_row == null || (Role)user_row["Role"] != Role.Owner)
				return false;
			else {
				var query = String.Format("OrderDateTime >= #{0}# AND OrderDateTime < #{1}#",
					dateTimeStart, dateTimeEnd);
				var reservation_rows = this.DataBase.Tables["Reservations"].Select(query);
				var reservations = this.ReservationRowsToList(reservation_rows);
				var json_bytes = JsonSerializer.SerializeToUtf8Bytes<List<Reservation>>(reservations);
				stream.Write(json_bytes, 0, json_bytes.Length);
				stream.Position = 0;
				if (stream.Length == 0)
					return false;
				else
					return true;
			}
		}

		public Boolean TryFetchReport(Guid session_token, out Report report, DateTime date)
		{
			report = null;
			var user_row = this.GetUserRow(session_token);
			if (user_row == null || (Role)user_row["Role"] != Role.Owner)
				return false;
			else {
				String end_date = date.Date.AddDays(1.0).ToString("O");
				String start_date = date.Date.ToString("O");
				var query = String.Format("OrderDateTime >= #{0}# AND OrderDateTime < #{1}#",
					start_date, end_date);
				var reservation_rows = this.DataBase.Tables["Reservations"].Select(query);
				var reservations = this.ReservationRowsToList(reservation_rows);
				Int32 tickets_sold = 0, consumables_sold = 0;
				Single income = 0;
				foreach (Reservation reservation in reservations) {
					tickets_sold += reservation.GroupSize;
					income += reservation.Room.Price * reservation.GroupSize;
					foreach (ConsumableItem item in reservation.ConsumableItems) {
						consumables_sold += item.Amount;
						income += item.Consumable.Price * item.Amount;
					}
				}
				report = new Report(tickets_sold, consumables_sold, income);
				return true;
			}
		}

		public Boolean TryAddReview(Guid session_token, Review review)
		{
			var user_row = this.GetUserRow(session_token);
			if (user_row == null)
				return false;
			else {
				var review_table = this.DataBase.Tables["Reviews"];
				var query = String.Format("ProductId = {0}", review.RoomId);
				var review_rows = this.DataBase.Tables["RoomAttrs"].Select(query);
				if (review_rows.Length == 0)
					return false;
				else {
					var review_row = review_table.NewRow();
					review_row["UserId"] = user_row["UserId"];
					review_row["RoomId"] = review.RoomId;
					review_row["DateTime"] = DateTime.Now;
					review_row["Text"] = review.Text;
					review_row["Rating"] = review.Rating;
					review_table.Rows.Add(review_row);
					return true;
				}
			}
		}

		public Boolean TryFetchReviews(Guid session_token, MemoryStream stream, Room room)
		{
			var user_row = this.GetUserRow(session_token);
			if (user_row == null)
				return false;
			else {
				var query = String.Format("RoomId = {0}", room.ProductId);
				var review_rows = this.DataBase.Tables["Reviews"].Select(query);
				var reviews = new List<Review>();
				var rel0 = this.DataBase.Relations["User-Review"];
				foreach (var review_row in review_rows) {
					var auther_row = review_row.GetParentRow(rel0);
					reviews.Add(new Review() {
						RoomId = room.ProductId,
						RoomName = room.Name,
						UserName = (String)auther_row["UserName"],
						DateTime = (DateTime)review_row["UserName"],
						Text = (String)review_row["Text"],
						Rating = (Int32)review_row["Rating"]
					});
				}
				var json_bytes = JsonSerializer.SerializeToUtf8Bytes<List<Review>>(reviews);
				stream.Write(json_bytes, 0, json_bytes.Length);
				stream.Position = 0;
				if (stream.Length == 0)
					return false;
				else
					return true;
			}
		}
	}
}
