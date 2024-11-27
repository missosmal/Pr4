using Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static IPAddress IpAdress;
        private Socket _clientSocket;
        public static int Port;
        public static int Id = -1;
        public MainWindow()
        {
            InitializeComponent();
        }

        public static bool CheckCommand(string message)
        {
            bool BCommand = false;
            string[] DataMessage = message.Split(new string[1] { " " }, StringSplitOptions.None);
            if (DataMessage.Length > 0)
            {
                string Command = DataMessage[0];
                if (Command == "connect")
                {
                    if (DataMessage.Length != 3)
                    {
                        MessageBox.Show($"Использование: connect [login] [password]\nПример: connect User1 Asdfg123");
                        BCommand = false;
                    }
                    else
                    {
                        BCommand = true;
                    }
                }
                else if (Command == "cd")
                {
                    BCommand = true;
                }
                else if (Command == "get")
                {
                    if (DataMessage.Length == 1)
                    {
                        MessageBox.Show($"Использование: get [NameFile]\nПример: get Test.txt");
                        BCommand = false;
                    }
                    else BCommand = true;
                }
                else if (Command == "set")
                {
                    if (DataMessage.Length == 1)
                    {
                        MessageBox.Show("Использование: set [NameFile]\nПример: set Test.txt");
                        BCommand = false;
                    }
                    else BCommand = true;
                }
            }
            return BCommand;
        }

        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string IPAdress = txtIpAddress.Text.ToString();
                int port = int.Parse(txtPort.Text);
                string login = txtLogin.Text.ToString();
                string password = txtPassword.Text.ToString();

                _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _clientSocket.Connect(IPAdress, port);

                string message = $"connect {login} {password}";
                SendMessage(message);

                string response = ReceiveMessage();
                var viewModelMessage = JsonConvert.DeserializeObject<ViewModelMessage>(response);

                if (viewModelMessage.Command == "autorization")
                {
                    Id = int.Parse(viewModelMessage.Data);
                    MessageBox.Show("Connected successfully!");
                    LoadDirectories();
                }
                else
                {
                    MessageBox.Show("Invalid login or password.");
                }
            }
            catch (Exception ex) 
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void SendMessage(string message)
        {
            var viewModelSend = new ViewModelSend(message, Id);
            string jsonMessage = JsonConvert.SerializeObject(viewModelSend);
            byte[] messageBytes = Encoding.UTF8.GetBytes(jsonMessage);
            _clientSocket.Send(messageBytes);
        }

        private string ReceiveMessage()
        {
            byte[] buffer = new byte[10485760];
            int bytesReceived = _clientSocket.Receive(buffer);
            return Encoding.UTF8.GetString(buffer, 0, bytesReceived);
        }

        private void LoadDirectories()
        {
            string message = $"cd";
            SendMessage(message);

            string response = ReceiveMessage();
            var viewModelMessage = JsonConvert.DeserializeObject<ViewModelMessage>(response);

            if (viewModelMessage.Command == "cd")
            {
                var directories = JsonConvert.DeserializeObject<List<string>>(viewModelMessage.Data);
                lstDirectories.ItemsSource = directories;
            }
            else
            {
                MessageBox.Show(viewModelMessage.Data);
            }
        }

        private void LstDirectories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstDirectories.SelectedItem != null)
            {
                string selectedDirectory = lstDirectories.SelectedItem.ToString();
                string message = $"cd {selectedDirectory}";
                SendMessage(message);

                string response = ReceiveMessage();
                var viewModelMessage = JsonConvert.DeserializeObject<ViewModelMessage>(response);

                if (viewModelMessage.Command == "cd")
                {
                    var files = JsonConvert.DeserializeObject<List<string>>(viewModelMessage.Data);
                    lstFiles.ItemsSource = files;
                }
                else
                {
                    MessageBox.Show(viewModelMessage.Data);
                }
            }
        }
    }
}
