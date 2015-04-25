using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using usmooth.common;

namespace usmooth.app
{
    public class NetClient : IDisposable
    {
        public event SysPost.StdMulticastDelegation Connected;
        public event SysPost.StdMulticastDelegation Disconnected;

        public bool IsConnected { get { return _tcpClient != null; } }

        public string RemoteAddr { get { return IsConnected ? _tcpClient.Client.RemoteEndPoint.ToString() : ""; } }

        public void Connect(string host, int port)
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

                _host = "";
                _port = 0;

                UsLogging.Printf("connection closed.");
                SysPost.InvokeMulticast(this, Disconnected);
            }
        }

        public void RegisterCmdHandler(eNetCmd cmd, EtCmdHandler handler)
        {
            _cmdParser.RegisterHandler(cmd, handler);
        }

        public void Tick_CheckConnectionStatus()
        {
            try
            {
                if (!_tcpClient.Connected)
                {
                    UsLogging.Printf("disconnection detected. (_tcpClient.Connected == false).");
                    throw new Exception();
                }

                // check if the client socket is still readable
                if (_tcpClient.Client.Poll(0, SelectMode.SelectRead))
                {
                    byte[] checkConn = new byte[1];
                    if (_tcpClient.Client.Receive(checkConn, SocketFlags.Peek) == 0)
                    {
                        UsLogging.Printf("disconnection detected. (failed to read by Poll/Receive).");
                        throw new IOException();
                    }
                }
            }
            catch (Exception ex)
            {
                DisconnectOnError("disconnection detected while checking connection status.", ex);
            }
        }

        public void Tick_ReceivingData()
        {
            try
            {
                while (_tcpClient.Available > 0)
                {
                    byte[] cmdLenBuf = new byte[2];
                    int cmdLenRead = _tcpClient.GetStream().Read(cmdLenBuf, 0, cmdLenBuf.Length);
                    ushort cmdLen = BitConverter.ToUInt16(cmdLenBuf, 0);
                    if (cmdLenRead > 0 && cmdLen > 0)
                    {
                        byte[] buffer = new byte[cmdLen];
                        int len = _tcpClient.GetStream().Read(buffer, 0, buffer.Length);

                        UsCmd cmd = new UsCmd(buffer);
                        UsCmdExecResult result = _cmdParser.Execute(cmd);
                        switch (result)
                        {
                            case UsCmdExecResult.Succ:
                                break;
                            case UsCmdExecResult.Failed:
                                UsLogging.Printf("net cmd execution failed: {0}.", new UsCmd(buffer).ReadNetCmd());
                                break;
                            case UsCmdExecResult.HandlerNotFound:
                                UsLogging.Printf("net unknown cmd: {0}.", new UsCmd(buffer).ReadNetCmd());
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DisconnectOnError("error detected while receiving data.", ex);
            }
        }

        public void Dispose()
        {
            Disconnect();
        }

        public void SendPacket(UsCmd cmd)
        {
            try
            {
                byte[] cmdLenBytes = BitConverter.GetBytes((ushort)cmd.WrittenLen);
                _tcpClient.GetStream().Write(cmdLenBytes, 0, cmdLenBytes.Length);
                _tcpClient.GetStream().Write(cmd.Buffer, 0, cmd.WrittenLen);
            }
            catch (Exception ex)
            {
                DisconnectOnError("error detected while sending data.", ex);
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
                    UsLogging.Printf("connected successfully.");
                    SysPost.InvokeMulticast(this, Connected);
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                DisconnectOnError("connection failed while handling OnConnect().", ex);
            }
        }

        private void DisconnectOnError(string info, Exception ex)
        {
            UsLogging.Printf(LogWndOpt.Bold, info);
            UsLogging.Printf(ex.ToString());

            Disconnect();
        }

        private string _host = "";
        private int _port = 0;
        private TcpClient _tcpClient;
        private UsCmdParsing _cmdParser = new UsCmdParsing();
    }
}
