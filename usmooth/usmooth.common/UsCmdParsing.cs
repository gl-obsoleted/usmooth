using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace usmooth.common
{
    public delegate bool EtCmdHandler(eNetCmd cmd, UsCmd c);

    public enum UsCmdExecResult
    {
        Succ,
        Failed,
        HandlerNotFound,
    }

    public class UsCmdParsing
    {
        public void RegisterHandler(eNetCmd cmd, EtCmdHandler handler)
        {
            m_handlers[cmd] = handler;
        }

        public UsCmdExecResult Execute(UsCmd c)
        {
            try
            {
                eNetCmd cmd = c.ReadNetCmd();
                EtCmdHandler handler;
                if (!m_handlers.TryGetValue(cmd, out handler))
                {
                    return UsCmdExecResult.HandlerNotFound;
                }

                if (handler(cmd, c))
                {
                    return UsCmdExecResult.Succ;
                }
                else
                {
                    return UsCmdExecResult.Failed;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[cmd] Execution failed. ({0})", ex.Message);
                return UsCmdExecResult.Failed;
            }
        }

        Dictionary<eNetCmd, EtCmdHandler> m_handlers = new Dictionary<eNetCmd, EtCmdHandler>();
    }
}
