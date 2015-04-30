using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace usmooth.common
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
}
