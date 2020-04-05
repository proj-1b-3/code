namespace App
{
	using System;

	static class Program
	{
		public static void Main(string[] args)
		{
			var server = new Server();
			var client = new Client();

			server.LoadData();
			client.Begin(server);
			server.SaveData();

			return;
		}
	}
}
