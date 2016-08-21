using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for DebugConsole.xaml
    /// </summary>
    public partial class DebugConsole : UserControl
    {
        public DebugConsole()
        {
            InitializeComponent();
            var writer = new TextBoxStreamWriter(consoleOutput);
            Console.SetOut(writer);
        }

    }


    public class TextBoxStreamWriter : TextWriter
    {
        TextBox _output = null;

        public TextBoxStreamWriter(TextBox output)
        {
            _output = output;
        }

        public override void WriteLine(string value)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                _output.AppendText("[" + DateTime.Now.ToLongTimeString() + "]");
            }));
            base.WriteLine(value);
        }

        public override void Write(char value)
        {
            base.Write(value);
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                bool scrollToBottom = false;
                if (value == '\n')
                {
                    var vertOffset = _output.VerticalOffset;
                    double dViewport = _output.ViewportHeight;
                    double dExtent = _output.ExtentHeight;

                    if (dExtent > dViewport && dExtent - dViewport - vertOffset < 20)
                        scrollToBottom = true;
                }

                _output.AppendText(value.ToString()); // When character data is written, append it to the text box.

                if (scrollToBottom)
                    _output.ScrollToEnd();
            }));
        }

        public override Encoding Encoding
        {
            get { return System.Text.Encoding.UTF8; }
        }
    }
}
