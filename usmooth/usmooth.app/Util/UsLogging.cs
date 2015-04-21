using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace usmooth.app
{
    public enum LogWndOpt
    {
        Info,
        Bold,
        Error,
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
}
