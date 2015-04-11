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
using Timer = System.Timers.Timer;

namespace usmooth.app
{
    public class QotdClient : IDisposable
    {
        string _host;
        ushort _port;

        private UsCmdParsing m_cmdParser = new UsCmdParsing();

        private TcpClient _tcpClient;
        // A delegate of a log method
        private readonly Action<string> _logCallback;

        private Timer m_tickTimer = new Timer(1000);

        public event SysPost.StdMulticastDelegation Connected;
        public event SysPost.StdMulticastDelegation Disconnected;

        // QOTD client constructor
        public QotdClient(Action<string> logCallback)
        {
            _logCallback = logCallback;
            m_tickTimer.Elapsed += (object sender, global::System.Timers.ElapsedEventArgs e) => Tick();
            m_tickTimer.AutoReset = true;
        }

        public void Connect(string host, ushort port)
        {
            _host = host;
            _port = port;
            _tcpClient = new TcpClient();
            _tcpClient.BeginConnect(_host, _port, OnConnect, _tcpClient);
            AddToLog(string.Format("[b]connect to [u]{0}:{1}[/u]...[/b]", host, port));
        }

        public void Disconnect()
        {
            m_tickTimer.Stop();
            if (_tcpClient != null)
            {
                _tcpClient.Close();
                _tcpClient = null;

                AddToLog(string.Format("Connection closed."));
                SysPost.InvokeMulticast(this, Disconnected);
            }
        }

        public void RegisterCmdHandler(eNetCmd cmd, EtCmdHandler handler)
        {
            m_cmdParser.RegisterHandler(cmd, handler);
        }

        private void Tick()
        {
            try
            {
                // check if the remote client is still connected
                if (_tcpClient.Client.Poll(0, SelectMode.SelectRead))
                {
                    byte[] checkConn = new byte[1];
                    if (_tcpClient.Client.Receive(checkConn, SocketFlags.Peek) == 0)
                    {
                        throw new IOException();
                    }
                }

                if (!_tcpClient.Connected)
                {
                    throw new Exception();
                }

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
                else
                {
                    AddToLog(string.Format("idle {0}.", DateTime.Now.ToLongTimeString()));
                }
            }
            catch (Exception ex)
            {
                Disconnect();
            }
        }

        ~QotdClient()
        {
        }

        // Free the resources
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
                    m_tickTimer.Start();
                    AddToLog(string.Format("Connected successfully."));
                    SysPost.InvokeMulticast(this, Connected);
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
            _logCallback(text);
        }
    }
}
