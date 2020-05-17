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
	private Dictionary<String, Command> Commands;

	public delegate void Command();

	public Client()
	{
		CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
		this.Stop = false;
		this.Commands = new Dictionary<String, Command>
		{
			{ "lgi", this.Login },
			{ "lgo", this.Logout },
			{ "rgs", this.Register },
			{ "drgs", this.Deregister },
			{ "pay", this.Payment },
			{ "qt", this.Exit },
			{ "?", this.Help },
			{ "vcns", this.ViewConsumables },
			{ "vrsrv", this.ViewReservation },
			{ "vrvws", this.ViewReviews },
			{ "vrprt", this.ViewDailyReport },
			{ "vrms", this.ViewRooms },
			{ "ccns", this.MakeConsumable },
			{ "crsrv", this.MakeReservation },
			{ "crvw", this.MakeReview },
			{ "crm", this.MakeRoom },
			{ "ersrv", this.EditBasket },
			{ "erm", EditRoom },
			{ "ecns", this.EditConsumables },
			{ "dcns", this.RemoveConsumable },
			{ "drm", this.RemoveRoom },
		};
	}

	public void Begin(Server server)
	{
		String n;
		Command command;

		Server = server;
		while (!Stop) {
			n = ReadField("> ").ToLower();
			if (!this.Commands.TryGetValue(n, out command))
				Console.WriteLine("Invalid command");
			else
				command();
		}
		Server = null;
		return;
	}

	private static String ReadField(String field_name)
	{
		Console.Write(field_name);
		return Console.ReadLine().Trim();
	}

	private void Help()
	{
		Console.WriteLine(
			"commands:\n\t- help\n\t- login\n\t- register\n\t- deregister\n\t- exit");
		if (CurrentUser == null)
			return;
	}
	
	private void Login()
	{
		if (CurrentUser != null) {
			Console.WriteLine("You are already logged in");
			return;
		}
		String email = ReadField("email: ");
		String password = ReadField("password: ");
		if (email == "" || password == "") {
			Console.WriteLine("Leave no field empty!");
		} else if (!Server.TryLogin(email, password, out CurrentUser)) {
			Console.WriteLine("Wrong email or password");
		} else {
			Console.WriteLine("Login successful");
			FetchRooms();
			FetchConsumables();
		}
	}

	private void Logout()
	{
		if (CurrentUser == null) {
			Console.WriteLine("You must be logged in to logout");
		} else if (!Server.TryLogout(CurrentUser.SessionToken)) {
			Console.WriteLine("Something went wrong");
		} else {
			Console.WriteLine("Logout successful");
			CurrentUser = null;
		}
	}

	private void Register()
	{
		String username, email, password;
		if (CurrentUser != null) {
			Console.WriteLine("Unable to register when logged in");
			return;
		}

		username = ReadField("username: ");
		email = ReadField("email: ");
		password = ReadField("password: ");
		if (username == "" || password == "" || email == "")
			Console.WriteLine("Leave no field empty!");
		else if (!Server.TryAddUser(username, email, password))
			Console.WriteLine("The username is already in use");
		else
			Console.WriteLine("Registration successful");
	}

	private void Deregister()
	{
		String password;

		if (CurrentUser == null) {
			Console.WriteLine("You must be logged in to deregister");
			return;
		}

		password = ReadField("password: ");
		if (password == "")
			Console.WriteLine("Leave no field empty!");
		else if (!Server.TryRemoveUser(CurrentUser.SessionToken, password))
			Console.WriteLine("Something went wrong, please try again");
		else
			this.CurrentUser = null;
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

	private void MakeReservation()
	{
		String n;

		if (CurrentUser == null) {
			Console.WriteLine("You must be logged in to buy a ticket");
			return;
		}

		Console.Write(
			"[1] view rooms\n" +
			"[2] select room\n" +
			"[3] pay\n");
		n = ReadField("> ");
		switch (n) {
		case "1": this.ViewRooms(); break;
		case "2": this.SelectRoom(); break;
		case "3": this.Payment(); break;
		default: Console.WriteLine("idk"); break;
		}
	}

	private void ViewBasket()
	{
		if (CurrentUser == null) {
			Console.WriteLine("You have to be logged in to use this command");
		} else if (Basket == null) {
			Console.WriteLine("You haven't selected anything yet");
		} else {
			Console.WriteLine("Reservation:\nRoom\n\tName: {0}\n\tGroup size: {1}\n"
				+ "\tDate: {2}\n", Basket.Room.Name, Basket.GroupSize,
				Basket.TargetDateTime.ToString("D"));
			foreach (var item in Basket.ConsumableItems)
				Console.WriteLine("Items:\n\tProduct name: {0}\n\tAmount: {1}",
					item.Consumable.Name, item.Amount);
		}
	}
	
	private void EditBasket()
	{
		String chosenGenre = ReadField("[P]roduct or [R]oom: ");
		if (chosenGenre == "P")
			this.EditCurrentConsumables();
		else if (chosenGenre == "R")
			this.EditCurrentRoom();
	}

	private void EditCurrentConsumables()
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
			var item = this.Basket.ConsumableItems.Find(item =>
			                      item.Consumable.ProductId == chosenProduct.ProductId);
			if (item != null)
				item.Amount = newAmount;
		}
	}

	private void EditCurrentRoom()
	{
		Int32 newGroupsize;
		String roomName = ReadField("Room name: ");
		var chosenRoom = this.Rooms.Find(room => room.Name == roomName);
		if (chosenRoom == null) {
			Console.WriteLine("Invalid name");
		} else if (!Int32.TryParse(ReadField("New group size: "), out newGroupsize)) {
			Console.WriteLine("invalid number");
		} else {
			Int32 freePlaces = chosenRoom.Capacity - Server.CheckReservation(Basket);
			if (newGroupsize > freePlaces) {
				Console.WriteLine("there's no enough places");
				return;
			}
			Console.WriteLine("Places left: {0}", freePlaces);
			String confirm = ReadField("Confirm reservation ([Y]es or [N]o): ");
			if (confirm == "Y")
				Basket.GroupSize = newGroupsize;
		}
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
		else
			Console.WriteLine("Payment succeed");
		Basket = new Reservation();
	}

	private void FetchRooms()
	{
		MemoryStream stream = new MemoryStream();
		if (!Server.TryFetchRooms(CurrentUser.SessionToken, stream))
			Console.Write("Something went wrong while trying to get the rooms data"
			                  + "from the server\n");
		else
			this.Rooms = JsonSerializer.Deserialize<List<Room>>(stream.ToArray());
		stream.Close();
	}
	
	private void ViewRooms()
	{
		if (CurrentUser == null) {
			Console.Write("You need to be logged in\n");
		} else {
			this.FetchRooms();
			foreach(Room room in this.Rooms)
				Console.Write("\nName: {0}\nTheme: {1}\nDescription: {2}\n"
					+ "Capacity: {3}\nMaximum duration: {5}\nNumber of rounds: {4} 	
					+ "\nPrice: {6}\n", room.Name, room.Theme, room.Description,
					room.Capacity, room.NumberOfRounds, room.MaxDuration, room.Price);
			Console.Write("\n");
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
		if (CurrentUser == null)
			;
		else if (CurrentUser.Role != Role.Owner)
			Console.WriteLine("You do not have the permissions to perform this action");
		else if (!Int64.TryParse(ReadField("Room ID"), out roomId))
			Console.WriteLine("That is not a valid Room ID");
		else
			Server.TryRemoveRoom(CurrentUser.SessionToken, roomId);
	}

	private void EditRoom()
	{
		Single _price;
		Int32 _capacity;
		Int32 _roundsAmount;
		Int32 _maxDuration;

		if (CurrentUser == null) {
			return;
		} else if (CurrentUser.Role != Role.Owner) {
			Console.WriteLine("You do not have the permissions to perform this action");
			return;
		}
		String chosenRoom = ReadField("Room name: ");
		var room = this.Rooms.Find(room => room.Name == chosenRoom);
		if (room == null) {
			Console.WriteLine("Invalid name");
			return;
		}
		Room copied = room.Clone();
		String name = ReadField("Name: ");
		if (name != "")
			copied.Name = name;
		String theme = ReadField("Theme: ");
		if (theme != "")
			copied.Theme = theme;
		String description = ReadField("Description: ");
		if (description != "")
			copied.Description = description;
		String price = ReadField("Price: ");
		if (price != "")
			if (Single.TryParse(price, out _price))
				copied.Price = _price;
			else
				Console.WriteLine("Invalid number");
		String availabile = ReadField("Available ([Y]es or [N]o): ");
		if (availabile != "")
			if (availabile == "Y")
				copied.Available = true;
			else
				copied.Available = false;
		String capacity = ReadField("Capacity: ");
		if (capacity != "")
			if (Int32.TryParse(price, out _capacity))
				copied.Capacity = _capacity;
			else
				Console.WriteLine("Invalid number");
		String roundsAmount = ReadField("Number of rounds: ");
		if (roundsAmount != "")
			if (Int32.TryParse(roundsAmount, out _roundsAmount))
				copied.NumberOfRounds = _roundsAmount;
			else
				Console.WriteLine("Invalid number");
		String maxDuration = ReadField("Maximum duration: ");
		if (maxDuration != "")
			if (Int32.TryParse(maxDuration, out _maxDuration))
				copied.MaxDuration = _maxDuration;
			else
				Console.WriteLine("Invalid number");
		Server.TryEditRoom(CurrentUser.SessionToken, copied);
	}

	private void FetchConsumables()
	{
		var stream = new MemoryStream();
		if (!Server.TryFetchConsumables(CurrentUser.SessionToken, stream))
			Console.WriteLine("Something went wrong while trying to get the products "
			                  + "data from the server");
		else
			this.Consumables = JsonSerializer
			                   .Deserialize<List<Consumable>>(stream.ToArray());
		stream.Close();
	}

	private void ViewConsumables()
	{
		if (CurrentUser == null) {
			Console.WriteLine("You have to be logged in");
			return;
		}
		FetchConsumables();
		foreach (var con in Consumables) {
			Console.WriteLine("\nName: {0}\nDescription: {1}\nPrice: {2}", con.Name,
			                  con.Description, con.Price);
			if (con.Available)
				Console.WriteLine("Available: Yes");
			else
				Console.WriteLine("Available: No");
			
		}
		Console.WriteLine("");
	}

	private void SelectConsumable()
	{
		Int32 amount;
		if (this.CurrentUser == null) {
			Console.WriteLine("You must be logged in to select a consumbale");
		} else if (this.Basket == null) {
			Console.WriteLine("Please selecct room first");
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

	private void EditConsumables()
	{
		if (CurrentUser == null) {
			return;
		}
		if (CurrentUser.Role != Role.CafeManager) {
			Console.WriteLine("You do not have the permissions to perform this action");
			return;
		}
		string chosenConsumable = ReadField("Product name: ");
		var consumable = this.Consumables.Find(
			consumable => consumable.Name == chosenConsumable);
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

	private void FetchUserReservations()
	{
		var stream = new MemoryStream();
		if (!Server.TryFetchUserReservations(CurrentUser.SessionToken, stream))
			Console.WriteLine("Something went wrong while trying to get the Orders "
			                  + "data from the server");
		else
			this.Reservations = JsonSerializer
			                    .Deserialize<List<Reservation>>(stream.ToArray());
		stream.Close();
	}

	private void FetchReservationsBetween(DateTime t1, DateTime t2)
	{
		var stream = new MemoryStream();
		if (!this.Server.TryFetchReservationsBetween(CurrentUser.SessionToken, stream, t1, t2))
			Console.WriteLine("Something went wrong while trying to get the Orders "
			                  + "data from the server");
		else
			this.Reservations = JsonSerializer.Deserialize<List<Reservation>>(stream.ToArray());
		stream.Close();
	}

	private void ViewReservation()
	{
		String n;

		if (CurrentUser == null) {
			Console.WriteLine("You need to be logged in");
			return;
		}
		Console.Write("[1] current\n[2] all my\n[3] between\n");
		n = ReadField("> ");
		switch (n) {
		case "1": this.ViewBasket(); break;
		case "2": this.ViewUserReservations(); break;
		case "3": this.ViewReservationsBetween(); break;
		default: Console.WriteLine("idk"); break;
		}
	}


	private void ViewReservationsBetween()
	{
		DateTime t1, t2;
		if (this.CurrentUser == null || this.CurrentUser.Role != Role.Owner) {
			Console.WriteLine("You dont have permission to do this procces");
		} else if (!DateTime.TryParse(ReadField("Start date (YYYY-MM-DD): "), out t1)) {
			Console.WriteLine("Invalid date");
		} else if (!DateTime.TryParse(ReadField("End date (YYYY-MM-DD): "), out t2)) {
			Console.WriteLine("Invalid date");
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
		Console.WriteLine("Reservations: ");
		foreach (Reservation res in this.Reservations) {
			Console.Write("\tRoom name: {0}\n\tDescription: {1}\n\tPrice: {2}\n"
				+ "\tGroup size: {3}\n\tDate: {4}\n\tRound: {5}\n", res.Room.Name,
				res.Room.Description, res.Room.Price, res.GroupSize,
				res.TargetDateTime.ToString("D"), res.RoundNumber);
			foreach (var con in res.ConsumableItems)
				Console.Write("\tproduct name: {0}\n\tPrice: {1}\n "
					+ "\tDescription: {2}\n\tAmount: {3}\n", con.Consumable.Name,
					con.Consumable.Price, con.Consumable.Description, con.Amount);
			Console.WriteLine("________________________________________");
		}
	}

	private void ViewDailyReport()
	{
		Report report;
		DateTime date;
		if (CurrentUser.Role != Role.Owner)
			Console.WriteLine("You do not have permissions to use this command");
		else if (!DateTime.TryParse(ReadField("Date (YYYY-MM-DD): "), out date)
		         && date < DateTime.Now.Date)
			Console.WriteLine("Invalid date");
		else if (!Server.TryFetchReport(CurrentUser.SessionToken, out report, date))
			Console.WriteLine("Something went wrong while trying to get report "
			                  + "information");
		else 
			Console.WriteLine("Amount tickets sold: {0}\nAmount consumables sold: {1}\n"
			                  + "Income: {2}", report.TicketsSold,
			                  report.ConsumablesSold, report.Income);
	}

	private void MakeReview()
	{
		Int32 reviewRating;
		if (CurrentUser == null) {
			Console.WriteLine("You need to be signed in to complete this action");
			return;
		}
		String roomName = ReadField("Room name: ");
		var room = this.Rooms.Find(room => room.Name == roomName);
		if (room == null) {
			Console.WriteLine("invalid room name");
			return;
		}
		String reviewText = ReadField("Review: ");
		if (!Int32.TryParse(ReadField("Rating: "), out reviewRating)) {
			Console.WriteLine("Invalid number");
		} else if (reviewRating < 0 || reviewRating > 5) {
			Console.WriteLine("Invalid number");
		} else {
			var review = new Review(){ RoomId = room.ProductId, Text = reviewText,
			                           Rating = reviewRating };
			if (!Server.TryAddReview(CurrentUser.SessionToken, review))
				Console.WriteLine("Something went wrong when trying to add your"
				                  + "review");
		}
	}

	private void FetchReviews(Room room)
	{
		var stream = new MemoryStream();
		if (!Server.TryFetchReviews(CurrentUser.SessionToken, stream, room))
			Console.WriteLine("Something went wrong while trying to get the Orders data"
			                  + "from the server");
		else
			this.Reviews = JsonSerializer.Deserialize<List<Review>>(stream.ToArray());
	}

	private void ViewReviews()
	{
		MemoryStream stream = new MemoryStream();
		String roomName = ReadField("Room name: ");
		var room = this.Rooms.Find(room => room.Name == roomName);
		if (room == null) {
			Console.WriteLine("Room doesn't exsist");
			return; }
		FetchReviews(room);
		foreach (var review in this.Reviews) {
			Console.WriteLine("\nName: {0}\nDate: {1}\nRating: {2}\nReview: {3}",
			                  review.UserName, review.DateTime.ToString("D"),
			                  review.Rating, review.Text); }
	}

	private void Exit()
	{
		if (CurrentUser != null)
			this.Logout();
		this.Stop = true;
	}
}
}
