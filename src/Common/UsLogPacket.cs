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
using usmooth.common;

// This is copied from UnityEngine.dll to keep compatible with it 
public enum UsLogType 
{
    Error = 0,
    Assert = 1,
    Warning = 2,
    Log = 3,
    Exception = 4,
}

public class UsLogPacket
{
    public static Dictionary<UsLogType, string> s_type2color = new Dictionary<UsLogType, string>() {
        { UsLogType.Error,      "Red" },
        { UsLogType.Assert,     "Orange" },
        { UsLogType.Warning,    "Orange" },
        { UsLogType.Exception,  "Purple" },
    };

    public static string s_gameLogTimeColor = "DarkSeaGreen";

    public const int MAX_CONTENT_LEN = 1024;
    public const int MAX_CALLSTACK_LEN = 1024;

    // main info
    public UsLogType LogType;
    public string Content;

    // time info
    public float RealtimeSinceStartup;

    // debugging info
    public string Callstack;

    public UsLogPacket()
    {

    }

    public UsLogPacket(UsCmd c)
    {
        LogType = (UsLogType)c.ReadInt32();
        Content = c.ReadString();
        RealtimeSinceStartup = c.ReadFloat();
        Callstack = c.ReadString();
    }

    public UsCmd CreatePacket()
    {
        UsCmd c = new UsCmd();
        c.WriteNetCmd(eNetCmd.SV_App_Logging);
        c.WriteInt32((int)LogType);
        c.WriteStringStripped(Content, MAX_CONTENT_LEN);
        c.WriteFloat(RealtimeSinceStartup);
        c.WriteStringStripped(Callstack, MAX_CALLSTACK_LEN);
        return c;
    }

    public string Print()
    {
        string ret = string.Format("{0} {1} {2}", GetTimeDecorated(), GetLogTypeDecorated(), Content);
        if (!string.IsNullOrEmpty(Callstack))
        {
            ret += string.Format("\n[color=DarkGray]{0}[/color]", Callstack);
        }
        return ret;
    }

    private string GetTimeDecorated()
    {
        return string.Format("[color={0}]{1:0.00}[/color]", s_gameLogTimeColor, RealtimeSinceStartup);
    }

    private string GetLogTypeDecorated()
    {
        switch (LogType)
        {
            case UsLogType.Error:
            case UsLogType.Assert:
            case UsLogType.Warning:
            case UsLogType.Exception:
                return string.Format("[b][color={0}]({1})[/color][/b]", s_type2color[LogType], LogType);

            case UsLogType.Log:
            default:
                return "";
        }
    }
}
