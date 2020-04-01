namespace App
{
	using System;

	class User
	{
		private String _UserName;
		public String UserName {
			get { return _UserName; }
		}

		private Guid _SessionToken;
		public Guid SessionToken {
			get { return _SessionToken; }
		}

		public User(String username, Guid session_token)
		{
			_UserName = username;
			_SessionToken = session_token;
		}
	}
}
