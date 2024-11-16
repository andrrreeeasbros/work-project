using System;
using System.IO;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DirectShowLib;
using System.Collections.Generic;
using OpenCvSharpWindow = OpenCvSharp.Window;
using System.Windows.Media.Animation;

namespace WinMain
{
    public partial class MainWindow : Window
    {


        private const string ABOUT_LINK = "https://github.com/andrrreeeasbros/work-project";
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

        private bool _isAnimating = false; // Новый флаг для блокировки повторных действий

        private void ToggleConsoleButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isAnimating) return; // Если анимация уже выполняется, выходим

            _isAnimating = true; // Устанавливаем флаг анимации

            if (_isConsoleVisible)
            {
                // Если консоль видима, скрываем её
                var hideAnimation = (Storyboard)FindResource("HideConsoleTextBoxAnimation");
                hideAnimation.Completed += (s, _) =>
                {
                    ConsoleTextBox.Visibility = Visibility.Collapsed; // Скрываем консоль после завершения анимации
                    _isConsoleVisible = false; // Обновляем состояние
                    _isAnimating = false; // Сбрасываем флаг
                };
                hideAnimation.Begin(ConsoleTextBox);

                // Меняем стрелку на кнопке
                UpdateArrowIcon("M 0,5 L 5,0 L 10,5 Z"); // Стрелка вверх
            }
            else
            {
                // Если консоль скрыта, показываем её
                ConsoleTextBox.Visibility = Visibility.Visible; // Делаем консоль видимой перед анимацией
                var showAnimation = (Storyboard)FindResource("ShowConsoleTextBoxAnimation");
                showAnimation.Completed += (s, _) =>
                {
                    _isConsoleVisible = true; // Обновляем состояние
                    _isAnimating = false; // Сбрасываем флаг
                };
                showAnimation.Begin(ConsoleTextBox);

                // Меняем стрелку на кнопке
                UpdateArrowIcon("M 0,0 L 5,5 L 10,0 Z"); // Стрелка вниз
            }
        }

        // Обновление иконки стрелки
        private void UpdateArrowIcon(string data)
        {
            var arrow = ToggleConsoleButton.Template.FindName("ArrowIcon", ToggleConsoleButton) as System.Windows.Shapes.Path;
            if (arrow != null)
            {
                arrow.Data = Geometry.Parse(data);
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

        // Обработчик для кнопки SecondIconButton
        private void SecondIconButton_Click(object sender, RoutedEventArgs e)
        {
            // Создаём пользовательское окно
            var window = new Window
            {
                Title = "Справочник",
                Width = 300,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Background = Brushes.White,
                ResizeMode = ResizeMode.NoResize
            };

            // Основной StackPanel для содержимого
            var panel = new StackPanel
            {
                Margin = new Thickness(10)
            };

            // Текст с горячими клавишами
            var textBlock = new TextBlock
            {
                Text = "Горячие клавиши:\n" +
                    "Shift - Консоль\n" +
                    "Ctrl - Выбор устройства\n" +
                    "CapsLock - Выбор камеры",
                TextWrapping = TextWrapping.Wrap,
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 20)
            };

            // Горизонтальная панель для кнопок
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // Кнопка "О проекте"
            var aboutButton = new Button
            {
                Content = "О проекте",
                Margin = new Thickness(0, 0, 10, 0),
                Padding = new Thickness(10)
            };

            // Кнопка "Ok"
            var okButton = new Button
            {
                Content = "Ok",
                Padding = new Thickness(10)
            };

            // Обработчик для кнопки "О проекте"
        // Обработчик для кнопки "О проекте"
        aboutButton.Click += (s, args) =>
        {
            try
            {
                // Открыть ссылку в веб-браузере
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = ABOUT_LINK,
                    UseShellExecute = true // Обеспечивает открытие ссылки в системе по умолчанию
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось открыть ссылку: " + ex.Message,
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        };

            // Обработчик для кнопки "Ok"
            okButton.Click += (s, args) => window.Close();

            // Добавляем кнопки в горизонтальную панель
            buttonPanel.Children.Add(aboutButton);
            buttonPanel.Children.Add(okButton);

            // Добавляем элементы в основную панель
            panel.Children.Add(textBlock);
            panel.Children.Add(buttonPanel);

            // Устанавливаем панель как содержимое окна
            window.Content = panel;

            // Показываем окно
            window.ShowDialog();
        }


        // Обработчик для кнопки FirstImageButton
        private void FirstImageButton_Click(object sender, RoutedEventArgs e)
        {
            // Ваш код для обработки клика на FirstImageButton
            MessageBox.Show("FirstImageButton clicked!");
        }


        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            base.OnKeyDown(e);

            // Проверка, что клавиша не обрабатывается элементами ввода
            var focusedElement = System.Windows.Input.Keyboard.FocusedElement;
            if (focusedElement is TextBox || focusedElement is PasswordBox || focusedElement is RichTextBox)
            {
                // Не обрабатывать горячие клавиши, если фокус находится на элементе ввода текста
                return;
            }

            // Обработка горячих клавиш
            switch (e.Key)
            {
                case System.Windows.Input.Key.LeftShift:
                    ToggleConsoleButton_Click(null, null); // Переключение консоли
                    break;

                case System.Windows.Input.Key.LeftCtrl:
                    NoDevicesButton_Click(null, null); // Отображение списка устройства
                    break;

                case System.Windows.Input.Key.CapsLock:
                    SelectCameraButton_Click(null, null); // Выбор камер
                    break;

                default:
                    break;
            }
        }
    }
}
