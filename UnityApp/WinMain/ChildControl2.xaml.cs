using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using OpenCvSharp;
using WinMain;
using System.Threading.Tasks;

namespace WinMain
{
    public partial class ChildControl2 : UserControl
    {
        private VideoCapture _capture;
        private Mat _frame;
        private bool _isStreaming = false;
        private int cameraIndex;
        private CascadeClassifier _faceCascade;
        private MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;

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
                MessageBox.Show("Ошибка при воспроизведении видео: " + ex.Message);
            }
        }

        private void BackgroundVideo_MediaEnded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (BackgroundVideo != null)
                {
                    BackgroundVideo.Position = TimeSpan.Zero;
                    BackgroundVideo.Play();
                }
                else
                {
                    MessageBox.Show("Ошибка: Видео не найдено.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при повторном воспроизведении видео: " + ex.Message);
            }
        }

        public async void StartCameraStream(int cameraIndexLocal)
        {
            cameraIndex = cameraIndexLocal;

            try
            {
                if (cameraIndex == -1)
                {
                    MessageBox.Show("Камера не найдена.");
                    BackgroundVideo.Visibility = Visibility.Visible;
                    return;
                }

                _capture = new VideoCapture(cameraIndex);
                if (!_capture.IsOpened())
                {
                    BackgroundVideo.Visibility = Visibility.Visible;
                    return;
                }

                _frame = new Mat();
                _isStreaming = true;
                BackgroundVideo.Visibility = Visibility.Collapsed;

                // Запуск асинхронного потока для захвата кадров
                await Task.Run(() => CaptureFrameAsync());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при запуске камеры: " + ex.Message);
                BackgroundVideo.Visibility = Visibility.Visible;
            }
        }

        private async Task CaptureFrameAsync()
        {
            try
            {
                while (_isStreaming)
                {
                    if (_capture.Read(_frame))
                    {
                        if (!_frame.Empty())
                        {
                            DetectFaceAndDrawRectangle();
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                BackgroundImage.Source = ConvertMatToBitmapSource(_frame);
                            });
                        }
                    }

                    // Добавлена задержка для ограничения частоты кадров
                    await Task.Delay(33); // Переход к следующему кадру (примерно 30 FPS)
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при захвате видео: " + ex.Message);
                _isStreaming = false;
                BackgroundVideo.Visibility = Visibility.Visible;
            }
        }

        private void DetectFaceAndDrawRectangle()
        {
            try
            {
                var grayFrame = new Mat();
                Cv2.CvtColor(_frame, grayFrame, ColorConversionCodes.BGR2GRAY);

                var faces = _faceCascade.DetectMultiScale(grayFrame, 1.1, 4, HaarDetectionTypes.ScaleImage, new OpenCvSharp.Size(30, 30));

                foreach (var face in faces)
                {
                    Cv2.Rectangle(_frame, face, new Scalar(0, 0, 255), 2);

                    var xPos = face.X;
                    var yPos = face.Y;
                    var width = face.Width;
                    var height = face.Height;

                    mainWindow.PrintLogInConsole($"Face detected at X: {xPos}, Y: {yPos}, Width: {width}, Height: {height}");
                }

                // Очистка ресурсов после обработки
                grayFrame?.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при детекции лиц: {ex.Message}");
            }
        }

        public void StopCameraStream()
        {
            _isStreaming = false;
            _capture?.Release(); // Освобождение ресурсов камеры
            _frame?.Dispose(); // Освобождение памяти
        }

        private BitmapSource ConvertMatToBitmapSource(Mat mat)
        {
            if (mat.Empty())
                throw new ArgumentException("Мат пустой");

            var bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mat);
            var bitmapImage = new BitmapImage();
            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
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
