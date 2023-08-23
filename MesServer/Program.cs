//using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;


using static MesServer.communications;

namespace MesServer
{
	class Program
	{
        
        /// //////////////////////
        public List<Users> users_from_db = new List<Users>(); // ЛИСТ ЮЗЕРОВ ИЗ ДБ
        /// //////////////////////


        static void Main(string[] args) 
        {
			string[] Allcommand =  { "stop - Server stop", "help - Output of all available commands", "stsrt - launching utilities for interacting with the client" };
			Console.WriteLine($"loading with arguments:\n{string.Join(" - ", args)}");

			string command = "";

			Console.WriteLine("server started");

			do
			{
				Console.Write("> ");
                command = Console.ReadLine();
				
                switch (command)
				{
					case "start":
						new Program().Run().Wait();//запускаем функцию Run в отдельном потоке после ждем её завершения
						break;
					case "help":
						Console.WriteLine($"\t{string.Join("\n\t", Allcommand)}");
						break;
					case "stop":
						break;
					default:
						Console.WriteLine("\tthis command is not found\n\ttype 'help' to see all possible commands");
						break;
				}
			} while (command != "stop");

			Console.WriteLine("Server is shutting down");
        }


		private const int serverPort = 1648; //порт подключения

        private async Task Run()//восновно происходит инициализация или запуск основных потоков
        {
            TcpListener listen = new TcpListener(IPAddress.Loopback, serverPort);//локальный ip-адрес выставляется автоматически (запуск слушающего сокета на порт ранее указаный
            listen.Start();//запуск слушающего сокета
            Console.WriteLine($"\tlistening for incoming connections on a port {serverPort}");

            WaitIcomingConnectionsAsync(listen);//функция если подключился пользователь, его отправит на другой сокет через который сервер будет с ним объщаться
            Console.WriteLine($"\twaiting for incoming connection");


            string[] Allcommand = { "socket close - stop socket, and return to server main menu", "help - Output of all available commands", "stsrt - launching utilities for interacting with the client" };
			string command = "";
			do{
				Console.Write("> ");
				command = Console.ReadLine();

				switch (command)
				{
					case "help":
						Console.WriteLine($"\t{string.Join("\n\t", Allcommand)}");
						break;
					case "socket close":
						break;
					default:
						Console.WriteLine("\tthis command is not found\n\ttype 'help' to see all possible commands");
						break;
				}
			} while (command != "socket close");
			Console.Write("\tClosing the listening socket!");
		}


        private ICollection<TcpClient> clients = new List<TcpClient>();//коллекция подключеных клиентов

        private async void WaitIcomingConnectionsAsync(TcpListener listen)//перекидывает на новый сокет пользователя и открывает так называемый "диалог" с юсером
        {
            while (true)
            {
                TcpClient newClient = await listen.AcceptTcpClientAsync();//
                lock (clients)
                    clients.Add(newClient);
                ServerClient(newClient);
            }
        }

        private const int number_of_login_attempts = 3;//кол-во попыток
        
        private async void ServerClient(TcpClient newClient)
        {
            NetworkStream ns;
            BinaryReader br;
            BinaryWriter bw;
            try
            {
                ns = newClient.GetStream();
                br = new BinaryReader(ns);
                bw = new BinaryWriter(ns);
            }
            catch (Exception)
            {
                Console.Write("соединение с одним из клиентов было разорвано! стараюсь убрать его из очереди, итог-> ");
                lock (clients)
                    clients.Remove(newClient);
                Console.WriteLine("успешно");
                return;
            }

            int number_of_attempts = 0;
            do{
                try
                {
                    {
                        string action_tmp = br.ReadString();
                        if (action_tmp == mes_for_ser_about_reg)//регистрация
                        {
                            string user_login = br.ReadString();
                            string user_pass = br.ReadString();
                            string user_email = br.ReadString();  
                            if (users.ContainsKey(user_login))//проверка существует ли такой логин, в моем случае просто поиск по деректории 
                            {//если существует то сервак отправляет клиенту что нужно переделать
                                bw.Write(answer_login_busy);// "busy" - логин занят
                            }
                            else
                            {
                                try
                                {
                                    users.Add(user_login, new User(new Dictionary<string, List<string>>(), user_pass));

                                    ////////////////
                                    //добавление нового юзера в бвазу
                                    DB_PROJECTEntities db = new DB_PROJECTEntities();
                                    Users new_user = new Users();
                                    
                                    foreach (var item in users_from_db) 
                                    {
                                        new_user.Id = item.Id + 1;
                                    }
                                    new_user.Login = user_login;
                                    new_user.Email = user_email;
                                    new_user.Password = user_pass;
                                    /////////////////


                                    bw.Write(answer_registration_is_ok);
                                }
                                catch (Exception ex)
                                {
                                    bw.Write(ex.Message);//ответ от сервера если пошло что-то не так
                                    //по идеи если ошибка не связанна с соединение сервера то последни предаст вчем проблема
                                    //если с соединение, то ошибку словит второй try
                                }
                            }
                        }
                        else if (action_tmp == mes_for_ser_about_log)//вход
                        {
                            number_of_attempts += 1;
                            string user_login = br.ReadString();
                            string user_pass = br.ReadString();
                            if (!users.ContainsKey(user_login))//проверка существует ли такой логин, в моем случае просто поиск по деректории 
                            {//если существует то сервак отправляет клиенту что нужно переделать
                                bw.Write(answer_not_found);// "not found" - логин не найден
                            }
                            else
                            {
                                try
                                {
                                    bw.Write(answer_you_are_logged);
                                }
                                catch (Exception ex)
                                {
                                    bw.Write(ex.Message);
                                    //по идеи если ошибка не связанна с соединение сервера то последни предаст вчем проблема
                                    //если с соединение, то ошибку словит второй try
                                }
                            }
                        }
                    }
                    if (number_of_attempts >= number_of_login_attempts)// если колличество попыток привышено то отключение клиента
                    {
                        break;
                    }
                }
                catch (Exception)
                {
                    Console.Write("соединение с одним из клиентов было разорвано! стараюсь убрать его из очереди, итог-> ");
                    lock (clients)
                        clients.Remove(newClient);
                    Console.WriteLine("успешно");
                    return;
                }
            } while (number_of_attempts != -1);//пропускает только если пользователь прошел если нет то рвет с ним соединение

            if (number_of_attempts == -1)
            {
                try
                {
                    do{
                        switch (br.ReadString())
                        {
                            case mes_for_ser_about_get_latest_data:
                                break;
                            case mes_for_ser_about_log_get_new_mes:
                                break;
                            case mes_for_ser_about_get_all_data:
                                break;
                        }
                    } while (true);
                }
                catch (Exception)
                {
                    Console.Write("отключение клиента! стараюсь убрать его из очереди, итог-> ");
                    lock (clients)
                        clients.Remove(newClient);
                    Console.WriteLine("успешно");
                }
            }


            //если ошибок небыло то перед закрытием потока уберет пользователя из списка
            Console.Write("соединение с одним из клиентов было разорвано! стараюсь убрать его из очереди, итог-> ");
            lock (clients)
                clients.Remove(newClient);
            Console.WriteLine("успешно");
        }
        private void update_chats(string user_login)//в разработке
        {
            byte[] byteArray = ObjectToByteArray(users[user_login].chats);
            int len_bytes_arr = byteArray.Length;
        }
        private Object ByteArrayToObject(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            Object obj = (Object)binForm.Deserialize(memStream);
            return obj;
        }
        private byte[] ObjectToByteArray(Object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }


        private Dictionary<string, User> users = new Dictionary<string, User>() {
            { "1", new User(new Dictionary<string, List<string>>() {
                {"2", new List<string>(){"2 в 18:20\nпривет!", "вы в 18:21\nну привет"} } }, "111")
            },
            { "2", new User(new Dictionary<string, List<string>>() {
                {"1", new List<string>(){"вы в 18:20\nпривет!", "1 в 18:21\nну привет"} } }, "222")
            }
        };
    














    }//program
}//namespase messerver























































//copyright © by kiratechnic