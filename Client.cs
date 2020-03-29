namespace App
{
	using System;
	using System.Collections.Generic;

	class Client
	{
		private static User CurrentUser;
		private static Boolean Stop = false;
		private static Server Connection = Server.Connect();

		private delegate void Command();

		private static readonly Dictionary<String, Command> Commands =
			new Dictionary<String, Command>
			{
				{ "login", Client.Login },
				{ "register", Client.Register },
				{ "deregister", Client.Deregister },
				{ "logout", Client.Logout },
				{ "reserve", Client.Reserve },
				{ "help", Client.Help },
				{ "exit", Client.Exit }
			};

		public static void Main(string[] args)
		{
			String input;
			Command command;

			for (;;) {
				input = ReadInput(">>> ").ToLower().Trim();
				if (! Commands.TryGetValue(input, out command)) {
					Console.WriteLine("Invalid command");
				}

				command();
			}
		}

		public static String ReadInput(String text)
		{
			Console.Write(text);
			return Console.ReadLine();
		}
		
		public static void Login()
		{
			String username = ReadInput("username: ");
			String password = ReadInput("password: ");

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

		public static void Logout()
		{
			if (CurrentUser == null) {
				Console.WriteLine("You must be logged in to logout");
				return;
			}
			
			if (! Connection.TryLogout(CurrentUser.SessionToken)) {
				Console.WriteLine("Something went wrong, please try again");
				return;
			}

			Console.WriteLine("Logout successful");
			CurrentUser = null;

			return;
		}

		public static void Reserve()
		{
			if (CurrentUser == null) {
				Console.WriteLine("You must be logged in to make a reservation");
				return;
			}

			string tickets = ReadInput("Please enter the amount of tickets you want to buy:");
			Int32 ntickets = 0;
			if (!Int32.TryParse(tickets, out ntickets)) {
				Console.WriteLine("We couldn't process this reservation, Please Try again and use a valid number");
				return;
			}
			string escaperoom = ReadInput("Choose one of our escape room programs: \n\tEscape1\n\tEscape2\n\tEscape3\n");
			if (escaperoom != "Escape1" & escaperoom != "Escape2" & escaperoom != "Escape3") {
				Console.WriteLine("We couldn't process this reservation, Please Try again");
				return;
			}
		}

		public static void Help()
		{
			System.Console.Write("Commands:\n\tlogin\n\tregister\n\tlogout" +
				"\n\tderegister\n\thelp\n\texit\n");

			return;
		}

		public static void Register()
		{
			String username = ReadInput("username: ");
			String password = ReadInput("password: ");

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

		public static void Deregister()
		{
			if (CurrentUser == null) {
				Console.WriteLine("You must be logged in to deregister");
				return;
			}

			String password = ReadInput("password: ");

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

		static void Exit()
		{
			Connection.Disconnect();
			Environment.Exit(0);
		}
	}
}