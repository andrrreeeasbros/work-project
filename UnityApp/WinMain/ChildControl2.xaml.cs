using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using OpenCvSharp;
using WinMain;

namespace WinMain
{
    public partial class ChildControl2 : UserControl
    {
        private VideoCapture _capture;
        private Mat _frame;
        private bool _isStreaming = false;
        private int cameraIndex;
        private CascadeClassifier _faceCascade;
        MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;

        public ChildControl2()
        {
            InitializeComponent();
            this.Loaded += ChildControl2_Loaded;

            // Загружаем классификатор для поиска лиц
            _faceCascade = new CascadeClassifier("res/haarcascade_frontalface_default.xml");
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
                BackgroundVideo.Visibility = Visibility.Visible; // Показываем BackgroundVideo при ошибке
            }
        }

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
                            DetectFaceAndDrawRectangle(); // Метод для обнаружения лица
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

        // Обнаружение лица и отрисовка квадрата
        private void DetectFaceAndDrawRectangle()
        {
            // Преобразуем кадр в черно-белое изображение для классификатора
            var grayFrame = new Mat();
            Cv2.CvtColor(_frame, grayFrame, ColorConversionCodes.BGR2GRAY);

            // Ищем лица
            var faces = _faceCascade.DetectMultiScale(grayFrame, 1.1, 4, HaarDetectionTypes.ScaleImage, new OpenCvSharp.Size(30, 30));

            foreach (var face in faces)
            {
                // Отображаем прямоугольник вокруг лица
                Cv2.Rectangle(_frame, face, new Scalar(0, 0, 255), 2);

                // Возвращаем координаты и размеры (X_pos, Y_pos, X_size, Y_size)
                var xPos = face.X;
                var yPos = face.Y;
                var width = face.Width;
                var height = face.Height;

                // Для примера выводим их в консоль
                mainWindow.PrintLogInConsole($"Face detected at X: {xPos}, Y: {yPos}, Width: {width}, Height: {height}");
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
