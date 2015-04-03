using NNanomsg;
using NNanomsg.Protocols;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace usmooth.app
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        PairSocket m_sock;

        const string socketAddress = "tcp://localhost:5088";

        protected override void OnStartup(StartupEventArgs e)
        {
            m_sock = new PairSocket();

            m_sock.Connect(socketAddress);

            m_sock.Send(Encoding.UTF8.GetBytes("the message is " + DateTime.Now.ToLongTimeString()));
        }

        protected override void OnExit(ExitEventArgs e)
        {
            m_sock.Dispose();
        }

    }
}
