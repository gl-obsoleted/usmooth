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

using System;
using System.IO;
using UnityEngine;
using usmooth.common;

public class UsvLogging : IDisposable
{
    public UsvLogging(UsNet net, bool logIntoFile)
    {
        if (net == null)
        {
            Debug.Break();
            return;
        }
         
        _net = net;

        if (logIntoFile)
        {
            DateTime dt = DateTime.Now;

            string logDir = SysUtil.CombinePaths(Application.persistentDataPath, "log", SysUtil.FormatDateAsFileNameString(dt));
            Directory.CreateDirectory(logDir);

            string logPath = Path.Combine(logDir, SysUtil.FormatTimeAsFileNameString(dt) + ".txt");

            _logWriter = new FileInfo(logPath).CreateText();
            _logWriter.AutoFlush = true;
            _logPath = logPath;
        }

        RegisterCallback();

        DumpCurrentState();
    }

    public void Dispose()
    {
        if (_logWriter != null)
        {
            _logWriter.Close();
        }
    }

    public void DumpCurrentState()
    {
        if (_logWriter != null)
        {
            Debug.Log(string.Format("Log file opened successfully. ('{0}')", _logPath));
        }
        else
        {
            Debug.Log(string.Format("Log file is disabled. (check out 'Log Into File' in 'UsMain.cs')"));
        }
    }

    private void RegisterCallback()
    {
#if UNITY_5_0
        Application.logMessageReceivedThreaded += OnLogReceived;
#else
        Application.RegisterLogCallbackThreaded(OnLogReceived);
#endif
    }

    private void OnLogReceived(string condition, string stackTrace, LogType type)
    {
        // do nothing if usmooth connection is not available (yet)
        if (_net == null)
            return;

        UsLogPacket pkt = new UsLogPacket();
        pkt.SeqID = _seqID++;
        pkt.RealtimeSinceStartup = Time.realtimeSinceStartup;
        pkt.Content = condition;
        pkt.LogType = (UsLogType)type;

        switch (type)
        {
            case LogType.Assert:
                _assertCount++;
                break;
            case LogType.Error:
                _errorCount++;
                pkt.Callstack = stackTrace;
                break;
            case LogType.Exception:
                _exceptionCount++;
                pkt.Callstack = stackTrace;
                break;

            case LogType.Warning:
            case LogType.Log:
            default:
                break;
        }

        if (_logWriter != null)
        {
            _logWriter.WriteLine("{0:0.00} {1} {2}", pkt.RealtimeSinceStartup, pkt.LogType, pkt.Content);
        }
        
        _net.SendCommand(pkt.CreatePacket());
    }

    private UsNet _net;

    private string _logPath;
    private StreamWriter _logWriter;

    private ushort _seqID = 0;
    private int _assertCount = 0;
    private int _errorCount = 0;
    private int _exceptionCount = 0;
}