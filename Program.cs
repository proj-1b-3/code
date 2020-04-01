namespace App
{
	using System;

	static class Program
	{
		public static void Main(string[] args)
		{
			String input;
			Client.Command command;
			
			var server = new Server();
			var client = new Client();

			server.LoadData();
			client.Connect(server);

			while (! client.Stop) {
				Console.Write(">>> ");
				input = Console.ReadLine().ToLower().Trim();

				if (! client.Commands.TryGetValue(input, out command)) {
					Console.WriteLine("Invalid command");
					continue;
				}

				command();
			}

			server.SaveData();

			return;
		}
	}
}
