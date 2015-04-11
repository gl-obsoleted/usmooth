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
using System.Configuration;
using ucore;
using usmooth.common;
using Timer = System.Timers.Timer;

namespace usmooth.app.Pages
{
    public enum LogWndOpt
    {
        Info,
        Bold,
        Error,
    }

    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : UserControl
    {
        private QotdClient _client;
        private Timer _handshakeTimer;

        public Home()
        {
            InitializeComponent();

            if (!cb_targetIP.Items.IsEmpty)
            {
                cb_targetIP.SelectedItem = cb_targetIP.Items[0];
            }

            bb_logging.BBCode = string.Empty;
            PrintLogWnd("usmooth is initialized successfully.");

            _client = new QotdClient(PrintLogWnd);

            _client.Connected += this.OnConnected;
            _client.Disconnected += this.OnDisconnected;

            _client.RegisterCmdHandler(eNetCmd.SV_HandshakeResponse, Handle_HandshakeResponse);
            _client.RegisterCmdHandler(eNetCmd.SV_KeepAliveResponse, Handle_KeepAliveResponse);
            _client.RegisterCmdHandler(eNetCmd.SV_ExecCommandResponse, Handle_ExecCommandResponse);
        }

        private void bt_connect_Click(object sender, RoutedEventArgs e)
        {
            if (cb_targetIP.Text.Length == 0)
                return;
            string[] info = cb_targetIP.Text.Split(':');
            if (info.Length != 2)
                return;

            _client.Connect(info[0], (ushort)EzConv.ToInt(info[1]));

            // temporarily disable all connection buttons to prevent double submitting
            bt_connect.IsEnabled = false;
            bt_disconnect.IsEnabled = false;
        }

        private void PrintLogWnd(string text)
        {
            PrintLogWnd(LogWndOpt.Info, text);
        }

        private void PrintLogWnd(LogWndOpt opt, string text)
        {
            m_loggingPanel.Dispatcher.Invoke(new Action(() =>
            {
                string time = string.Format("[color=Gray]{0}[/color]", DateTime.Now.ToLongTimeString());
                string content = "";
                switch (opt)
                {
                    case LogWndOpt.Info:
                        content = string.Format("{0} {1}\r\n", time, text);
                        break;
                    case LogWndOpt.Bold:
                        content = string.Format("{0} [b]{1}[/b]\r\n", time, text);
                        break;
                    case LogWndOpt.Error:
                        content = string.Format("{0} [color=Red]{1}[/color]\r\n", time, text);
                        break;
                    default:
                        break;
                }
                bb_logging.BBCode += content;
                m_loggingPanel.ScrollToBottom();
            }));
        }

        private bool Handle_HandshakeResponse(eNetCmd cmd, UsCmd c)
        {
            PrintLogWnd("eNetCmd.SV_HandshakeResponse received, connection validated.");

            this.Dispatcher.Invoke(new Action(() =>
            {
                bt_connect.IsEnabled = false;
                bt_disconnect.IsEnabled = true;

                _handshakeTimer.Stop();
            }));

            return true;
        }

        private bool Handle_KeepAliveResponse(eNetCmd cmd, UsCmd c)
        {

            return true;
        }

        private bool Handle_ExecCommandResponse(eNetCmd cmd, UsCmd c)
        {

            return true;
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
                PrintLogWnd("the command bar is empty, try 'help' to list all supported commands.");
                return;
            }

            PrintLogWnd(string.Format("command executed: [b]{0}[/b]", tb_cmdbox.Text));

            if (_client != null)
            {
                _client.SendCommand(tb_cmdbox.Text);
            }
            else
            {
                PrintLogWnd("not connected to server, command ignored.");
            }

            tb_cmdbox.Clear();
        }

        private void OnConnected(object sender, EventArgs e)
        {
            UsCmd cmd = new UsCmd();
            cmd.WriteInt16((short)eNetCmd.CL_Handshake);
            cmd.WriteInt16(Properties.Settings.Default.VersionMajor);
            cmd.WriteInt16(Properties.Settings.Default.VersionMinor);
            cmd.WriteInt16(Properties.Settings.Default.VersionPatch);
            _client.SendPacket(cmd);

            _handshakeTimer = new Timer(3000);
            _handshakeTimer.Elapsed += OnHandshakeTimeout;
            _handshakeTimer.Start();
        }

        void OnHandshakeTimeout(object sender, System.Timers.ElapsedEventArgs e)
        {
            PrintLogWnd(LogWndOpt.Error, "handshake timeout, closing connection...");
            DisconnectClient();
        }

        private void OnDisconnected(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() => 
            {
                bt_connect.IsEnabled = true;
                bt_disconnect.IsEnabled = false;
            }));
        }

        private void bt_disconnect_Click(object sender, RoutedEventArgs e)
        {
            PrintLogWnd(LogWndOpt.Bold, "trying to disconnect...");
            DisconnectClient();
        }

        private void DisconnectClient()
        {
            if (_handshakeTimer != null)
            {
                _handshakeTimer.Stop();
                _handshakeTimer = null;
            }

            _client.Disconnect();
        }
    }
}
