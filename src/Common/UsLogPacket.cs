using System;
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
    public static List<UsLogType> s_boldTypes = new List<UsLogType>() {
        UsLogType.Error,
        UsLogType.Assert,
        UsLogType.Exception,
    };

    public static Dictionary<UsLogType, string> s_type2color = new Dictionary<UsLogType, string>() {
        { UsLogType.Error,      "Red" },
        { UsLogType.Assert,     "Orange" },
        { UsLogType.Warning,    "Gold" },
        { UsLogType.Log,        "DarkGray" },
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
            ret += string.Format("\n{0}", Callstack);
        }
        return ret;
    }

    private string GetTimeDecorated()
    {
        return string.Format("[color={0}]{1:0.00}[/color]", s_gameLogTimeColor, RealtimeSinceStartup);
    }

    private string GetLogTypeDecorated()
    {
        string ret = string.Format("[color={0}]{1}[/color]", s_type2color[LogType], LogType);
        if (s_boldTypes.Contains(LogType))
        {
            ret = string.Format("[b]{0}[/b]", ret);
        }
        return ret;
    }
}
