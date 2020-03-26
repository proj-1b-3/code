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
					Server.Login
				},
				{
					"Register",
					Server.Register
				},
				{
					"reserve",
					Server.Reserve
				},
				{
					"CancelReservation",
					Server.DelReservation
				},
				{
					"AddProduct",
					Server.AddProduct
				},
				{
					"RemoveProduct",
					Server.DelProduct
				},
				{
					"logout",
					Server.Logout
				}
			};

        public static void Main(string[] args)
        {
			return; 
        }
    }
}