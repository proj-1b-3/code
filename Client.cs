namespace App
{
	using System;
	using System.Collections.Generic;

	class Client
	{
		public delegate void Command (object res, string[] args);

		public static readonly Dictionary<String, Command>
			commands = new Dictionary<String, Command>
			{
				{ "login", Client.Login },
				{ "register", Client.Register },
				{ "logout", Client.Logout }
			};

		public static void Main(string[] args)
		{
			return; 
		}

		public static void Login (object res, string[] args)
		{
			return;
		}

		public static void Logout (object res, string[] args)
		{
			return;
		}

		public static void Register (object res, string[] args)
		{
			return;
		}
	}
}