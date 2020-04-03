namespace App
{
	using System;

	class User
	{
		private String _UserName;
		public String UserName {
			get { return _UserName; }
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
			_UserName = username;
			_SessionToken = session_token;
			_Role = role;
		}
	}
}
