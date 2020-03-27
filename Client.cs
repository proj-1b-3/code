namespace App
{
	using System;
	using System.Collections.Generic;

	class Client
	{
		private static User CurrentUser;

		public delegate void Command ();

		public static readonly Dictionary<String, Command>
			Commands = new Dictionary<String, Command>
			{
				{ "login", Client.Login },
				{ "register", Client.Register },
				{ "deregister", Client.Deregister },
				{ "logout", Client.Logout },
				{ "exit", Client.Exit }
			};

		public static void Main (string[] args)
		{
			String[] input;
			Command cmd;

			for (;;) {
				input = ReadInput(">>> ").ToLower().Split(' ');
				if (Commands.TryGetValue(input[0], out cmd)) {
					cmd();
				}
			}
		}

		public static String ReadInput(String text)
		{
			Console.Write(text);
			return Console.ReadLine();
		}

		public static void Login ()
		{
			String username = ReadInput("username: ");
			String password = ReadInput("password: ");

			if (! Server.TryLogin(username, password, out CurrentUser)) {
				Console.WriteLine("Failed.");
				return;
			}
			
			Console.WriteLine("Secceeded.");
			
			return;
		}

		public static void Logout ()
		{
			if (CurrentUser == null) {
				Console.WriteLine(
					"Failed, you must be logged in to logout");
				return;
			}
			
			if (! Server.TryLogout(CurrentUser.Id)) {
				Console.WriteLine("Failed");
				return;
			}

			CurrentUser = null;
			Console.WriteLine("Succeeded.");

			return;
		}

		public static void Register ()
		{
			String username = ReadInput("username: ");
			String password = ReadInput("password: ");

			if (! Server.TryRegister(username, password)) {
				Console.WriteLine("Registration failed.");
				return;
			}
			
			Console.WriteLine("Registration succeeded.");

			return;
		}

		public static void Deregister ()
		{
			if (CurrentUser == null) {
				Console.WriteLine(
					"Failed, you must be logged in to deregister");
				return;
			}

			String password = ReadInput("password: ");

			if (! Server.TryDeregister(CurrentUser.Id, password)) {
				Console.WriteLine("Failed");
				return;
			}

			CurrentUser = null;

			return;
		}

		static void Exit ()
		{
			Environment.Exit(0);
		}
	}
}