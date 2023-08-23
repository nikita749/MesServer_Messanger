using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MesServer
{
    class communications
	{
		//==== сообщения серверу о тех или иных действиях(на будующие: нужно поискать символы которые меньше подвержаны искажению) =====
		public const string mes_for_ser_about_reg = "r"; // предупреждение о намерении зарегаться 
		public const string mes_for_ser_about_log = "l"; // предупреждение о дальнешем входе
		public const string mes_for_ser_about_log_get_new_mes = "new message"; // предупреждение о дальнешем входе
		public const string mes_for_ser_about_get_all_data = "give all data"; // хочу получить все данные о моих чатах
		public const string mes_for_ser_about_get_latest_data = "give latest data"; // хочу получить всепоследнии данные о моих чатах

		// ======================= ответы от сервера о тех или иных действиях ==============

		//    === регистрация ===
		public  const string answer_registration_is_ok = "ok";  // "ok" - вы успешно зарегистрируйтесь
		public  const string answer_login_busy = "busy"; // "busy" - логин уже занят
		//    ===================

		//    = вход =
		public  const string answer_you_are_logged = "welcome";// "welcome" - вы успешно вошли в аккаунт
		public  const string answer_not_found = "not found"; // "not found" такого логина несушествует или указан не верный пароль
		//    ========
	}
}










































//copyright © by kiratechnic