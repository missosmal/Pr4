using Common;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

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
                string query = "SELECT COUNT(*) FROM history WHERE username = @login AND password = @password";
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
                                userId = Users.FindIndex(x => x.login == dataMessage[1] && x.password == dataMessage[2]);
                                viewModelMessage = new ViewModelMessage("autorization", userId.ToString());
                                string username = Users[userId].login;
                                string password = Users[userId].password;
                                LogCommandToDatabase(username, password, viewModelSend.Message.Split(' ')[0]);
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
                                    Users[viewModelSend.Id].temp_src = $"C:\\"; // Значение по умолчанию
                                    foldersFiles = GetDirectory(Users[viewModelSend.Id].src);
                                }
                                else
                                {
                                    string cdFolder = "";
                                    for (int i = 1; i < dataMessage.Length; i++)
                                    {
                                        if (cdFolder == "")
                                            cdFolder += dataMessage[i];
                                        else cdFolder += " " + dataMessage[i];
                                    }
                                    Users[viewModelSend.Id].temp_src = Users[viewModelSend.Id].temp_src + cdFolder;
                                    foldersFiles = GetDirectory(Users[viewModelSend.Id].temp_src);
                                }
                                if (foldersFiles.Count == 0)
                                    viewModelMessage = new ViewModelMessage("message", "Директория пуста или не существует.");
                                else
                                    viewModelMessage = new ViewModelMessage("cd", JsonConvert.SerializeObject(foldersFiles));
                                string username = Users[viewModelSend.Id].login;
                                string password = Users[viewModelSend.Id].password;
                                LogCommandToDatabase(username, password, viewModelSend.Message.Split(' ')[0]);
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
                                string getFile = "";
                                for (int i = 1; i < dataMessage.Length; i++)
                                    if (getFile == "")
                                        getFile += dataMessage[i];
                                    else
                                        getFile += " " + dataMessage[i];
                                byte[] byteFile = File.ReadAllBytes(Users[viewModelSend.Id].temp_src + getFile);
                                viewModelMessage = new ViewModelMessage("file", JsonConvert.SerializeObject(byteFile));
                                string username = Users[viewModelSend.Id].login;
                                string password = Users[viewModelSend.Id].password;
                                LogCommandToDatabase(username, password, viewModelSend.Message.Split(' ')[0]);
                            }
                            else
                            {
                                viewModelMessage = new ViewModelMessage("message", "Необходимо авторизоваться");
                            }
                            reply = JsonConvert.SerializeObject(viewModelMessage);
                            byte[] message = Encoding.UTF8.GetBytes(reply);
                            handler.Send(message);
                        }
                        else
                        {
                            if (viewModelSend.Id != -1)
                            {
                                string username = Users[viewModelSend.Id].login;
                                string password = Users[viewModelSend.Id].password;
                                FileInfoFTP sendFileInfo = JsonConvert.DeserializeObject<FileInfoFTP>(viewModelSend.Message);
                                File.WriteAllBytes(Users[viewModelSend.Id].temp_src + @"\" + sendFileInfo.Name, sendFileInfo.Data);
                                viewModelMessage = new ViewModelMessage("message", "Файл загружен");
                                LogCommandToDatabase(username, password, viewModelSend.Message.Split(' ')[0]);
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

        private static void LogCommandToDatabase(string username, string password, string command)
        {
            string connectionString = "Server=localhost;port=3307;Database=ftp_data;uid=root;pwd=;";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "INSERT INTO history (username, password, command, sendDate) VALUES (@username, @password, @command, @sendDate)";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);
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
                string query = "SELECT id, username, password FROM history";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32("id");
                            string login = reader["username"].ToString();
                            string password = reader["password"].ToString();
                            Users.Add(new User(id, login, password, $@"C:\"));
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






