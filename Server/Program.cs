using Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Permissions;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class Program
    {
        public static List<User> Users = new List<User>();
        public static IPAddress IpAddress;
        public static int Port;

        public static bool AutorizationUser(string login, string password)
        {
            User user = null;
            user = Users.Find(x => x.login == login && x.password == password);
            return user != null;
        }

        public static List<string> GetDirectory(string src)
        {
            List<string> FoldersFiles = new List<string>();
            if (Directory.Exists(src))
            {
                string[] dirs = Directory.GetDirectories(src);
                foreach (string dir in dirs)
                {
                    string NameDirectory = dir.Replace(src, "");
                    FoldersFiles.Add(NameDirectory + "/");
                }
                string[] files = Directory.GetFiles(src);
                foreach (string file in files)
                {
                    string NameFile = file.Replace(src, "");
                    FoldersFiles.Add(NameFile);
                }
            }

            return FoldersFiles;
        }

        public static void StartServer()
        {
            // Создаем конечную точку, состоящую из IP-адреса и порта
            IPEndPoint endPoint = new IPEndPoint(IpAddress, Port);
            // Создаем сокет для прослушивания
            Socket sListener = new Socket(
                // Указываем адресную, которую будет использовать сокет IPv4
                AddressFamily.InterNetwork,
                // Указываем тип сокета, аналогич код
                SocketType.Stream,
                // Указываем протокол передачи
                ProtocolType.Tcp);
            // Связываем сокет с конечной точкой
            sListener.Bind(endPoint);
            // Устанавливаем лимит на входящие подключения
            sListener.Listen(10);
            // Устанавливаем цвет текста в консоли
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Сервер запущен...");
            // Запускаем бесконечный цикл
            while (true)
            {
                try
                {
                    // Создаём ещё один сокет для прослушивания
                    Socket Handler = sListener.Accept();
                    // Создаём переменную, которая будет принимать в себя сообщения от клиента
                    string Data = null;
                    // Объявляем массив байтов
                    byte[] Bytes = new byte[10485760];
                    // Получаем сообщение клиента и записываем байт в переменную, а затем в массив
                    int BytesRec = Handler.Receive(Bytes);
                    Data += Encoding.UTF8.GetString(Bytes, 0, BytesRec);
                    // Записываем сообщения, которые отправил пользователь
                    Console.WriteLine("Сообщение от пользователя: " + Data + "\n");
                    string Reply = "";
                    // Конвертируем сообщение клиента обратно в объект с помощью JsonConvert
                    ViewModelSend ViewModelSend = JsonConvert.DeserializeObject<ViewModelSend>(Data);
                    // Если сообщение от неизвестного пользователя
                    if (ViewModelSend == null)
                    {
                        ViewModelMessage viewModelMessage;
                        string[] DataCommand = ViewModelSend.Message.Split(new string[1] { " " }, StringSplitOptions.None);
                        // Если команда не равна connect
                        if (DataCommand[0] == "connect")
                        {
                            // Разбиваем оставшуюся часть ответа по пробелу, получая логин и пароль пользователя
                            string[] DataMessage = ViewModelSend.Message.Split(new string[1] { " " }, StringSplitOptions.None);
                            // Проверяем авторизацию на авторизацию
                            if (AutorizationUser(DataMessage[1], DataMessage[2]))
                            {
                                // В случае успеха, получаем код пользователя
                                int IdUser = Users.FindIndex(x => x.login == DataMessage[1] && x.password == DataMessage[2]);
                                // Формируем сообщение ответа, указываем команду и логин
                                viewModelMessage = new ViewModelMessage("autorization", IdUser.ToString());
                            }
                            else
                            {
                                // В случае неудачи, формируем ответ
                                viewModelMessage = new ViewModelMessage("message", "Не правильный логин и пароль пользователя.");
                            }
                            Reply = JsonConvert.SerializeObject(viewModelMessage);
                            byte[] message = Encoding.UTF8.GetBytes(Reply);
                            Handler.Send(message);
                        }
                        // Если пользователь имеет ответ cd команды, предлагается переход по категориям
                        else if (DataCommand[0] == "cd")
                        {
                            if (ViewModelSend.Id != -1)
                            {
                                // Проверяем что пользователь не равен, тогда проблема, получает пользователя по категории
                                string[] DataMessage = ViewModelSend.Message.Split(new string[] { " " }, StringSplitOptions.None);
                                List<string> FoldersFiles = new List<string>();
                                if (DataMessage.Length == 1)
                                {
                                    Users[ViewModelSend.Id].temp_src = Users[ViewModelSend.Id].src;
                                    FoldersFiles = GetDirectory(Users[ViewModelSend.Id].src);
                                }
                                else
                                {
                                    string cdFolder = "";
                                    for (int i = 1; i < DataMessage.Length; i++)
                                        if (cdFolder == "")
                                            cdFolder += DataMessage[i];
                                        else
                                            cdFolder += " " + DataMessage[i];
                                    Users[ViewModelSend.Id].temp_src = Users[ViewModelSend.Id].temp_src + cdFolder;
                                    FoldersFiles = GetDirectory(Users[ViewModelSend.Id].temp_src);
                                }
                                if (FoldersFiles.Count == 0)
                                    viewModelMessage = new ViewModelMessage("message", "Директория пуста или не существует.");
                                else
                                    viewModelMessage = new ViewModelMessage("cd", JsonConvert.SerializeObject(FoldersFiles));
                            }
                            else
                                viewModelMessage = new ViewModelMessage("message", "Необходимо авторизоваться");
                            Reply = JsonConvert.SerializeObject(viewModelMessage);
                            byte[] message = Encoding.UTF8.GetBytes(Reply);
                            Handler.Send(message);
                        }
                        else if (DataCommand[0] == "get")
                        {
                            if (ViewModelSend.Id != -1)
                            {
                                string[] DataMessage = ViewModelSend.Message.Split(new string[1] { " " }, StringSplitOptions.None);
                                string getFile = "";
                                for (int i = 1; i < DataMessage.Length; i++)
                                    if (getFile == "")
                                        getFile += DataMessage[i];
                                    else
                                        getFile += " " + DataMessage[i];
                                byte[] byteFile = File.ReadAllBytes(Users[ViewModelSend.Id].temp_src + getFile);
                                viewModelMessage = new ViewModelMessage("file", JsonConvert.SerializeObject(byteFile));
                            }
                            else
                                viewModelMessage = new ViewModelMessage("message", "Необходимо авторизоваться");
                            Reply = JsonConvert.SerializeObject(viewModelMessage);
                            byte[] message = Encoding.UTF8.GetBytes(Reply);
                            Handler.Send(message);
                        }
                        else
                        {
                            if (ViewModelSend.Id != -1)
                            {
                                FileInfoFTP SendFileInfo = JsonConvert.DeserializeObject<FileInfoFTP>(ViewModelSend.Message);
                                File.WriteAllBytes(Users[ViewModelSend.Id].temp_src + @"\" + SendFileInfo.Name, SendFileInfo.Data);
                                viewModelMessage = new ViewModelMessage("message", "Файл загружен");
                            }
                            else
                            {
                                viewModelMessage = new ViewModelMessage("message", "Необходимо авторизоваться");
                                Reply = JsonConvert.SerializeObject(viewModelMessage);
                                byte[] message = Encoding.UTF8.GetBytes(Reply);
                                Handler.Send(message);
                            }
                        }
                    }
                }
                catch (Exception exp)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Что-то случилось: " + exp.Message);
                }
            }
        }

        static void Main(string[] args)
        {
            Users.Add(new User("aooshchepkov", "Asdfg123", @"A:\Авиатехникум"));
            Console.Write("Введите IP адрес сервера: ");
            string sIdAddress = Console.ReadLine();
            Console.WriteLine("Введите порт: ");
            string sPort = Console.ReadLine();
            if(int.TryParse(sPort, out Port) && IPAddress.TryParse(sIdAddress, out IpAddress))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Данные успешно введены. Запускаю сервер.");
                StartServer();
            }
            Console.Read();
        }
    }
}






