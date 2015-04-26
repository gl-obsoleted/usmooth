
using System.Collections.Generic;
using usmooth.common;
using System;

public delegate bool UsCmdHandler(eNetCmd cmd, UsCmd c);

public enum UsCmdExecResult
{
	Succ,
	Failed,
	HandlerNotFound,
}

public class UsCmdParsing
{
	public void RegisterHandler(eNetCmd cmd, UsCmdHandler handler)
	{
		m_handlers[cmd] = handler;
	}
	
	public UsCmdExecResult Execute(UsCmd c)
	{
		try
		{
			eNetCmd cmd = c.ReadNetCmd();
			UsCmdHandler handler;
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
	
	Dictionary<eNetCmd, UsCmdHandler> m_handlers = new Dictionary<eNetCmd, UsCmdHandler>();
}

