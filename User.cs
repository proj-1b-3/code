namespace App
{
	using System;

	class User
	{
		private String _Name;
		public String Name {
			get { return _Name; }
		}

		private Role _Role;
		public Role Role {
			get { return _Role; }
		}

		private Guid _SessionToken;
		public Guid SessionToken {
			get { return _SessionToken; }
		}

		public User(String username, Guid session_token, Role role)
		{
			_Name = username;
			_SessionToken = session_token;
			_Role = role;
		}
	}
}
