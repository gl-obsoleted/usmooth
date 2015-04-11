using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace usmooth.common
{
    public delegate void EtCmdHandler(short cmd, UsCmd c);

    public class UsCmdParsing
    {
        public void RegisterHandler(eNetCmd cmd, EtCmdHandler handler)
        {
            m_handlers[cmd] = handler;
        }

        public void Execute(UsCmd c)
        {
            try
            {
                short cmdId = c.ReadInt16();
                m_handlers[(eNetCmd)cmdId](cmdId, c);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[cmd] Execution failed. ({0})", ex.Message);
            }
        }

        Dictionary<eNetCmd, EtCmdHandler> m_handlers = new Dictionary<eNetCmd, EtCmdHandler>();
    }
}
