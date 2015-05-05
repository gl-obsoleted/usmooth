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
using System.IO;
using UnityEngine;

public class UsLogging : MonoBehaviour
{
    private int _assertCount = 0;
    private int _errorCount = 0;
    private int _exceptionCount = 0;

    void Awake()
    {
        Application.RegisterLogCallbackThreaded(OnLogReceived);
    }

    private void OnLogReceived(string condition, string stackTrace, LogType type)
    {
        // do nothing if usmooth connection is not available (yet)
        if (UsNet.Instance == null)
            return;

        UsLogPacket pkt = new UsLogPacket();
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

        UsNet.Instance.SendCommand(pkt.CreatePacket());
    }

    void OnGUI()
    {
        GUILayout.Label(string.Format("Count: {0}", _exceptionCount));
        if (GUILayout.Button("Application Exception"))
        {
            throw new ApplicationException();
        }
        if (GUILayout.Button("Null Reference"))
        {
            GameObject go = null;
            Debug.Log(go.name);
        }
        if (GUILayout.Button("Float Divide By Zero"))
        {
            float x = 3.14f;
            float y = 0.0f;
            float z = x / y;
            Debug.Log(z.ToString());
        }
        if (GUILayout.Button("Integer Divide By Zero"))
        {
            int x = 42;
            int y = 0;
            int z = x / y;
            Debug.Log(z.ToString());
        }
        if (GUILayout.Button("Stack Overflow"))
        {
            OverflowStack(1, 2, 3);
        }
    }

    private int OverflowStack(int a, int b, int c)
    {
        return OverflowStack(c, b, a) + OverflowStack(b, c, a + 1);
    }
}