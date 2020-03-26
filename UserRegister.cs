namespace App
{
	using System;
	using System.Collections.Generic;

	enum Role {
		Owner,
		CafeManager,
		Manager,
		Consumer
	}

	class UserRecord
	{
		public String Name;
		public String Password;
		public Role Perms;
		public Boolean Active;
	}

	class UserRegister
	{
		public static Dictionary<String, Int32> NameToId;
		public static Dictionary<Int32, UserRecord> Users;
		
		public static Int32 UserNameToId(String name)
		{
			Int32 value = 0;
			if (NameToId.TryGetValue(name, out value)) {
				return value;
			} else {
				return -1;
			}
		}
		
		public static Boolean IsUserActive(Int32 id)
		{
			UserRecord user;
			if (Users.TryGetValue(id, out user)) {
				return user.Active;
			} else {
				return false;
			}
		}
	}
}
