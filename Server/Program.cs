using Common;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using static System.Environment;

namespace Server
{
    public class Program
    {
        public static List<User> Users = new List<User>();
        public static IPAddress IpAddress;
        public static int Port;
        private static string connectionString = "Server=localhost;port=3307;Database=ftp_data;uid=root;pwd=;";
        public static bool AuthenticateUser(string login, string password)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT COUNT(*) FROM users WHERE login = @login AND password = @password";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@login", login);
                    cmd.Parameters.AddWithValue("@password", password);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        public static bool AutorizationUser(string login, string password, out int userId)
        {
            userId = -1;
            User user = Users.Find(x => x.login == login && x.password == password);
            if (user != null)
            {
                userId = user.id;
                return true;
            }
            return false;
        }

        public static List<string> GetDirectory(string src)
        {
            List<string> FoldersFiles = new List<string>();
            if (Directory.Exists(src))
            {
                string[] dirs = Directory.GetDirectories(src);
                foreach (string dir in dirs)
                {
                    FoldersFiles.Add(dir + "\\");
                }
                string[] files = Directory.GetFiles(src);
                foreach (string file in files)
                {
                    FoldersFiles.Add(file);
                }
            }
            return FoldersFiles;
        }

        public static void StartServer()
        {
            IPEndPoint endPoint = new IPEndPoint(IpAddress, Port);
            Socket sListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sListener.Bind(endPoint);
            sListener.Listen(10);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Сервер запущен...");

            while (true)
            {
                Socket handler = null;
                try
                {
                    handler = sListener.Accept();
                    string data = null;
                    byte[] bytes = new byte[10485760];
                    int bytesRec = handler.Receive(bytes);
                    data += Encoding.UTF8.GetString(bytes, 0, bytesRec);
                    Console.WriteLine("Сообщение от пользователя: " + data + "\n");
                    string reply = "";

                    ViewModelSend viewModelSend = JsonConvert.DeserializeObject<ViewModelSend>(data);
                    if (viewModelSend != null)
                    {
                        ViewModelMessage viewModelMessage;
                        string[] dataCommand = viewModelSend.Message.Split(new string[1] { " " }, StringSplitOptions.None);
                        if (dataCommand[0] == "connect")
                        {
                            string[] dataMessage = viewModelSend.Message.Split(new string[1] { " " }, StringSplitOptions.None);
                            if (AutorizationUser(dataMessage[1], dataMessage[2], out int userId))
                            {
                                userId = Users.Find(x => x.login == dataMessage[1] && x.password == dataMessage[2]).id;
                                viewModelMessage = new ViewModelMessage("autorization", userId.ToString());
                                string username = Users.Find(x => x.login == dataMessage[1] && x.password == dataMessage[2]).login;
                                string password = Users.Find(x => x.login == dataMessage[1] && x.password == dataMessage[2]).password;
                                LogCommandToDatabase(userId, viewModelSend.Message.Split(' ')[0]);
                            }
                            else
                            {
                                viewModelMessage = new ViewModelMessage("message", "Неправильный логин и пароль пользователя.");
                            }
                            reply = JsonConvert.SerializeObject(viewModelMessage);
                            byte[] message = Encoding.UTF8.GetBytes(reply);
                            handler.Send(message);
                        }
                        else if (dataCommand[0] == "cd")
                        {
                            if (viewModelSend.Id != -1)
                            {
                                string[] dataMessage = viewModelSend.Message.Split(new string[1] { " " }, StringSplitOptions.None);
                                List<string> foldersFiles = new List<string>();
                                if (dataMessage.Length == 1)
                                {
                                    Users[viewModelSend.Id].temp_src = Users[viewModelSend.Id - 1].src;
                                    foldersFiles = GetDirectory(Users[viewModelSend.Id - 1].src);
                                }
                                else
                                {
                                    string cdFolder = string.Join(" ", dataMessage.Skip(1));
                                    Users[viewModelSend.Id - 1].temp_src = Path.Combine(Users[viewModelSend.Id - 1].temp_src, cdFolder);
                                    foldersFiles = GetDirectory(Users[viewModelSend.Id - 1].temp_src);
                                }
                                if (foldersFiles.Count == 0)
                                    viewModelMessage = new ViewModelMessage("message", "Директория пуста или не существует.");
                                else
                                    viewModelMessage = new ViewModelMessage("cd", JsonConvert.SerializeObject(foldersFiles));
                                string username = Users[viewModelSend.Id - 1].login;
                                string password = Users[viewModelSend.Id - 1].password;
                                var userId = Users.Find(x => x.login == username && x.password == password).id;
                                LogCommandToDatabase(userId, viewModelSend.Message.Split(' ')[0]);
                            }
                            else
                            {
                                viewModelMessage = new ViewModelMessage("message", "Необходимо авторизоваться");
                            }
                            reply = JsonConvert.SerializeObject(viewModelMessage);
                            byte[] message = Encoding.UTF8.GetBytes(reply);
                            handler.Send(message);
                        }
                        else if (dataCommand[0] == "get")
                        {
                            if (viewModelSend.Id != -1)
                            {
                                string[] dataMessage = viewModelSend.Message.Split(new string[1] { " " }, StringSplitOptions.None);
                                string getFile = string.Join(" ", dataMessage.Skip(1));
                                string fullFilePath = Path.Combine(Users[viewModelSend.Id].temp_src, getFile);
                                Console.WriteLine($"Trying to access file: {fullFilePath}");
                                if (File.Exists(fullFilePath))
                                {
                                    byte[] byteFile = File.ReadAllBytes(fullFilePath);
                                    viewModelMessage = new ViewModelMessage("file", JsonConvert.SerializeObject(byteFile));
                                    string username = Users[viewModelSend.Id - 1].login;
                                    string password = Users[viewModelSend.Id - 1].password;
                                    var userId = Users.Find(x => x.login == username && x.password == password).id;
                                    LogCommandToDatabase(userId, "get");
                                }
                                else
                                {
                                    viewModelMessage = new ViewModelMessage("message", "Необходимо авторизоваться");
                                }
                            reply = JsonConvert.SerializeObject(viewModelMessage);
                            byte[] message = Encoding.UTF8.GetBytes(reply);
                            handler.Send(message);
                            }
                        }
                        else
                        {
                            if (viewModelSend.Id != -1)
                            {
                                string username = Users[viewModelSend.Id - 1].login;
                                string password = Users[viewModelSend.Id - 1].password;
                                var userId = Users.Find(x => x.login == username && x.password == password).id;
                                FileInfoFTP SendFileInfo = JsonConvert.DeserializeObject<FileInfoFTP>(viewModelSend.Message);
                                string savePath = Path.Combine(Users[viewModelSend.Id - 1].temp_src, SendFileInfo.Name);
                                File.WriteAllBytes(savePath, SendFileInfo.Data);
                                viewModelMessage = new ViewModelMessage("message", "Файл загружен");
                                LogCommandToDatabase(userId, viewModelSend.Message.Split(' ')[0]);
                            }
                            else
                            {
                                viewModelMessage = new ViewModelMessage("message", "Необходимо авторизоваться");
                            }
                            reply = JsonConvert.SerializeObject(viewModelMessage);
                            byte[] message = Encoding.UTF8.GetBytes(reply);
                            handler.Send(message);
                        }
                    }
                }
                catch (Exception exp)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Что-то случилось: " + exp.Message);
                }
                finally
                {
                    if (handler != null && handler.Connected)
                    {
                        handler.Shutdown(SocketShutdown.Both);
                        handler.Close();
                    }
                }
            }
        }

        private static void LogCommandToDatabase(int userId, string command)
        {
            string connectionString = "Server=localhost;port=3307;Database=ftp_data;uid=root;pwd=;";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "INSERT INTO history (userId, command, sendDate) VALUES (@userId, @command, @sendDate)";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("userId", userId);
                    cmd.Parameters.AddWithValue("@command", command);
                    cmd.Parameters.AddWithValue("@sendDate", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static void LoadUsersFromDatabase()
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT id, login, password FROM users";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32("id");
                            string login = reader["login"].ToString();
                            string password = reader["password"].ToString();
                            Users.Add(new User(id, login, password, $@"{Environment.GetFolderPath(SpecialFolder.Desktop)}"));
                        }
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            LoadUsersFromDatabase();
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






