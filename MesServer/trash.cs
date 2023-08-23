//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace MesServer
//{
//    class trash
//    {
//		private Dictionary<string, User> users = new Dictionary<string, User>() {
//			{ "1", new User(new Dictionary<string, List<string>>() {
//				{"2", new List<string>(){"2 в 18:20\nпривет!", "вы в 18:21\nну привет"} } }, "111")
//			},
//			{ "2", new User(new Dictionary<string, List<string>>() {
//				{"1", new List<string>(){"вы в 18:20\nпривет!", "1 в 18:21\nну привет"} } }, "222")
//			}
//		};

//		const int number_of_attempts = 3; //кол-во попыток входа

//		NetworkStream ns;
//		BinaryReader br;
//		BinaryWriter bw;


//		private ICollection<TcpClient> clients = new List<TcpClient>();//коллекция подключеных клиентов
//		private const int serverPort = 1648; //порт подключения


//		//Enqueue("Hello"); Dequeue



//		//==== сообщения серверу о тех или иных действиях (на будующие: нужно поискать символы которые меньше подвержаны искажению) =====
//		private const string mes_for_ser_about_reg = "r"; // предупреждение о намерении зарегаться 
//		private const string mes_for_ser_about_log = "l"; // предупреждение о дальнешем входе
//														  //================================================================================================================================


//		// ======================= ответы от сервера о тех или иных действиях ==============

//		//    === регистрация ===
//		private const string answer_registration_is_ok = "ok";  // "ok" - вы успешно зарегистрируйтесь
//		private const string answer_login_busy = "busy"; // "busy" - логин уже занят
//														 //    ===================

//		//    = вход =
//		private const string answer_you_are_logged = "welcome";// "welcome" - вы успешно вошли в аккаунт
//		private const string answer_not_found = "not found"; // "not found" такого логина несушествует или указан не верный пароль
//															 //    ========

//		// =================================================================================





//		static void Main(string[] args) => new Program().Run().Wait();//запускаем функцию Run в отдельном потоке после ждем её завершения

//		private async Task Run()//восновно происходит инициализация или запуск основных потоков
//		{
//			TcpListener listen = new TcpListener(IPAddress.Loopback, serverPort);//локальный ip-адрес выставляется автоматически (запуск слушающего сокета на порт ранее указаный
//			listen.Start();//запуск слушающего сокета

//			Console.WriteLine("Слушаю взодящее соединение на порт 1648");//если все успешно прошло выведет

//			WaitIcomingConnectionsAsync(listen);//функция если подключился пользователь, его отправит на другой сокет через который сервер будет с ним объщаться

//			Console.WriteLine("Нажмите любую клавишу для завершения");
//			Console.ReadKey();
//			Console.WriteLine("End!");
//		}
//		private async void WaitIcomingConnectionsAsync(TcpListener listen)//перекидывает на новый сокет пользователя и открывает так называемый "диалог" с юсером
//		{
//			while (true)
//			{
//				TcpClient newClient = await listen.AcceptTcpClientAsync();//
//				lock (clients)
//					clients.Add(newClient);
//				ServerClient(newClient);
//			}
//		}


//		private async void ServerClient(TcpClient newClient)
//		{
//			try
//			{
//				ns = newClient.GetStream();
//				br = new BinaryReader(ns);
//				bw = new BinaryWriter(ns);
//				{
//					string action_tmp = br.ReadString();
//					if (action_tmp == mes_for_ser_about_reg)//регистрация
//					{
//						string user_login = br.ReadString();
//						string user_pass = br.ReadString();
//						if (users.ContainsKey(user_login))//проверка существует ли такой логин, в моем случае просто поиск по деректории 
//						{//если существует то сервак отправляет клиенту что нужно переделать
//							bw.Write(answer_login_busy);// "busy" - логин занят
//						}
//						else
//						{
//							try
//							{
//								users.Add(user_login, new User(new Dictionary<string, List<string>>(), user_pass));
//								bw.Write(answer_registration_is_ok);
//							}
//							catch (Exception ex)
//							{
//								bw.Write(ex.Message);//ответ от сервера если пошло что-то не так
//													 //bw.Write("перезапустить предложение");
//													 //return; //завкрывает поток параллельно, отключает пользователя
//							}
//						}
//					}
//					else if (action_tmp == mes_for_ser_about_log)//проверка логина пароля
//					{
//						string user_login = br.ReadString();
//						string user_pass = br.ReadString();
//						if (!users.ContainsKey(user_login))//проверка существует ли такой логин, в моем случае просто поиск по деректории 
//						{//если существует то сервак отправляет клиенту что нужно переделать
//							bw.Write(answer_not_found);// "not found" - логин не найден
//						}
//						else
//						{
//							try
//							{
//								bw.Write(answer_you_are_logged);
//							}
//							catch (Exception ex)
//							{
//								bw.Write(ex.Message);
//							}
//						}
//					}
//				}
//				for (int i = 0; i < number_of_attempts; i++)//предахронитель если больше n-ого кол-ва раз пытается зайти то дает кд в приложении пользователя
//				{

//				}
//			}
//			catch (Exception)
//			{
//				Console.Write("соединение с одним из клиентов было разорвано! стараюсь убрать его из очереди, итог-> ");
//				lock (clients)
//					clients.Remove(newClient);
//				Console.WriteLine("успешно");
//			}
//		}
//		private async void update_chats(string user_login)
//		{
//			byte[] byteArray = ObjectToByteArray(users[user_login].chats);
//			int len_bytes_arr = byteArray.Length;

//		}
//		private Object ByteArrayToObject(byte[] arrBytes)
//		{
//			MemoryStream memStream = new MemoryStream();
//			BinaryFormatter binForm = new BinaryFormatter();
//			memStream.Write(arrBytes, 0, arrBytes.Length);
//			memStream.Seek(0, SeekOrigin.Begin);
//			Object obj = (Object)binForm.Deserialize(memStream);
//			return obj;
//		}
//		private byte[] ObjectToByteArray(Object obj)
//		{
//			if (obj == null)
//				return null;
//			BinaryFormatter bf = new BinaryFormatter();
//			MemoryStream ms = new MemoryStream();
//			bf.Serialize(ms, obj);
//			return ms.ToArray();
//		}
//	}





//Console.WriteLine(Assembly.GetExecutingAssembly().GetName().Name); //возвращает имя exe файла 
//Process.Start($"{Assembly.GetExecutingAssembly().GetName().Name}.exe {string.Join(" ", args)}"); //планировалась использовать для рестарта сервера через консоль его-же но при открытии через EXE ошибка что путь не найден