namespace App
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.IO;

	class Client
	{
		private Boolean _Stop = false;
		public Boolean Stop { get { return _Stop; }}

		private User CurrentUser;
		private Server Connection;

		public delegate void Command();

		public Dictionary<String, Command> Commands;

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
		
		public void Connect(Server server) 
		{
			Connection = server;
		}

		public void Disconnect() 
		{
			Connection = null;
		}

		private static String ReadField(String field_name)
		{
			Console.Write(field_name);
			return Console.ReadLine().Trim();
		}

		/* Commands */

		public void Help()
		{
			Console.Write("Commands:\n" +
				"\thelp\n" +
				"\tlogin\n" +
				"\tlogout\n" +
				"\tregister\n" +
				"\tderegister\n" +
				"\texit\n");

			return;
		}

		private void helpManager(){

				Console.Write("Commands:\n" +
				"\thelp\n" +
				"\tlogout\n" +
				"\tregister\n" +
				"\tderegister\n" +
				"\tadd room\n" +
				"\tremove room\n" +
				"\texit\n");

			return;
		}

		private void HelpCustomer()
		{
			Console.Write("Commands:\n" +
				"\thelp\n" +
				"\tlogout\n" +
				"\tbuy ticket\n" +
				"\texit\n");

				return;
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

			String discription = ReadField("discription: ");
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
			
			Room room =  new Room(name, discription, price, theme, capacity);
			Connection.TryAddRoom( CurrentUser.SessionToken, room);
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
				foreach (DataColumn col in Rooms.Columns) {
					Console.WriteLine($"{col}: {row[col]}");
				}
			}
		}

		public void Exit()
		{
			Disconnect();
			_Stop = true;
		}
	}
}