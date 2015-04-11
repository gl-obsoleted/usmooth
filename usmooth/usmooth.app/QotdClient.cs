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
using Timer = System.Timers.Timer;

namespace usmooth.app
{
    class ConnectionStateChangedEventArgs : EventArgs
    {
        public ConnectionStateChangedEventArgs(bool connected)
        {
            Connected = connected;
        }

        public bool Connected { get; set; }
    }

    public delegate void ConnectionStateChangedHandler(bool connected);

    public class QotdClient : IDisposable
    {
        private TcpClient _tcpClient;
        // A Thread that reads data from the server
        private Thread _threadRead;
        // A delegate of a log method
        private readonly Action<string> _logCallback;

        private Timer m_tickTimer = new Timer(1000);

        public event ConnectionStateChangedHandler ConnectionStateChanged;

        // QOTD client constructor
        public QotdClient(string host, ushort port, Action<string> logCallback)
        {
            _logCallback = logCallback;
            try
            {
                // Creating a new TcpClient instance
                _tcpClient = new TcpClient();
            }
            catch (Exception e)
            {
                AddToLog(e.ToString());
                throw;
            }

            m_tickTimer.Elapsed += (object sender, global::System.Timers.ElapsedEventArgs e) => Tick();
            m_tickTimer.AutoReset = true;
            m_tickTimer.Start();

            AddToLog(string.Format("Start connection to {0}:{1}...", host, port));
            // Start the asynchronous connection procedure
            _tcpClient.BeginConnect(host, port, OnConnect, _tcpClient);
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
                    AddToLog(string.Format("server_msg: {0}.", Encoding.UTF8.GetString(buffer, 0, len)));
                }
                else
                {
                    AddToLog(string.Format("idle {0}.", DateTime.Now.ToLongTimeString()));
                }
            }
            catch (Exception ex)
            {
                Dispose();
            }
        }

        ~QotdClient()
        {
            FreeResources();
        }

        // Free the resources
        public void Dispose()
        {
            m_tickTimer.Stop();
            if (_tcpClient != null)
            {
                FreeResources();
                AddToLog(string.Format("Connection closed."));
                SysPost.InvokeMulticast(this, ConnectionStateChanged, new ConnectionStateChangedEventArgs(false));
            }
        }

        public void SendCommand(string cmd)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(cmd);

            try
            {
                _tcpClient.GetStream().Write(buffer, 0, buffer.Length);
            }
            catch (Exception)
            {
                Dispose();                
            }
        }

        private void FreeResources()
        {
            if (_tcpClient != null)
            {
                _tcpClient.Close();
                _tcpClient = null;
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
                    SysPost.InvokeMulticast(this, ConnectionStateChanged, new ConnectionStateChangedEventArgs(true));
                }
            }
            catch (Exception)
            {
                AddToLog(string.Format("Connection failed."));
                Dispose();
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
