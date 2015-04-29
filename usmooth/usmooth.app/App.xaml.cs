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
        protected override void OnStartup(StartupEventArgs e)
        {
            NetManager.Instance = new NetManager();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            NetManager.Instance.Dispose();
        }
    }
}
