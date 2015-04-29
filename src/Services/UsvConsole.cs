using System;
using System.Collections.Generic;
using UnityEngine;

public delegate bool UsvConsoleCmdHandler(string[] args);

public class UsvConsole
{
    public void RegisterHandler(string cmd, UsvConsoleCmdHandler handler)
    {
        _handlers[cmd.ToLower()] = handler;
    }

    public bool ExecuteCommand(string fullcmd)
    {
        string[] fragments = fullcmd.Split();
        if (fragments.Length == 0)
        {
            Debug.Log("empty command received, ignored.");
            return false;
        }

        UsvConsoleCmdHandler handler;
        if (!_handlers.TryGetValue(fragments[0].ToLower(), out handler))
        {
            Debug.Log(string.Format("unknown command ('{0}') received, ignored.", fullcmd));
            return false;            
        }

        return handler(fragments);
    }

    private Dictionary<string, UsvConsoleCmdHandler> _handlers = new Dictionary<string, UsvConsoleCmdHandler>();
}
