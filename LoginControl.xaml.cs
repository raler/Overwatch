using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Overwatch
{
    /// <summary>
    /// Interaction logic for LoginControl.xaml
    /// </summary>
    public partial class LoginControl : UserControl
    {
        public LoginControl()
        {
            InitializeComponent();
            App.Chat.OnCurrentVersionReceived += Chat_OnCurrentVersionReceived;
        }

        void Chat_OnCurrentVersionReceived(object sender)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                versionOverlay.Visibility = System.Windows.Visibility.Hidden;
            }));
        }

        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            Login();
        }

        private void usernameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            checkBoxes();
        }

        private void passwordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            checkBoxes();
        }

        private void checkBoxes()
        {
            if (usernameBox.Text != "" && passwordBox.Password != "")
                loginButton.IsEnabled = true;
            else
                loginButton.IsEnabled = false;
        }

        private void UserControl_Loaded_1(object sender, RoutedEventArgs e)
        {
            usernameBox.Focus();
        }

        private void inputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Login();
        }

        private void Login()
        {
            if (loginButton.IsEnabled == true && versionOverlay.Visibility != System.Windows.Visibility.Visible)
            {
                this.IsEnabled = false;
                var password = passwordBox.Password;
                passwordBox.Password = "";
                App.Window.Cursor = Cursors.Wait;
                App.Chat.Start(usernameBox.Text, password);
            }
        }
    }
}
