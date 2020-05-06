namespace App
{
	using System;

	class User
	{
		public String UserName {get;set;}
		public Role Role {get;set;}
		public Guid SessionToken {get;set;}

		public User() {}

		public User(String userName, Guid sessionToken, Role role)
		{
			this.UserName = userName;
			this.SessionToken = sessionToken;
			this.Role = role;
		}
	}
}
