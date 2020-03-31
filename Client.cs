namespace App
{
	using System;
	using System.Collections.Generic;
	using System.Data;

	class Client
	{
		public Boolean Stop = false;
		private User CurrentUser;
		private Server Connection;

		public delegate void Command();

		public Dictionary<String, Command> Commands;

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
				{ "new room", NewRoom },
				{ "help", Help },
				{ "exit", Exit }
			};
		}
		
		public void Connect(Server server) 
		{
			Connection = server;
		}

		public void Disconnect() 
		{
			Connection = null;
		}

		public String ReadField(String field_name)
		{
			Console.Write(field_name);
			return Console.ReadLine().Trim();
		}

		public void Login()
		{
			Console.Write("username: ");
			String username = Console.ReadLine();
			Console.Write("password: ");
			String password = Console.ReadLine();

			if (username == "" || password == "") {
				Console.WriteLine("Leave no field empty!");
				return;
			}

			if (! Connection.TryLogin(username, password, out CurrentUser)) {
				Console.WriteLine("Wrong username or password");
				return;
			}

			Console.WriteLine("Login successful");
			
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

		public void Help()
		{
			Console.Write("Commands:\n\tlogin\n\tregister\n\tlogout" +
				"\n\tderegister\n\thelp\n\texit\n");

			return;
		}

		public void Register()
		{
			Console.Write("username: ");
			String username = Console.ReadLine();
			Console.Write("password: ");
			String password = Console.ReadLine();

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

			Console.Write("password: ");
			String password = Console.ReadLine();

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

		public void NewRoom()
		{
			String name = ReadField("name: ");
			if (name == "") {
				return;
			}

			String theme = ReadField("theme: ");
			if (theme == "") {
				return;
			}

			String dis = ReadField("discription: ");
			if (dis == "") {
				return;
			}

			Int32 cap;
			if (! Int32.TryParse(ReadField("capacity: "), out cap)) {
				return;
			}

			Single price;
			if (! Single.TryParse(ReadField("price: "), out price)) {
				return;
			}

			var row = Connection.DataBase.Tables["Rooms"].NewRow();
			row["RoomName"] = name;
			row["Theme"] = theme;
			row["Discription"] = dis;
			row["Capacity"] = cap;
			row["Price"] = price;
			Connection.DataBase.Tables["Rooms"].Rows.Add(row);
		}

		public void ListRooms()
		{
			foreach (DataRow room in Connection.DataBase.Tables["Rooms"].Rows) {
				foreach (DataColumn field in Connection.DataBase.Tables["Rooms"].Columns) {
					Console.WriteLine($"{field}: {room[field]}");
				}
			}
		}

		public void Exit()
		{
			Disconnect();
			Stop = true;
		}
	}
}