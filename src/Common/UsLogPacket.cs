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
    public const int MAX_CONTENT_LEN = 1024;
    public const int MAX_CALLSTACK_LEN = 1024;

    // main info
    public UsLogType LogType;
    public string Content;

    // thread info
    //public int ThreadID;
    //public string ThreadName;

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
        string ret = string.Format("[color=DarkSeaGreen]{0:0.00}[/color] [color=LightGray]{1}[/color] {2}", RealtimeSinceStartup, LogType, Content);
        if (!string.IsNullOrEmpty(Callstack))
        {
            ret += string.Format("\n{0}", Callstack);
        }
        return ret;
    }
}
