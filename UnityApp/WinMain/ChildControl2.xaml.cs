using System;
using System.Windows;
using System.Windows.Controls;
using OpenCvSharp;

namespace WinMain
{
    public partial class ChildControl2 : UserControl
    {
        public ChildControl2()
        {
            InitializeComponent();
            this.Loaded += ChildControl2_Loaded;
        }

        private void ChildControl2_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Начинаем воспроизведение видео после загрузки контрола
                if (BackgroundVideo != null)
                {
                    BackgroundVideo.Play();
                }
                else
                {
                    MessageBox.Show("Ошибка: Видео не найдено.");
                }
            }
            catch (Exception ex)
            {
                // Логируем ошибку
                MessageBox.Show("Ошибка при воспроизведении видео: " + ex.Message);
            }
        }

        private void BackgroundVideo_MediaEnded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Перезапуск видео при завершении воспроизведения
                if (BackgroundVideo != null)
                {
                    BackgroundVideo.Position = TimeSpan.Zero; // Устанавливаем начало видео
                    BackgroundVideo.Play(); // Запускаем видео заново
                }
                else
                {
                    MessageBox.Show("Ошибка: Видео не найдено.");
                }
            }
            catch (Exception ex)
            {
                // Логируем ошибку
                MessageBox.Show("Ошибка при повторном воспроизведении видео: " + ex.Message);
            }
        }
    }
}
