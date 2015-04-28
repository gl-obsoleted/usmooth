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
using usmooth.common;

namespace usmooth.app.Pages
{

    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : UserControl
    {
        public Home()
        {
            if (AppNetManager.Instance == null)
                throw new Exception();

            InitializeComponent();

            // refresh recent target IP list
            cb_targetIP.Items.Add(Properties.Settings.Default.LocalAddr);
            if (Properties.Settings.Default.RecentAddrList != null)
            {
                foreach (var item in Properties.Settings.Default.RecentAddrList)
                {
                    cb_targetIP.Items.Add(item);
                }
            }
            cb_targetIP.SelectedItem = cb_targetIP.Items[0];

            bb_logging.BBCode = string.Empty;

            UsLogging.Receivers += Impl_PrintLogToWnd;
            UsLogging.Printf("usmooth is initialized successfully.");

            AppNetManager.Instance.LogicallyConnected += OnLogicallyConnected;
            AppNetManager.Instance.LogicallyDisconnected += OnLogicallyDisconnected;
        }

        private void OnLogicallyConnected(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                bt_connect.IsEnabled = false;
                bt_disconnect.IsEnabled = true;

                string remoteAddr = cb_targetIP.Text;
                UsLogging.Printf(LogWndOpt.Bold, "connected to [u]{0}[/u].", remoteAddr);

                if (AppSettingsUtil.AppendAsRecentlyConnected(remoteAddr))
                {
                    cb_targetIP.Items.Add(remoteAddr);
                    UsLogging.Printf("{0} is appended into the recent connection list.", remoteAddr);
                }
            }));
        }

        private void OnLogicallyDisconnected(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                bt_connect.IsEnabled = true;
                bt_disconnect.IsEnabled = false;
            }));
        }

        private void bt_connect_Click(object sender, RoutedEventArgs e)
        {
            if (AppNetManager.Instance == null)
            {
                UsLogging.Printf(LogWndOpt.Bold, "NetManager not available, connecting failed.");
                return;
            }

            if (!AppNetManager.Instance.Connect(cb_targetIP.Text))
            {
                UsLogging.Printf(LogWndOpt.Bold, "connecting failed.");
                return;
            }

            // temporarily disable all connection buttons to prevent double submitting
            bt_connect.IsEnabled = false;
            bt_disconnect.IsEnabled = false;
        }

        private void Impl_PrintLogToWnd(LogWndOpt opt, string text)
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
            AppNetManager.Instance.ExecuteCmd(tb_cmdbox.Text);
            tb_cmdbox.Clear();
        }

        private void bt_disconnect_Click(object sender, RoutedEventArgs e)
        {
            UsLogging.Printf(LogWndOpt.Error, "[b]disconnecting manually...[/b]");
            AppNetManager.Instance.Disconnect();
        }
    }
}
