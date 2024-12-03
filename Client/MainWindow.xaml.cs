using Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Client
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IPAddress ipAddress;
        private int port;
        private int userId = -1;
        private Stack<string> directoryStack = new Stack<string>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (IPAddress.TryParse(txtIpAddress.Text, out ipAddress) && int.TryParse(txtPort.Text, out port))
            {
                string login = txtLogin.Text;
                string password = txtPassword.Password;
                if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Введите логин и пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                try
                {
                    var response = SendCommand($"connect {login} {password}");
                    if (response?.Command == "autorization")
                    {
                        userId = int.Parse(response.Data);
                        MessageBox.Show("Подключение успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadDirectories();
                    }
                    else
                    {
                        MessageBox.Show(response?.Data ?? "Ошибка авторизации.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка подключения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Введите корректный IP и порт.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void LoadDirectories()
        {
            var response = SendCommand("cd");
            if (response?.Command == "cd")
            {
                var directories = JsonConvert.DeserializeObject<string[]>(response.Data);
                lstDirectories.Items.Clear();
                lstFiles.Items.Clear();

                if (directoryStack.Count > 0)
                {
                    lstDirectories.Items.Add("Назад");
                }

                foreach (var dir in directories)
                {
                    lstDirectories.Items.Add(dir);
                }
            }
            else
            {
                MessageBox.Show("Не удалось загрузить список директорий.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private ViewModelMessage SendCommand(string message)
        {
            try
            {
                IPEndPoint endPoint = new IPEndPoint(ipAddress, port);
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    socket.Connect(endPoint);
                    if (socket.Connected)
                    {
                        var request = new ViewModelSend(message, userId);
                        byte[] requestBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request));
                        socket.Send(requestBytes);

                        byte[] responseBytes = new byte[10485760];
                        int receivedBytes = socket.Receive(responseBytes);
                        string responseData = Encoding.UTF8.GetString(responseBytes, 0, receivedBytes);

                        return JsonConvert.DeserializeObject<ViewModelMessage>(responseData);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка соединения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return null;
        }

        private void lstDirectories_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (lstDirectories.SelectedItem == null)
                return;

            string selectedItem = lstDirectories.SelectedItem.ToString();
            if (selectedItem == "Назад")
            {
                if (directoryStack.Count > 0)
                {
                    directoryStack.Pop();
                    LoadDirectories();
                }
            }
            else if (selectedItem.EndsWith("/"))
            {
                directoryStack.Push(selectedItem);
                var response = SendCommand($"cd {selectedItem}");
                if (response?.Command == "cd")
                {
                    var items = JsonConvert.DeserializeObject<List<string>>(response.Data);
                    if (directoryStack.Count > 0)
                    {
                        lstFiles.Items.Add("Назад");
                    }
                    foreach (var item in items)
                    {
                        lstFiles.Items.Add(item);
                    }
                }
                else
                {
                    MessageBox.Show($"Ошибка открытия директории: {response?.Data}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                GetFile(selectedItem);
            }
            //else
            //{
            //    var response = SendCommand($"get {selectedItem}");
            //    if (response?.Command == "file")
            //    {
            //        try
            //        {
            //            byte[] fileData = JsonConvert.DeserializeObject<byte[]>(response.Data);
            //            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), selectedItem);
            //            File.WriteAllBytes(filePath, fileData);
            //            MessageBox.Show($"Файл {selectedItem} успешно загружен на рабочий стол.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            //        }
            //        catch (Exception ex)
            //        {
            //            MessageBox.Show($"Ошибка при загрузке файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            //        }
            //    }
            //    else
            //    {
            //        MessageBox.Show($"Ошибка скачивания файла: {response?.Data}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            //    }
            //}
        }

        private void lstFiles_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (lstFiles.SelectedItem != null)
            {
                string selectedFile = lstFiles.SelectedItem.ToString();
                if (selectedFile == "Назад")
                {
                    if (directoryStack.Count > 0)
                    {
                        directoryStack.Pop();
                        LoadDirectories();
                    }
                }
                else if (selectedFile.EndsWith("/"))
                {
                    directoryStack.Push(selectedFile);
                    var response = SendCommand($"cd {selectedFile}");
                    if (response?.Command == "cd")
                    {
                        var items = JsonConvert.DeserializeObject<List<string>>(response.Data);
                        lstFiles.Items.Clear();
                        if (directoryStack.Count > 0)
                        {
                            lstFiles.Items.Add("Назад");
                        }
                        foreach (var item in items)
                        {
                            lstFiles.Items.Add(item);
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Ошибка открытия директории: {response?.Data}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    GetFile(selectedFile);
                }
            }

        }

        private void GetFile(string fileName)
        {
            var response = SendCommand($"get \\{fileName}");
            if (response?.Command == "file")
            {
                try
                {
                    byte[] fileData = JsonConvert.DeserializeObject<byte[]>(response.Data);
                    string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);
                    File.WriteAllBytes(filePath, fileData);
                    MessageBox.Show($"Файл {fileName} успешно загружен на рабочий стол.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show($"Ошибка скачивания файла: {response?.Data}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
