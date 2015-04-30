using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace usmooth.app
{
    public enum LogWndOpt
    {
        Info,
        Bold,
        Error,
        NetLog,
    }

    public delegate void LoggingHandler(LogWndOpt opt, string text);

    public class UsLogging
    {
        public static event LoggingHandler Receivers;

        public static void Debugf(string format, params object[] args)
        {
            System.Diagnostics.Debugger.Log(0, null, string.Format(format, args) + Environment.NewLine);
        }

        public static void Printf(string format, params object[] args)
        {
            Printf(LogWndOpt.Info, format, args);
        }

        public static void Printf(LogWndOpt opt, string format, params object[] args)
        {
            string formatted = string.Format(format, args);
            foreach (LoggingHandler Caster in Receivers.GetInvocationList())
            {
                ISynchronizeInvoke SyncInvoke = Caster.Target as ISynchronizeInvoke;
                try
                {
                    if (SyncInvoke != null && SyncInvoke.InvokeRequired)
                        SyncInvoke.Invoke(Caster, new object[] { opt, formatted });
                    else
                        Caster(opt, formatted);
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine("UsLogging.Printf() failed. \n");
                    Console.WriteLine("{0}:\n", ex.ToString());
                }
            }
        }
    }

    public class UsNetLogging
    {
        #region Constants
        public static Dictionary<UsLogType, string> s_type2color = new Dictionary<UsLogType, string>() {
        { UsLogType.Error,      "Red" },
        { UsLogType.Assert,     "Orange" },
        { UsLogType.Warning,    "Orange" },
        { UsLogType.Exception,  "Purple" },
        };

        public static string s_gameLogTimeColor = "DarkSeaGreen";
        #endregion

        public static void Print(UsLogPacket packet)
        {
            string timeStr = string.Format("[color={0}]{1:0.00}({2})[/color]", s_gameLogTimeColor, packet.RealtimeSinceStartup, packet.SeqID);

            string logTypeStr = "";
            switch (packet.LogType)
            {
                case UsLogType.Error:
                case UsLogType.Assert:
                case UsLogType.Warning:
                case UsLogType.Exception:
                    logTypeStr = string.Format("[b][color={0}]({1})[/color][/b]", s_type2color[packet.LogType], packet.LogType);
                    break;

                case UsLogType.Log:
                default:
                    break;
            }

            string ret = string.Format("{0} {1} {2}", timeStr, logTypeStr, packet.Content);
            if (!string.IsNullOrEmpty(packet.Callstack))
            {
                ret += string.Format("\n[color=DarkGray]{0}[/color]", packet.Callstack);
            }

            UsLogging.Printf(LogWndOpt.NetLog, ret);
        }
    }
}
