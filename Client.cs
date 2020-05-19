namespace App
{
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Globalization;

class Client
{
	private Boolean Stop;
	private User CurrentUser;
	private Server Server;
	private List<Room> Rooms;
	private List<Reservation> Reservations;
	private List<Review> Reviews;
	private List<Consumable> Consumables;
	private Reservation Basket;

	//private Dictionary<String, Command> Commands;
	//public delegate void Command();

	public Client()
	{
		CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
		this.Stop = false;
		this.Basket = new Reservation() { ConsumableItems = new List<ConsumableItem>() };
	}

	public void Begin(Server server)
	{
		String n;
		Console.Write("Go to profile to login and registering\n\n");

		Server = server;
		while (!Stop) {
			Console.Write("[1] go to profile\n"
				+ "[2] go to rooms\n"
				+ "[3] go to consumables\n"
				+ "[4] go to reservations\n"
				+ "[5] go to reviews\n"
				+ "[6] go to reports\n"
				+ "[7] pay\n"
				+ "[0] exit\n");
			n = ReadField("> ").ToLower();
			switch (n) {
			case "0": this.Exit(); break;
			case "1": this.Profile(); break;
			case "2": this.Room(); break;
			case "3": this.Consumable(); break;
			case "4": this.Reservation(); break;
			case "5": this.Review(); break;
			case "6": this.Report(); break;
			case "7": this.Payment(); break;
			default: Console.Write("Invalid command number\n"); break;
			}
			Block();
		}
		Server = null;
		return;
	}

	private static String ReadField(String field_name)
	{
		Console.Write(field_name);
		return Console.ReadLine().Trim();
	}

	private static void Block()
	{
		Console.Write("\n>> Press enter to continue ");
		Console.ReadLine();
		Console.Write("\n");
	}

	private void Profile()
	{
		String n;
		while (true) {
			Console.Write("[1] login\n"
				+ "[2] logout\n"
				+ "[3] register\n"
				+ "[4] deregister\n"
				+ "[0] return\n");
			n = ReadField("> ");
			switch (n) {
			case "0": return;
			case "1": this.Login(); break;
			case "2": this.Logout(); break;
			case "3": this.Register(); break;
			case "4": this.Deregister(); break;
			default: Console.Write("Invalid command number\n"); break;
			}
			Block();
		}
	}
	
	private void Login()
	{
		String email, password;

		if (CurrentUser != null) {
			Console.Write("You are already logged in\n");
			return;
		}
		email = ReadField("email: ");
		password = ReadField("password: ");
		if (email == "" || password == "") {
			Console.WriteLine("Leave no field empty!");
		} else if (!Server.TryLogin(email, password, out CurrentUser)) {
			Console.Write("Wrong email or password\n");
		} else {
			Console.Write("Login successful\n");
		}
	}

	private void Logout()
	{
		if (CurrentUser == null) {
			Console.Write("You must be logged in to logout\n");
		} else if (!Server.TryLogout(CurrentUser.SessionToken)) {
			Console.Write("Something went wrong\n");
		} else {
			Console.Write("Logout successful\n");
			CurrentUser = null;
		}
	}

	private void Register()
	{
		String username, email, password;

		if (CurrentUser != null) {
			Console.Write("Unable to register when logged in\n");
			return;
		}

		username = ReadField("username: ");
		email = ReadField("email: ");
		password = ReadField("password: ");
		if (username == "" || password == "" || email == "")
			Console.Write("Leave no field empty!\n");
		else if (!Server.TryAddUser(username, email, password))
			Console.Write("The username is already in use\n");
		else
			Console.Write("Registration successful\n");
	}

	private void Deregister()
	{
		String password;

		if (CurrentUser == null) {
			Console.Write("You must be logged in to deregister\n");
			return;
		}

		password = ReadField("password: ");
		if (password == "")
			Console.Write("Leave no field empty!\n");
		else if (!Server.TryRemoveUser(CurrentUser.SessionToken, password))
			Console.Write("Something went wrong, please try again\n");
		else
			this.CurrentUser = null;
	}

	private void Payment()
	{
		if (CurrentUser == null)
			return;
		if (Basket == null)
			return;
		MemoryStream stream = new MemoryStream();
		var pay_json = JsonSerializer.SerializeToUtf8Bytes<Reservation>(Basket);
		stream.Write(pay_json, 0, pay_json.Length);
		if (!Server.TryPay(CurrentUser.SessionToken, stream))
			Console.WriteLine("Unsuccessful payment, Please try again");
		else {
			Console.WriteLine("Payment succeed");
			Basket = new Reservation() { ConsumableItems = new List<ConsumableItem>() };
		}
	}

	private void Exit()
	{
		if (CurrentUser != null)
			this.Logout();
		this.Stop = true;
	}

	// ROOMS

	private void Room()
	{
		String n;
		if (CurrentUser == null) {
			Console.Write("You need to be logged in\n");
			return;
		}
		while (true) {
			Console.Write("[1] view rooms\n" 
				+ "[2] select room\n" 
				+ "[3] view selected room\n" 
				+ "[4] edit selected room\n" 
				+ "[5] create room\n"
				+ "[6] remove room\n"
				+ "[7] edit room\n"
				+ "[0] return\n");
			n = ReadField("> ");
			this.FetchRooms();
			switch (n) {
			case "0": return;
			case "1": this.ViewRooms(); break;
			case "2": this.SelectRoom(); break;
			case "3": this.ViewSelectedRoom(); break;
			case "4": this.EditSelectedRoom(); break;
			case "5": this.MakeRoom(); break;
			case "6": this.RemoveRoom(); break;
			case "7": this.EditRoom(); break;
			default: Console.Write("Invalid command number\n"); break;
			}
			Block();
		}
	}

	private void FetchRooms()
	{
		MemoryStream stream = new MemoryStream();
		if (!Server.TryFetchRooms(CurrentUser.SessionToken, stream)) {
			Console.Write("Something went wrong while trying to get the rooms data"
			                  + "from the server\n");
		} else {
			this.Rooms = JsonSerializer.Deserialize<List<Room>>(stream.ToArray());
		}
		stream.Close();
	}
	
	private void ViewRooms()
	{
		foreach(Room room in this.Rooms)
			Console.Write("\nName: {0}\nTheme: {1}\nDescription: {2}\n"
				+ "Capacity: {3}\nMaximum duration: {5}\nNumber of rounds: {4}\n"
				+ "Price: {6}\n", room.Name, room.Theme, room.Description,
				room.Capacity, room.NumberOfRounds, room.MaxDuration, room.Price);
	}

	private void ViewSelectedRoom()
	{
		if (this.Basket.Room == null)
			Console.Write("\nNo room has been selected\n");
		else
			Console.Write("\nName: {0}\nGroup size: {1}\n",
				this.Basket.Room.Name, this.Basket.GroupSize);
	}

	private void SelectRoom()
	{
		DateTime date;
		String roomName;
		Reservation rsrv;
		Room room;
		Int32 freePlaces, groupSize, round;

		roomName = ReadField("Room name: ");
		room = this.Rooms.Find(room => room.Name == roomName);
		if (room == null) {
			Console.WriteLine("Invalid room name");
		} else if (!Int32.TryParse(ReadField("Group size: "), out groupSize)
				&& groupSize > 0 && groupSize <= room.Capacity) {
			Console.WriteLine("Invalid number");
		} else if (!DateTime.TryParse(ReadField("Date (YYYY-MM-DD): "), out date) 
				&& date < DateTime.Now) {
			Console.WriteLine("Invalid date");
		} else if (!Int32.TryParse(ReadField("Round: "), out round)
				&& round > 0 && round <= room.NumberOfRounds) {
			Console.WriteLine("Invalid number");
		} else {
			rsrv = new Reservation(room, date.Date, round, groupSize);
			freePlaces = room.Capacity - Server.CheckReservation(rsrv);
			if (groupSize > freePlaces) {
				Console.WriteLine("there's no enough places");
			} else {
				Console.WriteLine("Places left: {0}", freePlaces);
				if (ReadField("Confirm reservation ([Y]es or [N]o): ") == "Y")
					this.Basket = rsrv;
			}
		}
	}

	private void MakeRoom()
	{
		if (CurrentUser == null) {
			Console.WriteLine("You need to be logged in");
			return;
		}
		if (CurrentUser.Role != Role.Owner) {
			Console.WriteLine("You do not have the permissions to perform this action");
			return;
		}
		String name = ReadField("name: ");
		if (name == "") {
			Console.WriteLine("invalid name");
			return;
		}
		String theme = ReadField("theme: ");
		if (theme == "") {
			Console.WriteLine("invalid theme");
			return;
		}
		String discription = ReadField("description: ");
		if (discription == "") {
			Console.WriteLine("invalid description");
			return;
		}
		Int32 capacity;
		if (!Int32.TryParse(ReadField("capacity: "), out capacity)) {
			Console.WriteLine("invalid number");
			return;
		}
		Int32 numberofrounds;
		if (!Int32.TryParse(ReadField("Number of rounds: "), out numberofrounds)) {
			Console.WriteLine("invalid number");
			return;
		}
		Int32 maxduration;
		if (!Int32.TryParse(ReadField("Maximum Duration: "), out maxduration)) {
			Console.WriteLine("invalid number");
			return;
		}
		Single price;
		if (!Single.TryParse(ReadField("price: "), out price)) {
			Console.WriteLine("invalid price");
			return;
		}
		var room = new Room(name, theme, discription, capacity, price, numberofrounds,
			maxduration);
		Server.TryAddRoom(CurrentUser.SessionToken, room);
	}

	private void RemoveRoom()
	{
		Int64 roomId;
		if (CurrentUser.Role != Role.Owner)
			Console.Write("You do not have the permissions to perform this action");
		else if (!Int64.TryParse(ReadField("Room ID"), out roomId))
			Console.Write("That is not a valid Room ID\n");
		else
			Server.TryRemoveRoom(CurrentUser.SessionToken, roomId);
	}

	private void EditRoom()
	{
		String roomName, desc, name, theme, ava;
		Single _price;
		Int32 _capacity, _roundsAmount,  _maxDuration;
		Room room, copied;

		if (CurrentUser == null) {
			return;
		} else if (CurrentUser.Role != Role.Owner) {
			Console.WriteLine("You do not have the permissions to perform this action");
			return;
		}
		roomName = ReadField("Room name: ");
		room = this.Rooms.Find(room => room.Name == roomName);
		if (room == null) {
			Console.WriteLine("Invalid name");
			return;
		}
		copied = room.Clone();
		name = ReadField("Name: ");
		if (name != "")
			copied.Name = name;
		theme = ReadField("Theme: ");
		if (theme != "")
			copied.Theme = theme;
		desc = ReadField("Description: ");
		if (desc != "")
			copied.Description = desc;
		if (Single.TryParse(ReadField("Price: "), out _price))
			copied.Price = _price;
		else
			Console.WriteLine("Invalid number");
		ava = ReadField("Available ([Y]es or [N]o): ");
		if (ava != "")
			if (ava == "Y")
				copied.Available = true;
			else
				copied.Available = false;
		if (Int32.TryParse(ReadField("Capacity: "), out _capacity))
			copied.Capacity = _capacity;
		else
			Console.Write("Invalid number\n");
		if (Int32.TryParse(ReadField("Number of rounds: "), out _roundsAmount))
			copied.NumberOfRounds = _roundsAmount;
		else
			Console.Write("Invalid number\n");
		if (Int32.TryParse(ReadField("Maximum duration: "), out _maxDuration))
			copied.MaxDuration = _maxDuration;
		else
			Console.Write("Invalid number\n");
		this.Server.TryEditRoom(CurrentUser.SessionToken, copied);
	}

	private void EditSelectedRoom()
	{
		Int32 newGroupsize, freePlaces;
		String roomName, confirm;
		Room chosenRoom;

		roomName = ReadField("Room name: ");
		chosenRoom = this.Rooms.Find(room => room.Name == roomName);
		if (chosenRoom == null) {
			Console.Write("Invalid name\n");
			return;
		} else if (!Int32.TryParse(ReadField("New group size: "), out newGroupsize)) {
			Console.Write("invalid number\n");
		} else {
			freePlaces = chosenRoom.Capacity - Server.CheckReservation(Basket);
			if (newGroupsize > freePlaces) {
				Console.Write("there's no enough places\n");
			} else {
				Console.WriteLine("Places left: {0}", freePlaces);
				confirm = ReadField("Confirm reservation ([Y]es or [N]o): ");
				if (confirm == "Y")
					Basket.GroupSize = newGroupsize;
			}
		}
	}

	// CONSUMABLES

	private void Consumable()
	{
		String n;
		if (CurrentUser == null) {
			Console.WriteLine("You have to be logged in");
			return;
		}
		while (true) {
			Console.Write("[1] view consumables\n" 
				+ "[2] select consumable\n" 
				+ "[3] view selected consumables\n" 
				+ "[4] remove selected consumable\n" 
				+ "[5] edit selected consumable\n" 
				+ "[6] create consumable\n" 
				+ "[7] delete consumable\n" 
				+ "[8] edit consumable\n"
				+ "[0] return\n");
			n = ReadField("> ");
			this.FetchConsumables();
			switch (n) {
			case "0": return;
			case "1": this.ViewConsumables(); break;
			case "2": this.SelectConsumable(); break;
			case "3": this.ViewSelectedConsumables(); break;
			case "4": this.RemoveSelectedConsumable(); break;
			case "5": this.EditSelectedConsumable(); break;
			case "6": this.MakeConsumable(); break;
			case "7": this.RemoveConsumable(); break;
			case "8": this.EditConsumable(); break;
			default: Console.Write("Invalid command number\n"); break;
			}
			Block();
		}
	}

	private void FetchConsumables()
	{
		var stream = new MemoryStream();
		if (!Server.TryFetchConsumables(CurrentUser.SessionToken, stream))
			Console.WriteLine("Something went wrong while trying to get the product "
				+ "data from the server");
		else
			this.Consumables = JsonSerializer
				.Deserialize<List<Consumable>>(stream.ToArray());
		stream.Close();
	}

	private void ViewConsumables()
	{
		foreach (Consumable c in this.Consumables) {
			Console.Write("\nName: {0}\nDescription: {1}\nPrice: {2}\nAvailable:{3}\n",
				c.Name, c.Description, c.Price, c.Available ? "Yes" : "No");
		}
	}

	private void ViewSelectedConsumables()
	{
		foreach (ConsumableItem ci in this.Basket.ConsumableItems)
			Console.Write("\nName: {}\nAmount: {}\n", ci.Consumable.Name, ci.Amount);
	}

	private void SelectConsumable()
	{
		Int32 amount;
		if (this.CurrentUser == null) {
			Console.WriteLine("You must be logged in to select a consumbale");
		} else {
			this.FetchConsumables();
			String consumableName = ReadField("Name: ");
			var consumable = this.Consumables.Find(
				consumable => consumable.Name == consumableName);
			if (consumable == null) {
				Console.WriteLine("Invalid product name");
			} else if (!Int32.TryParse(ReadField("Amount: "), out amount)) {
				Console.WriteLine("Invalid number");
			} else {
				Basket.ConsumableItems.Add(new ConsumableItem(consumable, amount));
			}
		}
	}

	private void MakeConsumable()
	{
		if (CurrentUser == null) {
			return;
		}
		if (CurrentUser.Role != Role.CafeManager) {
			Console.WriteLine("You do not have the permissions to perform this action");
			return;
		}
		String name = ReadField("name: ");
		if (name == "") {
			Console.WriteLine("invalid name");
			return;
		}
		String discription = ReadField("description: ");
		if (discription == "") {
			Console.WriteLine("invalid description");
			return;
		}
		Single price;
		if (!Single.TryParse(ReadField("price: "), out price)) {
			Console.WriteLine("invalid price");
			return;
		}
		Boolean availability = false;
		String avb = ReadField("Available ([Y]es or [N]o): ");
		if (avb == "Y") {
			availability = true;
		}
		Server.TryAddConsumable(CurrentUser.SessionToken,
			new Consumable(name, discription, price, availability));
	}

	private void RemoveConsumable()
	{
		if (this.CurrentUser == null) {
			Console.WriteLine("You have to be logged in");
			return;
		}
		if (this.CurrentUser.Role != Role.CafeManager) {
			Console.WriteLine("You do not have the permissions to perform this action");
			return;
		}
		String consumableName = ReadField("Product name: ");
		var consumable = this.Consumables.Find(c => c.Name == consumableName);
		if (consumable == null)
			Console.WriteLine("Invalid name");
		else
			Server.TryRemoveConsumable(CurrentUser.SessionToken, consumable);
	}

	private void RemoveSelectedConsumable()
	{
		String name;
		Int32 n;
		
		name = ReadField("consumable name: ");
		n = this.Basket.ConsumableItems.RemoveAll(ci => ci.Consumable.Name == name);
		if (n < 0)
			Console.Write("You have not selected any consumable with that name");
	}

	private void EditConsumable()
	{
		if (CurrentUser == null) {
			return;
		}
		if (CurrentUser.Role != Role.CafeManager) {
			Console.WriteLine("You do not have the permissions to perform this action");
			return;
		}
		string chosenConsumable = ReadField("Product name: ");
		var consumable = this.Consumables.Find(consumable => consumable.Name == chosenConsumable);
		if (consumable == null) {
			Console.WriteLine("Invalid name");
			return;
		}
		Consumable copied = consumable.Clone();
		String name = ReadField("Name: ");
		if (name != "") {
			copied.Name = name;
		}
		String description = ReadField("Description: ");
		if (description != "") {
			copied.Description = description;
		}
		Single price;
		if (Single.TryParse(ReadField("Price: "), out price)) {
			Console.WriteLine("Invalid number");
			return;
		}
		copied.Price = price;
		String availabile = ReadField("Available ([Y]es or [N]o): ");
		if (availabile == "Y")
			copied.Available = true;
		else
			copied.Available = false;
		Server.TryEditConsumable(CurrentUser.SessionToken, copied);
	}
	
	private void EditSelectedConsumable()
	{
		Int32 newAmount;
		String consumableName = ReadField("Product name: ");
		var chosenProduct = this.Consumables.Find(
			Consumable => Consumable.Name == consumableName);
		if (chosenProduct == null) {
			Console.WriteLine("Invalid name");
		} else if (!Int32.TryParse(ReadField("New amount: "), out newAmount)) {
			Console.WriteLine("invalid number");
		} else {
			var item = this.Basket.ConsumableItems.Find(
				item => item.Consumable.ProductId == chosenProduct.ProductId);
			if (item != null)
				item.Amount = newAmount;
		}
	}

	// RESERVATION

	private void Reservation()
	{
		String n;
		if (CurrentUser == null) {
			Console.Write("You need to be logged in\n");
			return;
		}
		Console.Write(
			"[1] view all my reservations\n" +
			"[2] view reservations between\n");
		n = ReadField("> ");
		switch (n) {
		case "1": this.ViewUserReservations(); break;
		case "2": this.ViewReservationsBetween(); break;
		default: Console.WriteLine("Invalid command number"); break;
		}
		Block();
	}

	private void FetchUserReservations()
	{
		var stream = new MemoryStream();
		if (!Server.TryFetchUserReservations(CurrentUser.SessionToken, stream))
			Console.Write("Something went wrong while trying to get the Orders "
				+ "data from the server\n");
		else
			this.Reservations = JsonSerializer
				.Deserialize<List<Reservation>>(stream.ToArray());
		stream.Close();
	}

	private void FetchReservationsBetween(DateTime t1, DateTime t2)
	{
		var stream = new MemoryStream();
		if (!this.Server.TryFetchReservationsBetween(CurrentUser.SessionToken, stream, t1, t2))
			Console.Write("Something went wrong while trying to get the Orders "
				+ "data from the server\n");
		else
			this.Reservations = JsonSerializer
				.Deserialize<List<Reservation>>(stream.ToArray());
		stream.Close();
	}

	private void ViewReservationsBetween()
	{
		DateTime t1, t2;
		if (this.CurrentUser == null || this.CurrentUser.Role != Role.Owner) {
			Console.Write("You dont have permission to do this procces\n");
		} else if (!DateTime.TryParse(ReadField("Start date (YYYY-MM-DD): "), out t1)) {
			Console.Write("Invalid date\n");
		} else if (!DateTime.TryParse(ReadField("End date (YYYY-MM-DD): "), out t2)) {
			Console.Write("Invalid date\n");
		} else {
			this.FetchReservationsBetween(t1, t2);
			this.PrintReservations();
		}
	}

	private void ViewUserReservations()
	{
		this.FetchUserReservations();
		this.PrintReservations();
	}

	private void PrintReservations()
	{
		Console.Write("Reservations:\n");
		foreach (Reservation res in this.Reservations) {
			Console.Write("\tRoom name: {0}\n\tDescription: {1}\n\tPrice: {2}\n"
				+ "\tGroup size: {3}\n\tDate: {4}\n\tRound: {5}\n", res.Room.Name,
				res.Room.Description, res.Room.Price, res.GroupSize,
				res.TargetDateTime.ToString("D"), res.RoundNumber);
			foreach (ConsumableItem con in res.ConsumableItems)
				Console.Write("\tproduct name: {0}\n\tPrice: {1}\n"
					+ "\tDescription: {2}\n\tAmount: {3}\n",
					con.Consumable.Name, con.Consumable.Price,
					con.Consumable.Description, con.Amount);
			Console.Write("\n");
		}
	}

	// REPORT

	private void Report()
	{
		
		String n;
		if (CurrentUser == null) {
			Console.Write("You need to be signed in to complete this action\n");
		} else if (CurrentUser.Role != Role.Owner) {
			Console.Write("Access denied: insufficient permissions\n");
		} else while (true) {
			Console.Write("[0] go back\n"
				+ "[1] view yesterday's report\n");
			n = ReadField("> ");
			switch (n) {
			case "0": return;
			case "1": this.ViewDailyReport(); break;
			default: Console.Write("Invalid command number\n"); break;
			}
			Block();
		}
	}

	private void ViewDailyReport()
	{
		Report report;
		DateTime date;

		if (!DateTime.TryParse(ReadField("Date (YYYY-MM-DD): "), out date)
				&& date < DateTime.Now.Date) {
			Console.Write("Invalid date\n");
		} else if (!Server.TryFetchReport(CurrentUser.SessionToken, out report, date)) {
			Console.Write("Something went wrong while trying to get report "
				+ "information\n");
		} else { 
			Console.Write("\nAmount tickets sold: {0}\nAmount consumables sold: {1}\n"
				+ "Income: {2}", report.TicketsSold, report.ConsumablesSold,
				report.Income);
		}
	}

	// REVIEW

	private void Review()
	{
		String n;
		if (CurrentUser == null) {
			Console.Write("You need to be logged in\n");
		} else while (true) {
			Console.Write("[1] view reviews\n"
				+ "[2] create review\n"
				+ "[0] return\n");
			n = ReadField("> ");
			switch (n) {
			case "0": return;
			case "1": this.ViewReviews(); break;
			case "2": this.MakeReview(); break;
			default: Console.Write("Invalid command number\n"); break;
			}
			Block();
		}
	}

	private void FetchReviews(Room room)
	{
		var stream = new MemoryStream();
		if (!Server.TryFetchReviews(CurrentUser.SessionToken, stream, room))
			Console.Write("\nSomething went wrong while trying to get the Orders data"
				+ "from the server\n");
		else
			this.Reviews = JsonSerializer.Deserialize<List<Review>>(stream.ToArray());
		stream.Close();
	}

	private void ViewReviews()
	{
		String roomName;
		Room room;

		this.FetchRooms();
		roomName = ReadField("\nRoom name: ");
		room = this.Rooms.Find(room => room.Name == roomName);
		if (room == null) {
			Console.Write("that room does not exist\n");
			return;
		}
		this.FetchReviews(room);
		foreach (Review review in this.Reviews)
			Console.Write("\nName: {0}\nDate: {1}\nRating: {2}\nReview: {3}\n",
				review.UserName, review.DateTime.ToString("D"), review.Rating,
				review.Text);
	}

	private void MakeReview()
	{
		Int32 reviewRating;
		String roomName, reviewText;
		Review review;
		Room room;

		roomName = ReadField("\nRoom name: ");
		room = this.Rooms.Find(room => room.Name == roomName);
		if (room == null) {
			Console.Write("invalid room name\n");
			return;
		}
		reviewText = ReadField("Review: ");
		if (!Int32.TryParse(ReadField("Rating (1, 2, 3, 4 or 5): "), out reviewRating)) {
			Console.Write("Invalid number\n");
		} else if (reviewRating < 0 || reviewRating > 5) {
			Console.Write("Invalid number\n");
		} else {
			review = new Review() {
				RoomId = room.ProductId,
				Text = reviewText,
			        Rating = reviewRating
			};
			if (!Server.TryAddReview(CurrentUser.SessionToken, review))
				Console.Write("\nSomething went wrong when trying to add your"
					+ "review\n");
		}
	}
}
}
