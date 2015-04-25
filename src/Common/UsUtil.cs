using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace usmooth.app
{
    public class SysPost
    {
        public static bool AssertException(bool expr, string msg)
        {
#if DEBUG
            System.Diagnostics.Debug.Assert(expr);
            return expr;
#else
            if (expr)
                return true;

            throw new Exception(msg);
#endif
        }

        public delegate void StdMulticastDelegation(object sender, EventArgs e);

        public static void InvokeMulticast(object sender, MulticastDelegate md)
        {
            if (md != null)
            {
                InvokeMulticast(sender, md, null);
            }
        }

        public static void InvokeMulticast(object sender, MulticastDelegate md, EventArgs e)
        {
            if (md == null)
                return;

            foreach (StdMulticastDelegation Caster in md.GetInvocationList())
            {
                ISynchronizeInvoke SyncInvoke = Caster.Target as ISynchronizeInvoke;
                try
                {
                    if (SyncInvoke != null && SyncInvoke.InvokeRequired)
                        SyncInvoke.Invoke(md, new object[] { sender, e });
                    else
                        Caster(sender, e);
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine("Event handling failed. \n");
                    Console.WriteLine("{0}:\n", ex.ToString());
                }
            }
        }
    }

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
