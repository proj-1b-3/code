namespace App
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text.Json;

	class Client
	{
		private Boolean Stop = false;

		private User CurrentUser;
		private Server Connection;

		private List<Room> Rooms;
		private Order Basket;

		public delegate void Command();

		private Dictionary<String, Command> Commands;


		public Client()
		{
			Commands = new Dictionary<String, Command>
			{
				{ "login", Login },
				{ "register", Register },
				{ "deregister", Deregister },
				{ "logout", Logout },
				{ "make reservation", MakeReservation },
				{ "view basket", ViewBasket },
			    { "pay", Payment },
				{ "view rooms", ViewRooms },
				{ "add room", AddRoom },
				{ "remove room", RemoveRoom },
				{ "sync", FetchRooms },
				{ "help", Help },
				{ "exit", Exit }
			};
			
			Basket = new Order();
		}

		public void Begin(Server server)
		{
			String input;
			Command command;

			Connection = server;

			while (! Stop) {
				Console.Write(">>> ");
				input = Console.ReadLine().ToLower().Trim();

				if (! Commands.TryGetValue(input, out command)) {
					Console.WriteLine("Invalid command");
					continue;
				}

				command();
			}

			Connection = null;

			return;
		}

		private static String ReadField(String field_name)
		{
			Console.Write(field_name);
			return Console.ReadLine().Trim();
		}

		/* Commands */

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

			if (! Connection.TryLogin(email, password, out CurrentUser)) {
				Console.WriteLine("Wrong email or password");
				return;
			}

			Console.WriteLine("Login successful");
			FetchRooms();

			return;
		}

		public void Logout()
		{
			if (CurrentUser == null) {
				Console.WriteLine("You must be logged in to logout");
				return;
			}
			
			if (! Connection.TryLogout(CurrentUser.SessionToken)) {
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

			if (! Connection.TryRegister(username, email, password)) {
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

			if (! Connection.TryDeregister(CurrentUser.SessionToken, password)) {
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

			/*
			var room = this.Rooms.Find(room => room.Name = roomName);
			if (room == null) {
				Console.WriteLine("Invalid room name");
				return;
			}

			Int32 groupSize;
			if (! Int32.TryParse(ReadField("Group size"), out groupSize)) {
				Console.WriteLine("Invalid number");
				return;
			}

			DateTime date;
			if (! DateTime.TryParse(ReadField("Date (YYYY-MM-DD):\n"), out date)) {
				Console.WriteLine("Invalid date");
				return;
			}

			if(date < DateTime.Now){
				Console.WriteLine("Invalid date");
				return;
			}

			Int32 round;
			if (! Int32.TryParse(ReadField("Round: "), out round)) {
				Console.WriteLine("Invalid number");
				return;
			}

			if (round < 1 || round > room.RoundNumber) {
				Console.WriteLine("Invalid round number");
				return;
			}

			Basket.Reservations.Add(new Reservation(room.ProductId, groupSize, date.Date, round));
			return;
			*/

			foreach (var room in Rooms){
				if (room.Name == roomName){
					Int64 roomid = room.ProductId; 

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
					Basket.Reservations.Add(new Reservation (roomid, groupSize, date.Date, round));
					return;
				}
			}
		}

		public void ViewBasket()
		{
			if (CurrentUser == null) {
				return;
			}
			Console.WriteLine("Basket:");
			foreach(var item in Basket.Reservations){
				Console.WriteLine("Reservations:\n\tRoom ID {0}\n\tGroup size {1}" , item.RoomId, item.GroupSize);
				Console.WriteLine("\tDate " + item.DateTime.ToString("D"));
			}
			Console.WriteLine("");
			foreach(var item in Basket.Items){
				Console.WriteLine("Items:\n\tProduct ID {0}\n\tAmount {1}" , item.ProductId, item.Amount);
			}
		}

		public void Payment()
		{
			MemoryStream stream = new MemoryStream();
			var pay_json = JsonSerializer.SerializeToUtf8Bytes<Order>(Basket);
			stream.Write(pay_json, 0, pay_json.Length);
			if(!Connection.TryPay(CurrentUser.SessionToken, stream)){
				Console.WriteLine("Unsuccessful payment, Please try again");
			}
			Console.WriteLine("Payment succeed");

		}
		public void AddRoom()
		{
			if (CurrentUser == null){
				return;
			}

			String name = ReadField("name: ");
			if (name == "") {
				return;
			}

			String theme = ReadField("theme: ");
			if (theme == "") {
				return;
			}
			
			String discription = ReadField("description: ");
			if (discription == "") {
				return;
			}

			Int32 capacity;
			if (! Int32.TryParse(ReadField("capacity: "), out capacity)) {
				return;
			}

			Int32 numberofrounds;
			if (! Int32.TryParse(ReadField("Number of rounds: "), out numberofrounds)) {
				return;
			}
			
			Int32 maxduration;
			if (! Int32.TryParse(ReadField("Maximum Duration: "), out maxduration)) {
				return;
			}
			
			
			Single price;
			if (! Single.TryParse(ReadField("price: "), out price)) {
				return;
			}


			
			var room =  new Room(name, theme, discription, capacity, price, numberofrounds, maxduration);
			Connection.TryAddRoom(CurrentUser.SessionToken, room);
		}

		public void RemoveRoom()
		{
			if (CurrentUser == null) {
				return;
			}

			if (CurrentUser.Role != Role.Manager || CurrentUser.Role != Role.Owner) {
				Console.WriteLine("You do not have the permissions to perform this action");
				return;
			}
				
			Console.WriteLine("Room ID");
			Int64 roomid;
			if (!Int64.TryParse(ReadField(""), out roomid)){
				Console.WriteLine("That is not a valid Room ID");
				return;
			}
			Connection.TryRemoveRoom(CurrentUser.SessionToken, roomid);
		}

		private void FetchRooms()
		{
			MemoryStream stream = new MemoryStream();
			if (!Connection.TryGetRoomData(CurrentUser.SessionToken, stream)) {
				Console.WriteLine("Something went wrong while trying to get the product data from the server");
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

		public void Exit()
		{
			Stop = true;
		}
	}
}
