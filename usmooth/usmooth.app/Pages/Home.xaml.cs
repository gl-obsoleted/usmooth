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
using usmooth.common;

namespace usmooth.app.Pages
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : UserControl
    {
        private QotdClient _client;

        private UsCmdParsing m_cmdParser = new UsCmdParsing();

        public Home()
        {
            InitializeComponent();

            if (!cb_targetIP.Items.IsEmpty)
            {
                cb_targetIP.SelectedItem = cb_targetIP.Items[0];
            }

            bb_logging.BBCode = string.Empty;
            AddToLog("usmooth is initialized successfully.");

            m_cmdParser.RegisterHandler(eNetCmd.SV_HandshakeResponse, Handle_HandshakeResponse);
            m_cmdParser.RegisterHandler(eNetCmd.SV_KeepAliveResponse, Handle_KeepAliveResponse);
            m_cmdParser.RegisterHandler(eNetCmd.SV_ExecCommandResponse, Handle_ExecCommandResponse);
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
            m_loggingPanel.Dispatcher.Invoke(new Action(() =>
            {
                string content = string.Format("{0} {1}\r\n", DateTime.Now.ToLongTimeString(), text);
                bb_logging.BBCode += content;
                m_loggingPanel.ScrollToBottom();
            }));
        }

        private void Handle_HandshakeResponse(short cmd, UsCmd c)
        {

        }
        private void Handle_KeepAliveResponse(short cmd, UsCmd c)
        {

        }
        private void Handle_ExecCommandResponse(short cmd, UsCmd c)
        {

        }

        private void bt_exec_cmd_Click(object sender, RoutedEventArgs e)
        {
            ExecInputCmd();
        }

        private void tb_cmdbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                ExecInputCmd();
            }
        }

        private void ExecInputCmd()
        {
            if (tb_cmdbox.Text.Length == 0)
            {
                AddToLog("the command bar is empty, try 'help' to list all supported commands.");
                return;
            }

            AddToLog(string.Format("command executed: [b]{0}[/b]", tb_cmdbox.Text));

            if (_client != null)
            {
                _client.SendCommand(tb_cmdbox.Text);
            }
            else
            {
                AddToLog("not connected to server, command ignored.");
            }

            tb_cmdbox.Clear();
        }
    }
}
