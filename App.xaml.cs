using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Overwatch
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static LeagueChat Chat;
        public static MainWindow Window;
        public static DebugWindow DebugWindow;
        public App()
        {
            DebugWindow = new DebugWindow();

            Chat = new LeagueChat();
            Chat.Initialize();

            Window = new MainWindow();
            Window.Show();
        }
    }
}
