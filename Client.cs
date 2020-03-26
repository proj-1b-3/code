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
					UserRegister.LogI
				},
				{
					"reserve",
					UserRegister.LogI
				},
				{
					"logout",
					UserRegister.LogO
				}
			};

        public static void Main(string[] args)
        {
			return; 
        }
    }
}