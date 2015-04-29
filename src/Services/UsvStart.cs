using System;
using System.IO;
using UnityEngine;

public class UsvStart
{
    public static UsvStart Instance;

    public UsvLogging Logging;
    public UsvConsole Console;

    public UsvStart(UsNet net)
    {
        if (net == null)
        {
            Debug.Break();
            return;
        }

        Logging = new UsvLogging(net);
        Application.RegisterLogCallbackThreaded(Logging.OnLogReceived);

        Console = new UsvConsole();
    }
}