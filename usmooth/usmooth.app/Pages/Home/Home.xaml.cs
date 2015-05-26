/*!lic_info

The MIT License (MIT)

Copyright (c) 2015 SeaSunOpenSource

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

*/

﻿using System;
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
using System.Configuration;
using usmooth.common;

namespace usmooth.app.Pages
{

    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : UserControl
    {
        public Home()
        {
            if (NetManager.Instance == null)
                throw new Exception();

            InitializeComponent();

            // refresh recent target IP list
            cb_targetIP.Items.Add(Properties.Settings.Default.LocalAddr);
            if (Properties.Settings.Default.RecentAddrList != null)
            {
                foreach (var item in Properties.Settings.Default.RecentAddrList)
                {
                    cb_targetIP.Items.Add(item);
                }
            }
            cb_targetIP.SelectedItem = cb_targetIP.Items[0];

            bb_logging.BBCode = string.Empty;

            cb_logLevel.ItemsSource = Enum.GetValues(typeof(UsNetLogging.eLogLevel)).Cast<UsNetLogging.eLogLevel>();
            cb_logLevel.SelectedItem = UsNetLogging.s_logLevel;
            cb_logCallstackLevel.ItemsSource = Enum.GetValues(typeof(UsNetLogging.eLogCallstackLevel)).Cast<UsNetLogging.eLogCallstackLevel>();
            cb_logCallstackLevel.SelectedItem = UsNetLogging.s_logCallstackLevel;

            //AddSwitcher("t1", "t1/t1/t1", true);
            //AddSlider("t1", 0, 100, 50);

            UsLogging.Receivers += Impl_PrintLogToWnd;
            UsLogging.Printf("usmooth is initialized successfully.");

            RegisterNetHandlers();
        }

        public void AddSwitcher(string name, string path, bool initialValue)
        {
            RoutedEventHandler handler = (sender, e) =>
            {
                CheckBox c = sender as CheckBox;
                if (c == null || c.Tag == null || string.IsNullOrEmpty((string)c.Tag))
                    return;

                NetManager.Instance.ExecuteCmd(string.Format("toggle {0} {1}", c.Tag, ((bool)c.IsChecked ? 1 : 0)));
            };

            CheckBox cb = new CheckBox();
            cb.Tag = name;
            cb.Margin = new Thickness(10, 5, 0, 5);
            cb.Content = name;
            cb.ToolTip = path;
            cb.IsChecked = initialValue;
            cb.Checked += handler;
            cb.Unchecked += handler;
            cb.MinWidth = 150;
            _switchersPanel.Children.Add(cb);
        }

        public void AddSlider(string name, double minVal, double maxVal, double initialVal)
        {
            Label label = new Label();
            label.Content = name;
            label.MinWidth = 150;
            _slidersPanel.Children.Add(label);

            Slider slider = new Slider();
            slider.Tag = name;
            slider.Margin = new Thickness(10, 5, 0, 5);
            slider.Minimum = minVal;
            slider.Maximum = maxVal;
            slider.Value = initialVal;
            _slidersPanel.Children.Add(slider);

            slider.ValueChanged += (sender, e) => {
                Slider s = sender as Slider;
                if (s == null || s.Tag == null || string.IsNullOrEmpty((string)s.Tag))
                    return;

                NetManager.Instance.ExecuteCmd(string.Format("slide {0} {1:0.00}", s.Tag, s.Value));
            };
        }

        private void bt_connect_Click(object sender, RoutedEventArgs e)
        {
            if (NetManager.Instance == null)
            {
                UsLogging.Printf(LogWndOpt.Bold, "NetManager not available, connecting failed.");
                return;
            }

            if (!NetManager.Instance.Connect(cb_targetIP.Text))
            {
                UsLogging.Printf(LogWndOpt.Bold, "connecting failed.");
                return;
            }

            // temporarily disable all connection buttons to prevent double submitting
            cb_targetIP.IsEnabled = false;
            bt_connect.IsEnabled = false;
            bt_disconnect.IsEnabled = false;
        }

        private void Impl_PrintLogToWnd(LogWndOpt opt, string text)
        {
            m_loggingPanel.Dispatcher.Invoke(new Action(() =>
            {
                string time = string.Format("[color=Gray]{0}[/color]", DateTime.Now.ToLongTimeString());
                string content = "";
                switch (opt)
                {
                    case LogWndOpt.Info:
                        content = string.Format("{0} {1}\r\n", time, text);
                        break;
                    case LogWndOpt.Bold:
                        content = string.Format("{0} [b]{1}[/b]\r\n", time, text);
                        break;
                    case LogWndOpt.Error:
                        content = string.Format("{0} [color=Red]{1}[/color]\r\n", time, text);
                        break;
                    case LogWndOpt.NetLog:
                        content = string.Format("{0} ([color=SeaGreen]{1}[/color]) {2}\r\n", time, cb_targetIP.Text, text);
                        break;
                    default:
                        break;
                }
                bb_logging.BBCode += content;
                m_loggingPanel.ScrollToBottom();
            }));
        }

        private void bt_exec_cmd_Click(object sender, RoutedEventArgs e)
        {
            ExecInputCmd();
        }

        private void tb_cmdbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                ExecInputCmd();
            }
        }

        private void ExecInputCmd()
        {
            NetManager.Instance.ExecuteCmd(tb_cmdbox.Text);
            tb_cmdbox.Clear();
        }

        private void bt_disconnect_Click(object sender, RoutedEventArgs e)
        {
            UsLogging.Printf(LogWndOpt.Error, "[b]disconnecting manually...[/b]");
            NetManager.Instance.Disconnect();
        }

        private void tb_cmdbox_GotFocus(object sender, RoutedEventArgs e)
        {
            _logSettingsExpander.IsExpanded = false;
        }

        private void cb_logLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UsNetLogging.s_logLevel = (UsNetLogging.eLogLevel)cb_logLevel.SelectedItem;
        }

        private void cb_logCallstackLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UsNetLogging.s_logCallstackLevel = (UsNetLogging.eLogCallstackLevel)cb_logCallstackLevel.SelectedItem;
        }
    }
}
