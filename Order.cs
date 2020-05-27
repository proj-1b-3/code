namespace App
{
	using System;
	
	class Order
	{
		public string Country {get;set;}
		public string PostalCode {get;set;}
		public string City {get;set;}
		public Reservation Reservation {get;set;}

		public Order() {}

		public Order(string country, string postal_code, string city, Reservation reservation)
		{
			this.Country = country;
			this.PostalCode = postal_code;
			this.City = city;
			this.Reservation = reservation;
		}
	}
}
