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
    #region Constants
    public const int MAX_CONTENT_LEN = 1024;
    public const int MAX_CALLSTACK_LEN = 1024;
    #endregion

    // main info
    public ushort SeqID;
    public UsLogType LogType;
    public string Content;

    // time info
    public float RealtimeSinceStartup;

    // debugging info
    public string Callstack;

    public UsLogPacket() 
    {
        SeqID = ushort.MaxValue;
    }

    public UsLogPacket(UsCmd c)
    {
        SeqID = (ushort)c.ReadInt16();
        LogType = (UsLogType)c.ReadInt32();
        Content = c.ReadString();
        RealtimeSinceStartup = c.ReadFloat();
        Callstack = c.ReadString();
    }

    public UsCmd CreatePacket()
    {
        UsCmd c = new UsCmd();
        c.WriteNetCmd(eNetCmd.SV_App_Logging);
        c.WriteInt16((short)SeqID);
        c.WriteInt32((int)LogType);
        c.WriteStringStripped(Content, MAX_CONTENT_LEN);
        c.WriteFloat(RealtimeSinceStartup);
        c.WriteStringStripped(Callstack, MAX_CALLSTACK_LEN);
        return c;
    }
}
