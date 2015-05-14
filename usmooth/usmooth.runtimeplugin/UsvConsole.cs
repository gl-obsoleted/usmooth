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
using UnityEngine;

public delegate bool UsvConsoleCmdHandler(string[] args);

public class UsvConsole
{
    public static UsvConsole Instance;

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
