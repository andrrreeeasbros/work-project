using System.Windows;

namespace WinMain
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // Обработчик для кнопки с изображением
        private void ImageButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Кнопка с изображением нажата!");
        }
    }
}
