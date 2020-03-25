using System;
using System.Collections.Generic;

namespace App
{
    class Program
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
