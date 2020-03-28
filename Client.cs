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
				{ "help", Client.Help },
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
		
		public static void Login()
		{
			String username = ReadInput("username: ");
			String password = ReadInput("password: ");

			if (username == "" || password == "") {
				Console.WriteLine("Leave no field empty!");
				return;
			}

			if (! Server.TryLogin(username, password, out CurrentUser)) {
				Console.WriteLine("Wrong username or password");
				return;
			}
			
			return;
		}

		public static void Logout()
		{
			if (CurrentUser == null) {
				Console.WriteLine("You must be logged in to logout");
				return;
			}
			
			if (! Server.TryLogout(CurrentUser.Name)) {
				Console.WriteLine("Something went wrong, please try again");
				return;
			}

			CurrentUser = null;

			return;
		}

		public static void Help()
		{
			System.Console.WriteLine("You can use the following commands : 1-'login'");
			System.Console.WriteLine("                                     2-'register'");
			System.Console.WriteLine("                                     3-'logout'");
			System.Console.WriteLine("                                     4-'deregister'");
			System.Console.WriteLine("                                     5-'help'");
			System.Console.WriteLine("                                     6-'exit'");
		}
		public static void Register()
		{
			String username = ReadInput("username: ");
			String password = ReadInput("password: ");

			if (username == "" || password == "") {
				Console.WriteLine("Leave no field empty!");
				return;
			}

			if (! Server.TryRegister(username, password)) {
				Console.WriteLine("The username is already in use");
				return;
			}

			return;
		}

		public static void Deregister()
		{
			if (CurrentUser == null) {
				Console.WriteLine("You must be logged in to deregister");
				return;
			}

			String username = CurrentUser.Name;
			String password = ReadInput("password: ");

			if (username == "" || password == "") {
				Console.WriteLine("Leave no field empty!");
				return;
			}

			if (! Server.TryDeregister(username, password)) {
				Console.WriteLine("Something went wrong, please try again");
				return;
			}

			CurrentUser = null;

			return;
		}

		static void Exit()
		{
			Environment.Exit(0);
		}
	}
}