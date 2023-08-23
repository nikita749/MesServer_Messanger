using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MesServer
{
	class User
	{
		public Dictionary<string, List<string>> chats;
		

		public User(Dictionary<string, List<string>> tmp_chats_list, string tmp_pas)
		{
			chats = tmp_chats_list;
			//password = tmp_pas;
		}
	}
}
