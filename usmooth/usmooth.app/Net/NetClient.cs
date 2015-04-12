using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ucore;
using usmooth.common;

namespace usmooth.app
{
    public class LogEventArgs : EventArgs
    {
        public LogEventArgs(string text) 
        {
            Text = text;
        }

        public string Text { get; set; }
    }

    public class NetClient : IDisposable
    {
        string _host;
        ushort _port;

        private TcpClient _tcpClient;

        private UsCmdParsing m_cmdParser = new UsCmdParsing();

        public event SysPost.StdMulticastDelegation Connected;
        public event SysPost.StdMulticastDelegation Disconnected;
        public event SysPost.StdMulticastDelegation LogEmitted;

        public NetClient()
        {
        }

        public bool IsConnected { get { return _tcpClient != null; } }

        public void Connect(string host, ushort port)
        {
            _host = host;
            _port = port;
            _tcpClient = new TcpClient();
            _tcpClient.BeginConnect(_host, _port, OnConnect, _tcpClient);
            UsLogging.Printf(LogWndOpt.Bold, "connecting to [u]{0}:{1}[/u]...", host, port);
        }

        public void Disconnect()
        {
            if (_tcpClient != null)
            {
                _tcpClient.Close();
                _tcpClient = null;

                UsLogging.Printf("connection closed.");
                SysPost.InvokeMulticast(this, Disconnected);
            }
        }

        public void RegisterCmdHandler(eNetCmd cmd, EtCmdHandler handler)
        {
            m_cmdParser.RegisterHandler(cmd, handler);
        }

        public void Tick_CheckConnectionStatus()
        {
            try
            {
                if (!_tcpClient.Connected)
                {
                    throw new Exception();
                }

                // check if the remote client is still connected
                if (_tcpClient.Client.Poll(0, SelectMode.SelectRead))
                {
                    byte[] checkConn = new byte[1];
                    if (_tcpClient.Client.Receive(checkConn, SocketFlags.Peek) == 0)
                    {
                        throw new IOException();
                    }
                }
            }
            catch (Exception)
            {
                Disconnect();
            }
        }

        public void Tick_ReceivingData()
        {
            try
            {
                if (_tcpClient.Available > 0)
                {
                    byte[] buffer = new byte[8192];
                    int len = _tcpClient.GetStream().Read(buffer, 0, buffer.Length);

                    UsCmd cmd = new UsCmd(buffer);
                    UsCmdExecResult result = m_cmdParser.Execute(cmd);
                    switch (result)
                    {
                        case UsCmdExecResult.Succ:
                            break;
                        case UsCmdExecResult.Failed:
                            AddToLog(string.Format("server cmd execution failed: {0}.", new UsCmd(buffer).ReadNetCmd()));
                            break;
                        case UsCmdExecResult.HandlerNotFound:
                            AddToLog(string.Format("unknown server msg: {0}.", Encoding.UTF8.GetString(buffer, 0, len)));
                            break;
                    }
                }
            }
            catch (Exception)
            {
                Disconnect();
            }
        }

        public void Dispose()
        {
            Disconnect();
        }

        public void SendCommand(string cmd)
        {
            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(cmd);
                _tcpClient.GetStream().Write(buffer, 0, buffer.Length);
            }
            catch (Exception)
            {
                Disconnect();
            }
        }

        public void SendPacket(UsCmd cmd)
        {
            try
            {
                byte[] buffer = cmd.Buffer;
                _tcpClient.GetStream().Write(buffer, 0, buffer.Length);
            }
            catch (Exception)
            {
                Disconnect();
            }
        }

        // Called when a connection to a server is established
        private void OnConnect(IAsyncResult asyncResult)
        {
            // Retrieving TcpClient from IAsyncResult
            TcpClient tcpClient = (TcpClient)asyncResult.AsyncState;

            try
            {
                if (tcpClient.Connected) // may throw NullReference
                {
                    AddToLog(string.Format("Connected successfully."));
                    SysPost.InvokeMulticast(this, Connected);
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                AddToLog(string.Format("Connection failed."));
                Disconnect();
                return;
            }
        }

        // Adds a formatted entry to the log
        private void AddToLog(string text)
        {
            SysPost.InvokeMulticast(this, LogEmitted, new LogEventArgs(text));
        }
    }
}
