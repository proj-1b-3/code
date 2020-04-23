namespace App
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text.Json;
	using System.Globalization;

	class Client
	{
		private Boolean Stop = false;

		private User CurrentUser;
		private Server Server;

		private List<Room> Rooms;
		private List<Reservation> Reservations;
		private List<Reservation> ReservationHistory;
		private List<Review> Reviews;
		private List<Consumable> Consumables;
		private Reservation Basket;
		private CultureInfo Culture;

		public delegate void Command();

		private Dictionary<String, Command> Commands;

		public Client()
		{
			this.Commands = new Dictionary<String, Command>
			{
				{ "login", this.Login },
				{ "logout", this.Logout },
				{ "register", this.Register },
				{ "pay", this.Payment },
				{ "exit", this.Exit },
				{ "list rooms", this.ViewRooms },
				{ "select room", this.MakeReservation },
				{ "view consumables", this.ViewConsumables },
				{ "Fetch consumables", this.FetchConsumables },
				{ "select consumable", this.SelectConsumable },
				{ "list basket", this.ViewBasket },
				{ "edit basket", this.EditBasket },
				{ "deregister", this.Deregister },
				{ "view reservations", this.ViewReservations },
				{ "view reservation history", this.FetchReservationDate },
				{ "view report", this.ViewDailyReport },
				{ "make room", this.AddRoom },
				{ "remove room", this.RemoveRoom },
				{ "edit room", EditRoom },
				{ "make consumable", this.MakeConsumable },
				{ "remove consumable", this.RemoveConsumable },
				{ "edit consumable", this.EditConsumables },
				{ "add review", this.AddReview },
				{ "view reviews", this.ViewReviews },
			};
			Culture = CultureInfo.CurrentCulture;
		}

		public void Begin(Server server)
		{
			String input;
			Command command;

			Server = server;

			while (! Stop) {
				Console.Write(">>> ");
				input = Console.ReadLine().ToLower().Trim();
				if (!this.Commands.TryGetValue(input, out command)) {
					Console.WriteLine("Invalid command");
					continue;
				}

				command();
			}

			Server = null;

			return;
		}

		private static Boolean IsLeapYear(Int32 year)
		{
			if (year % 4 != 0) {
				return false;
			} else if (year % 100 != 0) {
				return true;
			} else if (year % 400 != 0) {
				return false;
			} else {
				return true;
			}
		}

		private static readonly Int32[] monthLength = {
			31, 28, 31, 30, 29, 30, 31, 31, 30, 30, 30, 31
		};

		private static Boolean TryParseDateTime(String s, out DateTime dateTime)
		{
			dateTime = new DateTime();
			var parts = s.Split(new Char[]{'-'});
			var ints = new Int32[3];
			if (parts.Length != 3) {
				return false;
			}

			if (parts[0].Length != 4 || parts[1].Length != 2 || parts[2].Length != 2) {
				return false;
			}

			for (int i=0; i < 3; i += 1) {
				if (!Int32.TryParse(parts[i], out ints[i])) {
					return false;
				}
			}

			if (ints[0] < 1 || ints[0] > 9999) {
				return false;
			} else if (ints[1] < 1 || ints[1] > 12) {
				return false;
			} else if (IsLeapYear(ints[0])) {
				if (ints[1] != 2 && (ints[2] < 1 || ints[2] > monthLength[ints[1]])) {
					return false;
				} else if (ints[2] < 1 || ints[2] > 29) {
					return false;
				}
			} else if (ints[2] < 1 || ints[2] > monthLength[ints[1]]) {
				return false;
			}

			dateTime = new DateTime(ints[0], ints[1], ints[2]);

			return true;
		}

		private static String ReadField(String field_name)
		{
			Console.Write(field_name);
			return Console.ReadLine().Trim();
		}

		public void Help()
		{
			if (CurrentUser == null) {
				Console.WriteLine(" Register or log in first ");
				Console.WriteLine(" To register type 'register' ");
				Console.WriteLine(" To login type 'login' ");
			} else if (CurrentUser.Role == Role.Owner) { 
				Console.WriteLine ("Commands you can use:  ");
				Console.WriteLine ("-\thelp");
				Console.WriteLine ("-\tlogin" );
				Console.WriteLine ("-\tlogout");
				Console.WriteLine ("-\tregister" );
				Console.WriteLine ("-\tderegister" );
				Console.WriteLine ("-\tbuy ticket");
				Console.WriteLine ("-\tview rooms");
				Console.WriteLine ("-\tadd room ");
				Console.WriteLine ("-\tremove room ");
				Console.WriteLine ("-\texit");
			} else if (CurrentUser.Role == Role.Manager) {
				Console.WriteLine ("Commands you can use:  ");
				Console.WriteLine ("-\thelp");
				Console.WriteLine ("-\tlogin" );
				Console.WriteLine ("-\tlogout");
				Console.WriteLine ("-\tregister" );
				Console.WriteLine ("-\tderegister" );
				Console.WriteLine ("-\tbuy ticket");
				Console.WriteLine ("-\tview rooms");
				Console.WriteLine ("-\tadd room ");
				Console.WriteLine ("-\tremove room ");
				Console.WriteLine ("-\texit");
			} else if (CurrentUser.Role == Role.CafeManager) {
				Console.WriteLine ("Commands you can use:  ");
				Console.WriteLine ("-\thelp");
				Console.WriteLine ("-\tlogin" );
				Console.WriteLine ("-\tlogout");
				Console.WriteLine ("-\tregister" );
				Console.WriteLine ("-\tderegister" );
				Console.WriteLine ("-\tbuy ticket");
				Console.WriteLine ("-\tview rooms");
				Console.WriteLine ("-\texit");
			} else if (CurrentUser.Role == Role.Consumer) {
				Console.WriteLine ("Commands you can use:  ");
				Console.WriteLine ("-\thelp");
				Console.WriteLine ("-\tlogin" );
				Console.WriteLine ("-\tlogout");
				Console.WriteLine ("-\tregister" );
				Console.WriteLine ("-\tderegister" );
				Console.WriteLine ("-\tbuy ticket");
				Console.WriteLine ("-\texit");
			}

			return;
		}
		
		public void Login()
		{
			if (CurrentUser != null) {
				return;
			}

			String email = ReadField("email: ");
			String password = ReadField("password: ");

			if (email == "" || password == "") {
				Console.WriteLine("Leave no field empty!");
				return;
			}

			if (! Server.TryLogin(email, password, out CurrentUser)) {
				Console.WriteLine("Wrong email or password");
				return;
			}

			Console.WriteLine("Login successful");
			FetchRooms();
			FetchConsumables();

			return;
		}

		public void Logout()
		{
			if (CurrentUser == null) {
				Console.WriteLine("You must be logged in to logout");
				return;
			}
			
			if (! Server.TryLogout(CurrentUser.SessionToken)) {
				Console.WriteLine("Something went wrong");
				return;
			}

			Console.WriteLine("Logout successful");
			CurrentUser = null;

			return;
		}

		public void Register()
		{
			if (CurrentUser != null) {
				return;
			}

			String username = ReadField("username: ");
			String email = ReadField("email: ");
			String password = ReadField("password: ");

			if (username == "" || password == "" || email == "") {
				Console.WriteLine("Leave no field empty!");
				return;
			}

			if (! Server.TryRegister(username, email, password)) {
				Console.WriteLine("The username is already in use");
				return;
			}

			Console.WriteLine("Registration successful");

			return;
		}

		public void Deregister()
		{
			if (CurrentUser == null) {
				Console.WriteLine("You must be logged in to deregister");
				return;
			}

			String password = ReadField("password: ");

			if (password == "") {
				Console.WriteLine("Leave no field empty!");
				return;
			}

			if (! Server.TryDeregister(CurrentUser.SessionToken, password)) {
				Console.WriteLine("Something went wrong, please try again");
				return;
			}

			CurrentUser = null;

			return;
		}

		public void MakeReservation()
		{
			if (CurrentUser == null) {
				Console.WriteLine("You must be logged in to buy a ticket");
				return;
			}
			string roomName = ReadField("Room name: ");

			var room = this.Rooms.Find(room => room.Name == roomName);
			if (room == null) {
				Console.WriteLine("Invalid room name");
				return;
			}

			Int32 groupSize;
			if (! Int32.TryParse(ReadField("Group size: "), out groupSize)) {
				Console.WriteLine("Invalid number");
				return;
			}

			DateTime date;
			if (! DateTime.TryParse(ReadField("Date (YYYY-MM-DD): "), out date)) {
				Console.WriteLine("Invalid date");
				return;
			}

			if(date < DateTime.Now){
				Console.WriteLine("Invalid date");
				return;
			}

			Int32 round;
			if (! Int32.TryParse(ReadField("Round: "), out round)) {
				return;
			}

			Int32 freePlaces =
				room.Capacity - Server.CheckReservation(new Reservation (room, date.Date, round, groupSize));
			if (groupSize > freePlaces){
				Console.WriteLine("there's no enough places");
				return;
			}
			Console.WriteLine("Places left: " + freePlaces);
			string confirm = ReadField("Confirm reservation ([Y]es or [N]o): ");
			if (confirm == "Y"){
				Basket = new Reservation(room, date.Date, round, groupSize);
			}
			return;
		}

		public void ViewBasket()
		{
			if (CurrentUser == null) {
				return;
			}
			Console.WriteLine("Basket:");
			Console.WriteLine("Room");
			Console.WriteLine("\tName: " + Basket.Room.Name);
			Console.WriteLine("\tGroup size: " + Basket.GroupSize);
			Console.WriteLine("\tDate: " + Basket.TargetDateTime.ToString("D"));

			Console.WriteLine("");
			foreach(var item in Basket.ConsumableItems){
				Console.WriteLine("Items:\n\tProduct name: {0}\n\tAmount: {1}" , item.Consumable.Name, item.Amount);
			}
		}

		public void EditBasket()
		{
			String chosenGenre = ReadField("[P]roduct or [R]oom: ");

			if (chosenGenre == "P"){
				string consumableName = ReadField("Product name: ");
				var chosenProduct = this.Consumables.Find(Consumable => Consumable.Name == consumableName);
				if (chosenProduct == null) {
					Console.WriteLine("Invalid name");
					return;
				}
				Int32 newAmount;
				if (! Int32.TryParse(ReadField("New amount: "), out newAmount)) {
					Console.WriteLine("invalid number");
					return;
				}

				foreach(var item in Basket.ConsumableItems){
					if(item.Consumable.ProductId == chosenProduct.ProductId){
						item.Amount = newAmount;
						return;
					}
				}
			}
			else if (chosenGenre == "R"){
				string roomName = ReadField("Room name: ");
				var chosenRoom = this.Rooms.Find(room => room.Name == roomName);
				if (chosenRoom == null) {
					Console.WriteLine("Invalid name");
					return;
				}
				Int32 newGroupsize;
				if (! Int32.TryParse(ReadField("New group size: "), out newGroupsize)) {
					Console.WriteLine("invalid number");
					return;
				}
				Int32 freePlaces = chosenRoom.Capacity - Server.CheckReservation(Basket);
					if (newGroupsize > freePlaces){
						Console.WriteLine("there's no enough places");
							return;
					}
					Console.WriteLine("Places left: " + freePlaces);
					string confirm = ReadField("Confirm reservation ([Y]es or [N]o): ");
					if (confirm == "Y"){
						Basket.GroupSize = newGroupsize;
					}
					return;
			}
		}


		public void Payment()
		{
			MemoryStream stream = new MemoryStream();
			var pay_json = JsonSerializer.SerializeToUtf8Bytes<Reservation>(Basket);
			stream.Write(pay_json, 0, pay_json.Length);
			if(!Server.TryPay(CurrentUser.SessionToken, stream)){
				Console.WriteLine("Unsuccessful payment, Please try again");
			}
			Console.WriteLine("Payment succeed");
			Basket = new Reservation();
		}
		public void AddRoom()
		{
			if (CurrentUser == null){
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
			if (! Int32.TryParse(ReadField("capacity: "), out capacity)) {
				Console.WriteLine("invalid number");
				return;
			}

			Int32 numberofrounds;
			if (! Int32.TryParse(ReadField("Number of rounds: "), out numberofrounds)) {
				Console.WriteLine("invalid number");
				return;
			}
			
			Int32 maxduration;
			if (! Int32.TryParse(ReadField("Maximum Duration: "), out maxduration)) {
				Console.WriteLine("invalid number");
				return;
			}
			
			
			Single price;
			if (! Single.TryParse(ReadField("price: "), out price)) {
				Console.WriteLine("invalid price");
				return;
			}
			
			var room =  new Room(name, theme, discription, capacity, price, numberofrounds, maxduration);
			Server.TryAddRoom(CurrentUser.SessionToken, room);
		}

		public void RemoveRoom()
		{
			if (CurrentUser == null) {
				return;
			}

			if (CurrentUser.Role != Role.Owner) {
				Console.WriteLine("You do not have the permissions to perform this action");
				return;
			}
				
			Int64 roomId;
			if (!Int64.TryParse(ReadField("Room ID"), out roomId)){
				Console.WriteLine("That is not a valid Room ID");
				return;
			}
			Server.TryRemoveRoom(CurrentUser.SessionToken, roomId);
		}

		public void EditRoom()
		{
			if (CurrentUser == null){
				return;
			}

			if (CurrentUser.Role != Role.Owner){
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
			String theme = ReadField("Theme: ");
			String description = ReadField("Description: ");
			String price = ReadField("Price: ");
			Single _price;
			String availabile = ReadField("Available ([Y]es or [N]o): ");
			String capacity = ReadField("Capacity: ");
			Int32 _capacity;
			String roundsAmount = ReadField("Number of rounds: ");
			Int32 _roundsAmount;
			String maxDuration = ReadField("Maximum duration: ");
			Int32 _maxDuration;

			if(name != ""){
				copied.Name = name;
			}
			if(description != ""){
				copied.Description = description;
			}
			if(price != ""){
				if(Single.TryParse(price, out _price)){
					copied.Price = _price;
				}
				else{
					Console.WriteLine("Invalid number");
				}
			}
			if(availabile != ""){
				if(availabile == "Y"){
					copied.Available = true;
				}
				else{
					copied.Available = false;
				}
			}
			if(theme != ""){
				copied.Theme = theme;
			}
			if(capacity != ""){
				if(Int32.TryParse(price, out _capacity)){
					copied.Capacity = _capacity;
				}
				else{
					Console.WriteLine("Invalid number");
				}
			}
			if(roundsAmount != ""){
				if(Int32.TryParse(roundsAmount, out _roundsAmount)){
					copied.NumberOfRounds = _roundsAmount;
				}
				else{
					Console.WriteLine("Invalid number");
				}
			}
			if(maxDuration != ""){
				if(Int32.TryParse(maxDuration, out _maxDuration)){
					copied.MaxDuration = _maxDuration;
				}
				else{
					Console.WriteLine("Invalid number");
				}
			}

			Server.TryEditRoom(CurrentUser.SessionToken, copied);
		}

		public void MakeConsumable()
		{
			if (CurrentUser == null){
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
			if (! Single.TryParse(ReadField("price: "), out price)) {
				Console.WriteLine("invalid price");
				return;
			}

			Boolean availability = false;
			String avb = ReadField("Available ([Y]es or [N]o): ");
			if(avb == "Y"){
				availability = true;
			}

			Server.TryAddConsumable(CurrentUser.SessionToken, new Consumable(name, discription, price, availability));
		}

		public void RemoveConsumable()
		{
			if (CurrentUser == null) {
				return;
			}

			if (CurrentUser.Role != Role.CafeManager) {
				Console.WriteLine("You do not have the permissions to perform this action");
				return;
			}

			string consumableName = ReadField("Product name: ");


			var Consumable = this.Consumables.Find(Consumable => Consumable.Name == consumableName);
			if (Consumable == null) {
				Console.WriteLine("Invalid name");
				return;
			}

			Server.TryRemoveConsumable(CurrentUser.SessionToken, Consumable);
		}

		public void EditConsumables()
		{
			if (CurrentUser == null){
				return;
			}

			if (CurrentUser.Role != Role.CafeManager){
				Console.WriteLine("You do not have the permissions to perform this action");
				return;
			}

			
			string chosenConsumable = ReadField("Product name: ");
			var consumable = this.Consumables.Find(Consumable => Consumable.Name == chosenConsumable);
			if (consumable == null) {
				Console.WriteLine("Invalid name");
				return;
			}
			Consumable copied = consumable.Clone();
			String name = ReadField("Name: ");
			String description = ReadField("Description: ");
			String price = ReadField("Price: ");
			Single _price;
			String availabile = ReadField("Available ([Y]es or [N]o): ");
			if(name != ""){
				copied.Name = name;
			}
			if(description != ""){
				copied.Description = description;
			}
			if(price != ""){
				if(Single.TryParse(price, out _price)){
					copied.Price = _price;
				}
				else{
					Console.WriteLine("Invalid number");
					return;
				}
			}
			if(availabile != ""){
				if(availabile == "Y"){
					copied.Available = true;
				}
				else{
					copied.Available = false;
				}
			}
			Server.TryEditConsumable(CurrentUser.SessionToken, copied);
		}

		public void SelectConsumable()
		{
			if (CurrentUser == null) {
				Console.WriteLine("You must be logged in to select a consumbale");
				return;
			}
			if (Basket == null){
				Console.WriteLine("Please selecct room first");
				return;
			}
			FetchConsumables();
			string consumableName = ReadField("Name: ");
			var consumable = this.Consumables.Find(consumable => consumable.Name == consumableName);
			if (consumable == null) {
				Console.WriteLine("Invalid product name");
				return;
			}

			Int32 amount;
			if (! Int32.TryParse(ReadField("Amount: "), out amount)) {
				Console.WriteLine("Invalid number");
				return;
			}
			Basket.ConsumableItems.Add(new ConsumableItem(consumable, amount));
		}

		public void FetchConsumables()
		{
			MemoryStream stream = new MemoryStream();
			if (!Server.TryFetchConsumables(CurrentUser.SessionToken, stream)) {
				Console.WriteLine("Something went wrong while trying to get the products data from the server");
				return;
			}
			byte[] raw_json = stream.ToArray();
			this.Consumables = JsonSerializer.Deserialize<List<Consumable>>(raw_json);
			stream.Close();
		}	

		public void ViewConsumables()
		{
			if (CurrentUser == null) {
				return;
			}

			FetchConsumables();

			foreach (var con in Consumables) {

				Console.WriteLine("\nName: {0}", con.Name);
				Console.WriteLine("Description: {0}", con.Description);
				Console.WriteLine("Price: {0}", con.Price);
				if(con.Available){
					Console.WriteLine("Available: Yes");
				}
				else{
					Console.WriteLine("Available: No");
				}
				
			}
			Console.WriteLine("");
		}

		public void FetchUserOrders()
		{
			MemoryStream stream = new MemoryStream();
			if (!Server.TryFetchUserReservations(CurrentUser.SessionToken, stream)) {
				Console.WriteLine("Something went wrong while trying to get the Orders data from the server");
				return;
			}
			byte[] rawJson = stream.ToArray();
			this.Reservations = JsonSerializer.Deserialize<List<Reservation>>(rawJson);
		}

		public void FetchReservationDate()
		{	
			if (CurrentUser.Role != Role.Owner) {
				Console.WriteLine("You dont have permission to do this procces");
				return;
			}
			MemoryStream stream = new MemoryStream();
			DateTime firstDate;
			if (! DateTime.TryParse(ReadField("Start date (YYYY-MM-DD): "), out firstDate)) {
				Console.WriteLine("Invalid date");
				return;
			}
			DateTime secondDate;
			if (! DateTime.TryParse(ReadField("End date (YYYY-MM-DD): "), out secondDate)) {
				Console.WriteLine("Invalid date");
				return;
			}
			if (!Server.TryFetchReservationsBetween(CurrentUser.SessionToken, stream, firstDate, secondDate)) {
				Console.WriteLine("Something went wrong while trying to get the Orders data from the server");
				return;
			}
			byte[] rawJson = stream.ToArray();
			this.ReservationHistory = JsonSerializer.Deserialize<List<Reservation>>(rawJson);

			foreach (var res in ReservationHistory){
				Console.WriteLine("\tRoom name: {0}", res.Room.Name);
				Console.WriteLine("\tDescription: {0}", res.Room.Description);
				Console.WriteLine("\tPrice: {0}", res.Room.Price);
				Console.WriteLine("\tGroup size: {0}", res.GroupSize);
				Console.WriteLine("\tDate: {0}", res.TargetDateTime.ToString("D"));
				Console.WriteLine("\tRound: {0}", res.RoundNumber);
				foreach(var con in res.ConsumableItems){
					Console.WriteLine("\tproduct name: " + con.Consumable.Name);
					Console.WriteLine("\tPrice: " + con.Consumable.Price);
					Console.WriteLine("\tDescription: " + con.Consumable.Description);
					Console.WriteLine("\tAmount: " + con.Amount);
				}
				Console.WriteLine("________________________________________");
			}
		}

		public void ViewDailyReport()
		{
			if (CurrentUser.Role != Role.Owner) {
				Console.WriteLine("You dont have permission to do this procces");
				return;
			}
			Report report;
			DateTime date;

			if (! TryParseDateTime(ReadField("Date (YYYY-MM-DD): "), out date)) {
				Console.WriteLine("Invalid date");
				return;
			}
			if(!Server.TryFetchReport(CurrentUser.SessionToken, out report, date)){
				Console.WriteLine("Something went wrong while trying to get report information");
			}
			if(report == null){
				Console.WriteLine("No data found");
				return;
			}
			Console.WriteLine("Amount tickets sold: " + report.TicketsSold);
			Console.WriteLine("Amount consumables sold: " + report.ConsumablesSold);
			Console.WriteLine("Income: " + report.Income);
		}
		public void ViewReservations()
		{
			if (CurrentUser == null) {
				return;
			}
			FetchUserOrders();
			Console.WriteLine("Reservations: ");
			foreach (var res in Reservations) {

				Console.WriteLine("\tRoom name: {0}", res.Room.Name);
				Console.WriteLine("\tDescription: {0}", res.Room.Description);
				Console.WriteLine("\tPrice: {0}", res.Room.Price);
				Console.WriteLine("\tGroup size: {0}", res.GroupSize);
				Console.WriteLine("\tDate: {0}", res.TargetDateTime.ToString("D"));
				Console.WriteLine("\tRound: {0}", res.RoundNumber);
				foreach(var con in res.ConsumableItems){
					Console.WriteLine("\tProduct name: " + con.Consumable.Name);
					Console.WriteLine("\tPrice: " + con.Consumable.Price);
					Console.WriteLine("\tDescription: " + con.Consumable.Description);
					Console.WriteLine("\tAmount: " + con.Amount);
				}
			}
		}

		private void FetchRooms()
		{
			MemoryStream stream = new MemoryStream();
			if (!Server.TryFetchRooms(CurrentUser.SessionToken, stream)) {
				Console.WriteLine("Something went wrong while trying to get the rooms data from the server");
				return;
			}
			byte[] raw_json = stream.ToArray();
			this.Rooms = JsonSerializer.Deserialize<List<Room>>(raw_json);

			stream.Close();
		}

		
		public void ViewRooms()
		{
			if (CurrentUser == null) {
				return;
			}
			FetchRooms();
			
			foreach (var room in Rooms) {
				Console.WriteLine("\nName: {0}", room.Name);
				Console.WriteLine("Theme: {0}", room.Theme);
				Console.WriteLine("Description: {0}", room.Description);
				Console.WriteLine("Capacity: {0}", room.Capacity);
				Console.WriteLine("Number of rounds: {0}", room.NumberOfRounds);
				Console.WriteLine("Maximum duration: {0}", room.MaxDuration);
				Console.WriteLine("Price: {0}", room.Price);
			}

			Console.WriteLine("");

			return;
		}

		public void AddReview()
		{	
			Review review = new Review();
			String roomName = ReadField("Room name: ");
			var room = this.Rooms.Find(room => room.Name == roomName);
			String reviewText = ReadField("Review: ");
			Int32 reviewRating;
			if (! Int32.TryParse(ReadField("Rating: "), out reviewRating)) {
				Console.WriteLine("Invalid number");
				return;
			}
			else if(reviewRating < 0 || reviewRating > 5){
				Console.WriteLine("Invalid number");
				return;
			}
			review.RoomId = room.ProductId;
			review.Text = reviewText;
			review.Rating = reviewRating;
			if(!Server.TryAddReview(CurrentUser.SessionToken, review)){
				Console.WriteLine("Something went wrong when trying to add your review");
				return;
			}
		}
		public void ViewReviews()
		{
			MemoryStream stream = new MemoryStream();
			String roomName = ReadField("Room name: ");
			var room = this.Rooms.Find(room => room.Name == roomName);
			if (!Server.TryFetchReviews(CurrentUser.SessionToken, stream, room)) {
				Console.WriteLine("Something went wrong while trying to get the Orders data from the server");
				return;
			}
			byte[] rawJson = stream.ToArray();
			this.Reviews = JsonSerializer.Deserialize<List<Review>>(rawJson);
			foreach(var review in Reviews){
				Console.WriteLine("Name: " + review.UserName);
				Console.WriteLine("Rating: " + review.Rating);
				Console.WriteLine("Review:\n" + review.Text);
				Console.WriteLine("Date: " + review.DateTime.ToString("D"));
			}
		}
		public void Exit()
		{
			Stop = true;
		}
	}
}
