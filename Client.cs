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
				{
					"login",
					Client.Login
				},
				{
					"Register",
					Client.Register
				},
				{
					"logout",
					Client.Logout
				}
			};

        public static void Main(string[] args)
        {
			return; 
        }
    }
}