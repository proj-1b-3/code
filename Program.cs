namespace App
{
	using System;

	class Program
	{
		public static void Main(string[] args)
		{
			String input;
			Client.Command command;
			
			var client = new Client();
			client.Connect();

			while (! client.Stop) {
				Console.Write(">>> ");
				input = Console.ReadLine().ToLower().Trim();

				if (! client.Commands.TryGetValue(input, out command)) {
					Console.WriteLine("Invalid command");
				}

				command();
			}

			client.Disconnect();

			return;
		}
	}
}
