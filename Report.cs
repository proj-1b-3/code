namespace App {
	using System;

	class Report {
		public int TicketsSold {get;set;}
		public int ConsumablesSold {get;set;}
		public float Income {get;set;}

		public Report() {}

		public Report(int ticketsSold, int consumablesSold, float income) {
			this.TicketsSold = ticketsSold;
			this.ConsumablesSold = consumablesSold;
			this.Income = income;
		}
	}
}
