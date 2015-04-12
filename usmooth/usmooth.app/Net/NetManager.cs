using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ucore;
using usmooth.common;

namespace usmooth.app
{
    public class NetManager : IDisposable
    {
        public static NetManager Instance;

        public bool IsConnected { get { return _client.IsConnected; } }

        public event SysPost.StdMulticastDelegation LogicallyConnected;
        public event SysPost.StdMulticastDelegation LogicallyDisconnected;

        public NetManager()
        {
            _client.Connected += OnConnected;
            _client.Disconnected += OnDisconnected;
            _client.LogEmitted += OnLogEmitted;

            _client.RegisterCmdHandler(eNetCmd.SV_HandshakeResponse, Handle_HandshakeResponse);
            _client.RegisterCmdHandler(eNetCmd.SV_KeepAliveResponse, Handle_KeepAliveResponse);
            _client.RegisterCmdHandler(eNetCmd.SV_ExecCommandResponse, Handle_ExecCommandResponse);

            _guardTimer.Timeout += OnGuardingTimeout;
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        public bool Connect(string addr)
        {
            ushort port = (ushort)EzConv.ToInt(Properties.Settings.Default.ServerPort);
            if (port == 0)
            {
                UsLogging.Printf(LogWndOpt.Bold, "port '{0}' invalid, connection aborted.", port);
                return false;
            }

            _client.Connect(addr, port);
            return true;
        }

        public void Disconnect()
        {
            _client.Disconnect();
        }

        public void Send(UsCmd cmd)
        {
            _client.SendPacket(cmd);
        }

        private void OnConnected(object sender, EventArgs e)
        {
            UsCmd cmd = new UsCmd();
            cmd.WriteInt16((short)eNetCmd.CL_Handshake);
            cmd.WriteInt16(Properties.Settings.Default.VersionMajor);
            cmd.WriteInt16(Properties.Settings.Default.VersionMinor);
            cmd.WriteInt16(Properties.Settings.Default.VersionPatch);
            _client.SendPacket(cmd);

            _guardTimer.Activate();
        }

        private void OnDisconnected(object sender, EventArgs e)
        {
            _guardTimer.Deactivate();
            SysPost.InvokeMulticast(this, LogicallyDisconnected);
        }

        private void OnLogEmitted(object sender, EventArgs e)
        {
            LogEventArgs lea = e as LogEventArgs;
            if (lea != null)
            {
                UsLogging.Printf(LogWndOpt.Info, lea.Text);
            }
        }
          
        private void OnGuardingTimeout(object sender, EventArgs e)
        {
            UsLogging.Printf(LogWndOpt.Error, "guarding timeout, closing connection...");
            Disconnect();
        }

        private bool Handle_HandshakeResponse(eNetCmd cmd, UsCmd c)
        {
            UsLogging.Printf("eNetCmd.SV_HandshakeResponse received, connection validated.");

            SysPost.InvokeMulticast(this, LogicallyConnected);

            _guardTimer.Deactivate();
            return true;
        }

        private bool Handle_KeepAliveResponse(eNetCmd cmd, UsCmd c)
        {

            return true;
        }

        private bool Handle_ExecCommandResponse(eNetCmd cmd, UsCmd c)
        {
            int retVal = c.ReadInt32();
            UsLogging.Printf(string.Format("command executing result: [b]{0}[/b]", retVal));

            return true;
        }

        private NetClient _client = new NetClient();
        private NetGuardTimer _guardTimer = new NetGuardTimer();
    }
}
