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
		private static Dictionary<String, Int32> NameToId;
		private static Dictionary<Int32, UserRecord> Users;
		
		public static Int32 UserNameToId(String name)
		{
			Int32 value = 0;
			if (NameToId.TryGetValue(name, out value)) {
				return value;
			} else {
				return -1;
			}
		}
		
		public static Boolean IsActive(Int32 id)
		{
			UserRecord value;
			if (Users.TryGetValue(id, out value)) {
				return value.Active;
			} else {
				return false;
			}
		}

		public static void LogI(object user, String[] args)
		{
			return;
		}
		
		public static void LogO(object none, String[] args)
		{
			return;
		}
	}
}
