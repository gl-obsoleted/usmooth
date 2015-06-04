using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using usmooth.common;

namespace usmooth.app.Pages.Effect
{
    public class EffectObject : INotifyPropertyChanged 
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Name { get { return _name; } set { _name = value; OnPropertyChanged("Name"); } }
        private string _name;

        public int MSAvg { get { return _avgMS; } set { _avgMS = value; OnPropertyChanged("MSAvg"); } }
        private int _avgMS;

        public int MSMax { get { return _maxMS; } set { _maxMS = value; OnPropertyChanged("MSMax"); } }
        private int _maxMS;

        public int TotalDrawCall { get { return _drawCall; } set { _drawCall = value; OnPropertyChanged("DrawCall"); } }
        private int _drawCall;

        public int TotalParticles { get { return _particleCount; } set { _particleCount = value; OnPropertyChanged("ParticleCount"); } }
        private int _particleCount;

        // Create the OnPropertyChanged method to raise the event 
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }

    /// <summary>
    /// Interaction logic for Effect.xaml
    /// </summary>
    public partial class Effect : UserControl
    {
        public Effect()
        {
            InitializeComponent();

            NetManager.Instance.RegisterCmdHandler(eNetCmd.SV_StressTestNames, NetHandle_StressTestNames);
            NetManager.Instance.RegisterCmdHandler(eNetCmd.SV_StressTestResult, NetHandle_StressTestResult);
        }

        private void bt_getEffectList_Click(object sender, RoutedEventArgs e)
        {
            NetManager.Instance.ExecuteCmd("get_effect_list");
        }

        private void bt_runStressTest_Click(object sender, RoutedEventArgs e)
        {
            _effectsToBePopulated.Clear();
            foreach (var item in EffectGrid.Items)
            {
                EffectObject mo = item as EffectObject;
                if (mo != null)
                {
                    _effectsToBePopulated.Add(mo.Name);
                }
            }

            _effectsEnumerator = _effectsToBePopulated.GetEnumerator();
            _isEffectEnumeratorAvailable = true;

            RunNextEffectStressTest();
        }

        void RunNextEffectStressTest()
        {
            if (!_isEffectEnumeratorAvailable)
                return;

            bool iterating = _effectsEnumerator.MoveNext();
            if (iterating)
            {
                string effectName = _effectsEnumerator.Current;
                NetManager.Instance.ExecuteCmd(string.Format("run_effect_stress {0} 100", effectName));
            }
            else
            {
                _effectsEnumerator.Dispose();
            }
        }

        bool NetHandle_StressTestNames(eNetCmd cmd, UsCmd c)
        {
            var effects = new ObservableCollection<EffectObject>();
            int count = c.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var e = new EffectObject();
                e.Name = c.ReadString();
                effects.Add(e);
            }

            EffectGrid.Dispatcher.Invoke(new Action(() =>
            {
                EffectGrid.DataContext = effects;
            }));
            return true;
        }

        bool NetHandle_StressTestResult(eNetCmd cmd, UsCmd c)
        {
            string name = c.ReadString();
            int avgMS = c.ReadInt32();
            int maxMS = c.ReadInt32();

            EffectGrid.Dispatcher.Invoke(new Action(() =>
            {
                foreach (var item in EffectGrid.Items)
                {
                    EffectObject mo = item as EffectObject;
                    if (mo != null && mo.Name == name)
                    {
                        if (_highlighted != null)
                        {
                            DataGridUtil.ClearHighlighted(EffectGrid, _highlighted);
                        }

                        mo.MSAvg = avgMS;
                        mo.MSMax = maxMS;

                        DataGridUtil.MarkAsHighlighted(EffectGrid, item, Colors.Chartreuse);
                        _highlighted = mo;

                        break;
                    }
                }
            }));

            RunNextEffectStressTest();

            return true;
        }

        List<string> _effectsToBePopulated = new List<string>();
        List<string>.Enumerator _effectsEnumerator;
        bool _isEffectEnumeratorAvailable = false;
        EffectObject _highlighted = null;
    }
}
