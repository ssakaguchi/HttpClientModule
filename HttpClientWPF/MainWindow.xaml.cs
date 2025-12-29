using System.Windows;
using System.Windows.Threading;

namespace HttpClientWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            
            LogTextBox.TextChanged += (_, __) =>
            {
                // レイアウト反映後にスクロールさせると安定します
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    LogTextBox.ScrollToEnd();
                }), DispatcherPriority.Background);
            };
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}