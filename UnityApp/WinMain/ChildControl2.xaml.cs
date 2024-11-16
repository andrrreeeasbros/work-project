using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using OpenCvSharp;

namespace WinMain
{
    public partial class ChildControl2 : UserControl
    {
        private VideoCapture _capture;
        private Mat _frame;
        private bool _isStreaming = false;
        private int cameraIndex;

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
        // Метод для старта потока камеры
            public void StartCameraStream(int cameraIndexLocal)
            {
                cameraIndex = cameraIndexLocal;

                try
                {
                    if (cameraIndex == -1)
                    {
                        MessageBox.Show("Камера не найдена.");
                        BackgroundVideo.Visibility = Visibility.Visible; // Показываем BackgroundVideo при ошибке
                        return;
                    }

                    _capture = new VideoCapture(cameraIndex);
                    if (!_capture.IsOpened())
                    {
                        BackgroundVideo.Visibility = Visibility.Visible; // Показываем BackgroundVideo при ошибке
                        return;
                    }

                    _frame = new Mat();
                    _isStreaming = true;

                    BackgroundVideo.Visibility = Visibility.Collapsed; // Скрываем BackgroundVideo, если камера успешно запущена
                    CaptureFrame();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при запуске камеры: " + ex.Message);
                    BackgroundVideo.Visibility = Visibility.Visible;// Показываем BackgroundVideo при ошибке
                }
            }


        // Метод для захвата кадров с камеры
        private void CaptureFrame()
        {
            if (_isStreaming)
            {
                try
                {
                    if (_capture.Read(_frame)) // Захват кадра с камеры
                    {
                        if (!_frame.Empty())
                        {
                            BackgroundImage.Source = ConvertMatToBitmapSource(_frame); // Отображаем кадр на Image
                        }
                        else
                        {
                            throw new Exception("Получен пустой кадр.");
                        }
                    }
                    else
                    {
                        throw new Exception("Ошибка при чтении кадра с камеры.");
                    }

                    // Переходим к следующему кадру через 33 миллисекунды (30 fps)
                    Dispatcher.InvokeAsync(() => CaptureFrame(), System.Windows.Threading.DispatcherPriority.Background);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка во время захвата видео: " + ex.Message);
                    _isStreaming = false;
                    BackgroundVideo.Visibility = Visibility.Visible; // Показываем BackgroundVideo при ошибке
                }
            }
        }

        // Преобразование Mat в BitmapSource
        private BitmapSource ConvertMatToBitmapSource(Mat mat)
        {
            if (mat.Empty())
                throw new ArgumentException("Мат пустой");

            var bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mat);
            var bitmapImage = new BitmapImage();
            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp); // Сохраняем кадр в поток
                stream.Seek(0, SeekOrigin.Begin);
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
            }
            return bitmapImage;
        }
    }
}
