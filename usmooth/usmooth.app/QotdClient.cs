using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace usmooth.app
{
    #region QOTD client
    public class QotdClient : IDisposable
    {
        private TcpClient _tcpClient;
        // A Thread that reads data from the server
        private Thread _threadRead;
        // A delegate of a log method
        private readonly Action<string> _logCallback;

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

            AddToLog(string.Format("Start connection to {0}:{1}...", host, port));
            // Start the asyncronous connection procedure
            _tcpClient.BeginConnect(host, port, OnConnect, _tcpClient);
        }

        ~QotdClient()
        {
            FreeResources();
        }

        // Free the resources
        public void Dispose()
        {
            if (_tcpClient != null)
            {
                FreeResources();
                AddToLog(string.Format("Connection closed."));
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
            if (!tcpClient.Connected)
                return;

            try
            {
                // Finish the connection procedure
                tcpClient.EndConnect(asyncResult);
                AddToLog(string.Format("Connection to {0} successfull.", tcpClient.Client.RemoteEndPoint));

                // Start data read thread
                _threadRead = new Thread(() => ThreadReadProcedure(tcpClient));
                _threadRead.Start();
            }
            catch (SocketException ex)
            {
                AddToLog(string.Format("<color=red>Error at TCP connection: {0}</color>", ex.Message));
            }
            catch (ObjectDisposedException)
            {
                // The listener was Stop()'d, disposing the underlying socket and
                // triggering the completion of the callback. We're already exiting,
                // so just return.
            }
            catch (Exception ex)
            {
                // Some other error occured. This should not happen
                //Debug.LogException(ex);
                AddToLog(string.Format("<color=red>An error occured: {0}</color>", ex.Message));
            }
        }

        // Receives data until connection is interrupted
        private void ThreadReadProcedure(TcpClient tcpClient)
        {
            // A string that will contain the received data
            string data = string.Empty;
            // A temporary byte[] buffer for receiving data
            byte[] buffer = new byte[256];
            // Number of bytes received last time
            int receivedBytes;

            // Receive the data until the end
            while ((receivedBytes = tcpClient.Client.Receive(buffer)) != 0)
            {
                // Add newly-received data to a string
                data += Encoding.UTF8.GetString(buffer, 0, receivedBytes);
            }

            // No more data to read, display the quote
            AddToLog(string.Format("Quote Of The Day:\r\n<b><size=15>{0}</size></b>", data));
            AddToLog(string.Format("Disconnected from {0}.", tcpClient.Client.RemoteEndPoint));
        }

        // Adds a formatted entry to the log
        private void AddToLog(string text)
        {
            _logCallback(string.Format("<color=blue>[client]</color> <color=black>{0}</color>", text.Trim()));
        }
    }
    #endregion
}
