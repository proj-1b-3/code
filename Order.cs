namespace App {
	using System;
	
	class Order {
		// public string Forename {get;set;}
		// public string Surname {get;set;}
		public string Country {get;set;}
		public string PostalCode {get;set;}
		public string City {get;set;}
		public Int64 CardNumber{get;set;}
		public Reservation Reservation {get;set;}

		public Order() {}

		public Order(string country, string postal_code, string city, Int64 CardNumber, Reservation reservation) {
			this.Country = country;
			this.PostalCode = postal_code;
			this.City = city;
			this.CardNumber = CardNumber;
			this.Reservation = reservation;
		}
	}
}
