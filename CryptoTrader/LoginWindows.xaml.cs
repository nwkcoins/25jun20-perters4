using MahApps.Metro.Controls;
using System.Windows;

namespace CryptoTrader
{
    /// <summary>
    /// Interaction logic for LoginWindows.xaml
    /// </summary>
    public partial class LoginWindows : MetroWindow
    {
        public LoginWindows()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            if (Username.Text == "TradingCrypto" && Password.Text == "1234567")
            {
                MainWindow a = new MainWindow();
                this.Close();
                a.Show();
            }
        }

        private void EXIT_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
