namespace App {
	using System;
	using System.Data;

	class Room : Product {
		public String Theme {get;set;}
		public Int32 Capacity {get;set;}
		public Int32 NumberOfRounds {get;set;}
		public Int32 MaxDuration {get;set;}

		public Room() {}

		public Room(string name, string theme, string desc, int cap, float price, int n_rnd, int max_dur)
				: base(-1, name, desc, price, true) {
			this.Theme = theme;
			this.Capacity = cap;
			this.NumberOfRounds = n_rnd;
			this.MaxDuration = max_dur;
		}

		public Room(DataRow prodRow, DataRow attrRow) {
			this.ProductId = (long)prodRow["ProductId"];
			this.Name = (string)prodRow["ProductName"];
			this.Description = (string)prodRow["Description"];
			this.Price = (float)prodRow["Price"];
			this.Available = (bool)prodRow["Available"];
			this.Theme = (string)attrRow["Theme"];
			this.Capacity = (int)attrRow["Capacity"];
			this.NumberOfRounds = (int)attrRow["NumberOfRounds"];
			this.MaxDuration = (int)attrRow["MaxDuration"];
		}

		public Room Clone() {
			var obj = new Room();
			obj.ProductId = this.ProductId;
			obj.Name = this.Name;
			obj.Description = this.Description;
			obj.Price = this.Price;
			obj.Available = this.Available;
			obj.Theme = this.Theme;
			obj.Capacity = this.Capacity;
			obj.NumberOfRounds = this.NumberOfRounds;
			obj.MaxDuration = this.MaxDuration;
			return obj;
		}
	}
}
