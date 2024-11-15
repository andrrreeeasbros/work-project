using System;
using System.IO;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DirectShowLib;
using Emgu.CV;

namespace WinMain
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            System.Diagnostics.Process.Start("../BuildApp/My project.exe");
            InitializeComponent();
        }

        private void HideButtonsAnimation_Completed(object sender, EventArgs e)
        {
            Camera1Button.Visibility = Visibility.Collapsed;
            Camera2Button.Visibility = Visibility.Collapsed;
        }
        private void Camera1Button_Click(object sender, RoutedEventArgs e)
        {
            SendMessageToUnity("SwitchCamera:2");
        }

        private void Camera2Button_Click(object sender, RoutedEventArgs e)
        {
            SendMessageToUnity("SwitchCamera:1");
        }
        private void SendMessageToUnity(string message)
        {
            try
            {
                var client = new TcpClient("127.0.0.1", 5000);  // Соединение с Unity сервером
                var stream = client.GetStream();
                var writer = new StreamWriter(stream);
                writer.WriteLine(message);  // Отправляем команду
                writer.Flush();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка подключения к Unity: " + ex.Message);
            }
        }
        private void SelectCameraButton_Click(object sender, RoutedEventArgs e)
        {
                // Сделать кнопки видимыми
                Camera1Button.Visibility = Visibility.Visible;
                Camera2Button.Visibility = Visibility.Visible;
        }
        private void ImageButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Настройки открыты");
        }

        private void NoDevicesButton_Click(object sender, RoutedEventArgs e)
        {
            PopulateCameraDevices();
            SlidePanel.Visibility = SlidePanel.Visibility == Visibility.Visible 
                                    ? Visibility.Collapsed 
                                    : Visibility.Visible;
        }

        private void PopulateCameraDevices()
        {
            SlidePanelContent.Children.Clear();

            var devices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            if (devices.Length == 0)
            {
                SlidePanelContent.Children.Add(new TextBlock
                {
                    Text = "Нет доступных устройств",
                    Foreground = Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                });
            }
            else
            {
                foreach (var device in devices)
                {
                    var button = new Button
                    {
                        Content = device.Name,
                        Margin = new Thickness(5),
                        Background = Brushes.White,
                        Foreground = Brushes.Black,
                        FontWeight = FontWeights.Bold
                    };
                    button.Click += (s, e) => SelectCamera(device);
                    SlidePanelContent.Children.Add(button);
                }
            }
        }
        
            // Вывод названия камеры
        private void SelectCamera(DsDevice device)
        {
            SlidePanel.Visibility = Visibility.Collapsed;
            NoDevicesButton.Content = device.Name;
            
        // Передаем устройство в ChildControl2
        // if (ChildControlHost.Content is ChildControl2 childControl)
        // {
        //     childControl.StartCameraStream(device);
        // }

            MessageBox.Show($"Выбрана камера: {device.Name}");
        }
    }
}
