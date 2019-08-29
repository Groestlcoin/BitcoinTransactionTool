using System.Windows;
using System.Windows.Input;

namespace BitcoinTransactionTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            try {
                Window.GetWindow(this)?.DragMove();
            }
            catch {
                //Do Nothing
            }
        }
    }
}
