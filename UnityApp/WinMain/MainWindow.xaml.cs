
﻿using System;
using System.Windows;
using System.Windows.Media.Animation;

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

         private void Camera1Button_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Камера 1");
        }

        // Обработчик нажатия кнопки Камера 2
        private void Camera2Button_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Камера 2");
        }
}
