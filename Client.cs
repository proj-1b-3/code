namespace App
{
	using System;
	using System.Collections.Generic;

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

		public void Exit()
		{
			Disconnect();
			Stop = true;
		}
	}
}