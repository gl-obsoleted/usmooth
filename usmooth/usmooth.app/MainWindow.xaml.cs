using FirstFloor.ModernUI.Windows.Controls;
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

namespace usmooth.app
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ModernWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            AppNetManager.Instance.LogicallyConnected += OnLogicallyConnected;
            AppNetManager.Instance.LogicallyDisconnected += OnLogicallyDisconnected;
        }

        private void OnLogicallyConnected(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.Title = string.Format("usmooth ({0})", AppNetManager.Instance.RemoteAddr);
            }));
        }

        private void OnLogicallyDisconnected(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.Title = "usmooth (not connected)";
            }));
        }
    }
}
