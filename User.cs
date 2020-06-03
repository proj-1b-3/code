namespace App {
	using System;

	class User {
		public string UserName {get;set;}
		public Role Role {get;set;}
		public Guid SessionToken {get;set;}

		public User() {}

		public User(string user_name, Guid session_token, Role role) {
			this.UserName = user_name;
			this.SessionToken = session_token;
			this.Role = role;
		}
	}
}
