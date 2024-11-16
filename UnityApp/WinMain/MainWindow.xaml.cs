using System;
using System.IO;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DirectShowLib;
using System.Collections.Generic;
using OpenCvSharpWindow = OpenCvSharp.Window; // Псевдоним для OpenCvSharp.Window
using System.Windows.Media.Animation;

namespace WinMain
{
    public partial class MainWindow : Window
    {
        private bool _isConsoleVisible = false;

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
                using var writer = new StreamWriter(stream);
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
            // Если кнопки уже видимы, скрываем их
            if (Camera1Button.Visibility == Visibility.Visible && Camera2Button.Visibility == Visibility.Visible)
            {
                // Запуск анимации исчезновения кнопок
                var hideButtonsAnimation = (Storyboard)FindResource("HideButtonsAnimation");
                hideButtonsAnimation.Begin();
            }
            else
            {
                // Сделать кнопки видимыми
                Camera1Button.Visibility = Visibility.Visible;
                Camera2Button.Visibility = Visibility.Visible;

                // Запуск анимации появления кнопок
                var showButtonsAnimation = (Storyboard)FindResource("ShowButtonsAnimation");
                showButtonsAnimation.Begin();
            }
        }

        private void ImageButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Настройки открыты");
        }

        private void NoDevicesButton_Click(object sender, RoutedEventArgs e)
        {
            PopulateCameraDevices();

            // Если панель видима, скрываем её с анимацией
            if (SlidePanel.Visibility == Visibility.Visible)
            {
                // Подписываемся на завершение анимации скрытия панели
                var hideSlidePanelAnimation = (Storyboard)FindResource("HideSlidePanelAnimation");
                hideSlidePanelAnimation.Completed += HideSlidePanelAnimation_Completed;  // Подписка на завершение
                hideSlidePanelAnimation.Begin();
            }
            else
            {
                // Сделать панель видимой и запустить анимацию её появления
                SlidePanel.Visibility = Visibility.Visible;  // Устанавливаем видимость перед анимацией
                var showSlidePanelAnimation = (Storyboard)FindResource("ShowSlidePanelAnimation");
                showSlidePanelAnimation.Begin();
            }
        }

        private void HideSlidePanelAnimation_Completed(object sender, EventArgs e)
        {
            // Скрыть панель после завершения анимации
            SlidePanel.Visibility = Visibility.Collapsed;
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

        private void ToggleConsoleButton_Click(object sender, RoutedEventArgs e)
        {
            _isConsoleVisible = !_isConsoleVisible; // Переключаем состояние консоли

            // Меняем стрелку на кнопке
            var arrow = ToggleConsoleButton.Template.FindName("ArrowIcon", ToggleConsoleButton) as System.Windows.Shapes.Path;
            if (arrow != null)
            {
                arrow.Data = Geometry.Parse(_isConsoleVisible
                    ? "M 0,0 L 5,5 L 10,0 Z" // Стрелка вниз
                    : "M 0,5 L 5,0 L 10,5 Z"); // Стрелка вверх
            }

            // Анимация появления или исчезновения консоли
            if (_isConsoleVisible)
            {
                ConsoleTextBox.Visibility = Visibility.Visible; // Делаем консоль видимой перед анимацией
                var showAnimation = (Storyboard)FindResource("ShowConsoleTextBoxAnimation");
                showAnimation.Begin(ConsoleTextBox);
            }
            else
            {
                var hideAnimation = (Storyboard)FindResource("HideConsoleTextBoxAnimation");
                hideAnimation.Completed += (s, _) =>
                {
                    ConsoleTextBox.Visibility = Visibility.Collapsed; // Скрываем консоль после завершения анимации
                };
                hideAnimation.Begin(ConsoleTextBox);
            }
        }

        private void SelectCamera(DsDevice device)
        {
            NoDevicesButton.Content = device.Name;
            SlidePanel.Visibility = Visibility.Collapsed;

            int cameraIndex = GetCameraIndex(device.Name);

            // Проверяем, что ContentControl уже содержит нужный элемент
            if (ChildControlHost.Content == null || !(ChildControlHost.Content is ChildControl2))
            {
                // Создаем новый экземпляр ChildControl2
                ChildControlHost.Content = new ChildControl2();
            }

            // Если в ContentHost находится ChildControl2, вызываем метод для потока камеры
            if (ChildControlHost.Content is ChildControl2 childControl)
            {
                childControl.StartCameraStream(cameraIndex);
            }
            else
            {
                MessageBox.Show("Ошибка: не удалось найти нужный элемент.");
            }
        }

        private int GetCameraIndex(string deviceName)
        {
            var devices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            for (int i = 0; i < devices.Length; i++)
            {
                if (devices[i].Name == deviceName)
                {
                    return i;  // Возвращаем индекс камеры
                }
            }
            return -1;  // Если камера не найдена, возвращаем -1
        }
    }
}
