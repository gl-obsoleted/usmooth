using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using usmooth.common;

namespace usmooth.app.Pages
{
    public partial class Home 
    {
        void RegisterNetHandlers()
        {
            NetManager.Instance.LogicallyConnected += OnLogicallyConnected;
            NetManager.Instance.LogicallyDisconnected += OnLogicallyDisconnected;

            NetManager.Instance.RegisterCmdHandler(eNetCmd.SV_QuerySwitchesResponse, NetHandle_QuerySwitchesResponse);
            NetManager.Instance.RegisterCmdHandler(eNetCmd.SV_QuerySlidersResponse, NetHandle_QuerySlidersResponse);
    
            NetManager.Instance.RegisterCmdHandler(eNetCmd.SV_StartAnalysePixels, NetHandle_StartAnalysePixels);
        }

        private void OnLogicallyConnected(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                cb_targetIP.IsEnabled = false;
                bt_connect.IsEnabled = false;
                bt_disconnect.IsEnabled = true;

                string remoteAddr = cb_targetIP.Text;
                UsLogging.Printf(LogWndOpt.Bold, "connected to [u]{0}[/u].", remoteAddr);

                if (AppSettingsUtil.AppendAsRecentlyConnected(remoteAddr))
                {
                    cb_targetIP.Items.Add(remoteAddr);
                    UsLogging.Printf("{0} is appended into the recent connection list.", remoteAddr);
                }

                // query switches and sliders
                {
                    UsCmd cmd = new UsCmd();
                    cmd.WriteNetCmd(eNetCmd.CL_QuerySwitches);
                    NetManager.Instance.Send(cmd);
                }
                {
                    UsCmd cmd = new UsCmd();
                    cmd.WriteNetCmd(eNetCmd.CL_QuerySliders);
                    NetManager.Instance.Send(cmd);
                }

            }));
        }

        private void OnLogicallyDisconnected(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                cb_targetIP.IsEnabled = true;
                bt_connect.IsEnabled = true;
                bt_disconnect.IsEnabled = false;

                _switchersPanel.Children.Clear();
                _slidersPanel.Children.Clear();
            }));
        }

        bool NetHandle_QuerySwitchesResponse(eNetCmd cmd, UsCmd c)
        {
            int count = c.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                string name = c.ReadString();
                string path = c.ReadString();
                short initVal = c.ReadInt16();

                _switchersPanel.Dispatcher.Invoke(new Action(() =>
                {
                    AddSwitcher(name, path, initVal != 0);
                }));
            }

            return true;
        }

        bool NetHandle_QuerySlidersResponse(eNetCmd cmd, UsCmd c)
        {
            int count = c.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                string name = c.ReadString();
                float minVal = c.ReadFloat();
                float maxVal = c.ReadFloat();
                float initVal = c.ReadFloat();

                _slidersPanel.Dispatcher.Invoke(new Action(() =>
                {
                    AddSlider(name, minVal, maxVal, initVal);
                }));
            }

            return true;
        }

        bool NetHandle_StartAnalysePixels(eNetCmd cmd, UsCmd c)
        {
            int count = c.ReadInt32();
            for (int i = 0; i < count; ++i )
            {
                string msg = c.ReadString();
                UsLogging.Printf(msg);
            }
            return true;
        }
    }
}
