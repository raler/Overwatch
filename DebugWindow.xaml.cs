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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Overwatch
{
    /// <summary>
    /// Interaction logic for DebugWindow.xaml
    /// </summary>
    public partial class DebugWindow : Window
    {
        public DebugWindow()
        {
            InitializeComponent();
            this.Activated += DebugWindow_Activated;
        }

        void DebugWindow_Activated(object sender, EventArgs e)
        {
            App.Window.Topmost = true;
            App.Window.Topmost = false;
            this.Focus();
            this.Topmost = true;
            this.Topmost = false;
        }

        private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!App.Window.IsClosing)
            {
                this.Hide();
                e.Cancel = true;
                App.Window.Focus();
            }
        }

        public void ScrollToBottom()
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                debugCons.consoleOutput.ScrollToEnd();
            }));
        }
    }
}
