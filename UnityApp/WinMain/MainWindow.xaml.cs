using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;  // Import for WPF Path
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.IO;
using System.Net.Sockets;

namespace WinMain
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // Обработчик события для кнопки "Выберите камеру"
        private void SelectCameraButton_Click(object sender, RoutedEventArgs e)
        {
            // Если кнопки видны, скрываем их, иначе показываем
            if (Camera1Button.Visibility == Visibility.Visible)
            {
                // Скрыть кнопки с анимацией
                Storyboard hideAnimation = (Storyboard)this.Resources["HideButtonsAnimation"];
                hideAnimation.Begin();
            }
            else
            {
                // Сделать кнопки видимыми
                Camera1Button.Visibility = Visibility.Visible;
                Camera2Button.Visibility = Visibility.Visible;

                // Запуск анимации для появления кнопок
                Storyboard showAnimation = (Storyboard)this.Resources["ShowButtonsAnimation"];
                showAnimation.Begin();
            }
        }

        // Обработчик завершения анимации для скрытия кнопок
        private void HideButtonsAnimation_Completed(object sender, EventArgs e)
        {
            // После завершения анимации скрываем кнопки
            Camera1Button.Visibility = Visibility.Collapsed;
            Camera2Button.Visibility = Visibility.Collapsed;
        }

        // Обработчик события для кнопки шестерёнки
        private void ImageButton_Click(object sender, RoutedEventArgs e)
        {
            // Ваш код для обработки кнопки шестерёнки
        }

        // Обработчик для кнопки Камера 1
        private void Camera1Button_Click(object sender, RoutedEventArgs e)
        {
            SendMessageToUnity("SwitchCamera:2");
        }

        // Обработчик для кнопки Камера 2
        private void Camera2Button_Click(object sender, RoutedEventArgs e)
        {
            SendMessageToUnity("SwitchCamera:1");
        }

        // Метод для отправки сообщения в Unity через TCP-сокет
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

        // Обработчик для кнопки "No devices"
       private void NoDevicesButton_Click(object sender, RoutedEventArgs e)
{
    // Переключаем видимость панели
    if (SlidePanel.Visibility == Visibility.Collapsed)
    {
        SlidePanel.Visibility = Visibility.Visible;  // Панель появляется
    }
    else
    {
        SlidePanel.Visibility = Visibility.Collapsed;  // Панель скрывается
    }
}
    }
}
