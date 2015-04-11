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
using ucore;

namespace usmooth.app.Pages
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : UserControl
    {
        private QotdClient _client;

        public Home()
        {
            InitializeComponent();

            for (int i = 0; i < 100; i++)
            {
                bb_logging.BBCode += "client is initialized successfully.\r\n";
            }
            m_loggingPanel.ScrollToBottom();
        }

        private void bt_connect_Click(object sender, RoutedEventArgs e)
        {
            if (cb_targetIP.Text.Length == 0)
                return;
            string[] info = cb_targetIP.Text.Split(':');
            if (info.Length != 2)
                return;

            _client = new QotdClient(info[0], (ushort)EzConv.ToInt(info[1]), AddToLog);
        }
        private void AddToLog(string text)
        {
            Console.WriteLine(text);
        }
    }
}
