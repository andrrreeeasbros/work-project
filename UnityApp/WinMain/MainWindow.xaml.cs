using System;
using System.IO;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DirectShowLib;
using Emgu.CV;
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


        // Вывод названия камеры
        private void SelectCamera(DsDevice device)
        {
            SlidePanel.Visibility = Visibility.Collapsed;
            NoDevicesButton.Content = device.Name;
            
            MessageBox.Show($"Выбрана камера: {device.Name}");
        }
    }
}
