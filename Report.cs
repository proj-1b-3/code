namespace App
{
	using System;

	class Report
	{
		public Int32 TicketsSold {get;set;}
		public Int32 ConsumablesSold {get;set;}
		public Single Income {get;set;}

		public Report() {}

		public Report(Int32 ticketsSold, Int32 consumablesSold, Single income)
		{
			this.TicketsSold = ticketsSold;
			this.ConsumablesSold = consumablesSold;
			this.Income = income;
		}
	}
}
