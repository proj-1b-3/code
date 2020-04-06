namespace App
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.IO;

	class Client
	{
		private Boolean Stop = false;

		private User CurrentUser;
		private Server Connection;

		public delegate void Command();

		private Dictionary<String, Command> Commands;

		private DataTable Rooms;

		public Client()
		{
			Commands = new Dictionary<String, Command>
			{
				{ "login", Login },
				{ "register", Register },
				{ "deregister", Deregister },
				{ "logout", Logout },
				{ "buy ticket", BuyTicket },
				{ "list rooms", ListRooms },
				{ "add room", AddRoom },
				{ "remove room", RemoveRoom },
				{ "help", Help },
				{ "exit", Exit }
			};
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
			}else if (CurrentUser.Role == Role.Owner){ 
				Console.WriteLine ("Commands you can use:  ");
				Console.WriteLine ("-\thelp");
				Console.WriteLine ("-\tlogin" );
				Console.WriteLine ("-\togout");
				Console.WriteLine ("-\tregister" );
				Console.WriteLine ("-\tderegister" );
				Console.WriteLine ("-\tbuy ticket");
				Console.WriteLine ("-\tlist rooms");
				Console.WriteLine ("-\tadd room ");
				Console.WriteLine ("-\tremove room ");
				Console.WriteLine ("-\texit");
			}else if (CurrentUser.Role == Role.Manager){
				Console.WriteLine ("Commands you can use:  ");
				Console.WriteLine ("-\thelp");
				Console.WriteLine ("-\tlogin" );
				Console.WriteLine ("-\tlogout");
				Console.WriteLine ("-\tregister" );
				Console.WriteLine ("-\tderegister" );
				Console.WriteLine ("-\tbuy ticket");
				Console.WriteLine ("-\tlist rooms");
				Console.WriteLine ("-\tadd room ");
				Console.WriteLine ("-\tremove room ");
				Console.WriteLine ("-\texit");
			}else if (CurrentUser.Role == Role.CafeManager){
				Console.WriteLine ("Commands you can use:  ");
				Console.WriteLine ("-\thelp");
				Console.WriteLine ("-\tlogin" );
				Console.WriteLine ("-\tlogout");
				Console.WriteLine ("-\tregister" );
				Console.WriteLine ("-\tderegister" );
				Console.WriteLine ("-\tbuy ticket");
				Console.WriteLine ("-\tlist rooms");
				Console.WriteLine ("-\texit");
			}else if (CurrentUser.Role == Role.Consumer){
				Console.WriteLine ("Commands you can use:  ");
				Console.WriteLine ("-\thelp");
				Console.WriteLine ("-\tlogin" );
				Console.WriteLine ("-\tlogout");
				Console.WriteLine ("-\tregister" );
				Console.WriteLine ("-\tderegister" );
				Console.WriteLine ("-\tbuy ticket");
				Console.WriteLine ("-\texit");
				return;	
			}

		
		}


		public void Login()
		{
			String username = ReadField("username: ");
			String password = ReadField("password: ");

			if (username == "" || password == "") {
				Console.WriteLine("Leave no field empty!");
				return;
			}

			if (! Connection.TryLogin(username, password, out CurrentUser)) {
				Console.WriteLine("Wrong username or password");
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
			String username = ReadField("username: ");
			String password = ReadField("password: ");

			if (username == "" || password == "") {
				Console.WriteLine("Leave no field empty!");
				return;
			}

			if (! Connection.TryRegister(username, password)) {
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

		public void BuyTicket()
		{
			if (CurrentUser == null) {
				Console.WriteLine("You must be logged in to buy a ticket");
				return;
			}

			Console.Write("amount: ");
			String tickets = Console.ReadLine();
			Int32 ntickets = 0;

			if (!Int32.TryParse(tickets, out ntickets)) {
				Console.WriteLine("Invalid number");
				return;
			}

			Console.Write("escaperoom: ");
			string escaperoom = Console.ReadLine().ToLower();

			if (escaperoom != "escape1" && escaperoom != "escape2" && escaperoom != "escape3") {
				Console.WriteLine("We don't offer this escaperoom");
				return;
			}
		}

		public void Basket()
		{
 			DataTable basket = new DataTable();
			basket.Columns.Add("ID");
			basket.Columns.Add("Type");
			basket.Columns.Add("Amount");
		}
		public void AddRoom()
		{
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
			
			Single price;
			if (! Single.TryParse(ReadField("price: "), out price)) {
				return;
			}


			
			var room =  new Room(name, theme, discription, capacity, price);
			Connection.TryAddRoom(CurrentUser.SessionToken, room);
		}

		public void RemoveRoom()
		{
			string name = ReadField("name: ");
			Connection.TryRemoveRoom(CurrentUser.SessionToken, name);
		}

		private void FetchRooms()
		{
			MemoryStream tabledata = new MemoryStream();
			
			if (!Connection.TryGetRoomData(CurrentUser.SessionToken, tabledata)) {
				Console.WriteLine("Something went wrong while trying to get the product data from the server");
				return;
			}

			Rooms = new DataTable();
			Rooms.ReadXml(tabledata);

			tabledata.Close();
		}

		public void ListRooms()
		{
			foreach (DataRow row in Rooms.Rows) {
				Console.WriteLine("");
				foreach (DataColumn col in Rooms.Columns) {
					Console.WriteLine($"{col}: {row[col]}");
				}
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