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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace usmooth.app
{
    public enum LogWndOpt
    {
        Info,
        Bold,
        Error,
        NetLog,
    }

    public delegate void LoggingHandler(LogWndOpt opt, string text);

    public class UsLogging
    {
        public static event LoggingHandler Receivers;

        public static void Debugf(string format, params object[] args)
        {
            System.Diagnostics.Debugger.Log(0, null, string.Format(format, args) + Environment.NewLine);
        }

        public static void Printf(string format, params object[] args)
        {
            Printf(LogWndOpt.Info, format, args);
        }

        public static void Printf(LogWndOpt opt, string format, params object[] args)
        {
            string formatted = string.Format(format, args);
            foreach (LoggingHandler Caster in Receivers.GetInvocationList())
            {
                ISynchronizeInvoke SyncInvoke = Caster.Target as ISynchronizeInvoke;
                try
                {
                    if (SyncInvoke != null && SyncInvoke.InvokeRequired)
                        SyncInvoke.Invoke(Caster, new object[] { opt, formatted });
                    else
                        Caster(opt, formatted);
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine("UsLogging.Printf() failed. \n");
                    Console.WriteLine("{0}:\n", ex.ToString());
                }
            }
        }
    }

    public class UsNetLogging
    {
        #region Constants
        public static Dictionary<UsLogType, string> s_type2color = new Dictionary<UsLogType, string>() {
        { UsLogType.Error,      "Red" },
        { UsLogType.Assert,     "Orange" },
        { UsLogType.Warning,    "Orange" },
        { UsLogType.Exception,  "Purple" },
        };

        public static string s_gameLogTimeColor = "DarkSeaGreen";
        #endregion

        #region Control-Vars

        // (later) these control-vars could be sent across the net
        // to prevent the game from emitting in the first place (to lower the network usage)

        public enum eLogLevel   // based on Unity logging categories 
        {
            Lv0_Silent,
            Lv1_ErrorsAndExceptions,
            Lv2_WarningsAndAsserts,
            Lv3_Logs,
        }

        public enum eLogCallstackLevel
        {
            Lv0_Hidden,
            Lv1_ShownOnExceptionsOnly,
            Lv2_ShownIfPossible,
        }

        public static eLogLevel s_logLevel = eLogLevel.Lv3_Logs;
        public static eLogCallstackLevel s_logCallstackLevel = eLogCallstackLevel.Lv2_ShownIfPossible;
        #endregion

        private static bool IsLogFiltered(UsLogType type)
        {
            switch (s_logLevel)
            {
                case eLogLevel.Lv0_Silent:
                    return true;
                case eLogLevel.Lv1_ErrorsAndExceptions:
                    return type == UsLogType.Assert || type == UsLogType.Warning || type == UsLogType.Log;
                case eLogLevel.Lv2_WarningsAndAsserts:
                    return type == UsLogType.Log;

                case eLogLevel.Lv3_Logs:
                default:
                    return false;
            }
        }

        private static bool IsCallstackFiltered(UsLogType type)
        {
            switch (s_logCallstackLevel)
            {
                case eLogCallstackLevel.Lv0_Hidden:
                    return true;
                case eLogCallstackLevel.Lv1_ShownOnExceptionsOnly:
                    return type != UsLogType.Exception;
                case eLogCallstackLevel.Lv2_ShownIfPossible:
                    return false;

                default:
                    return true;
            }
        }

        public static void Print(UsLogPacket packet)
        {
            if (IsLogFiltered(packet.LogType))
                return;

            string logTypeStr = "";
            switch (packet.LogType)
            {
                case UsLogType.Error:
                case UsLogType.Exception:
                case UsLogType.Assert:
                case UsLogType.Warning:
                    logTypeStr = string.Format("[b][color={0}]({1})[/color][/b]", s_type2color[packet.LogType], packet.LogType);
                    break;

                case UsLogType.Log:
                default:
                    break;
            }

            string timeStr = string.Format("[color={0}]{1:0.00}({2})[/color]", s_gameLogTimeColor, packet.RealtimeSinceStartup, packet.SeqID);

            string ret = string.Format("{0} {1} {2}", timeStr, logTypeStr, packet.Content);

            if (!IsCallstackFiltered(packet.LogType) && !string.IsNullOrEmpty(packet.Callstack))
            {
                ret += string.Format("\n[color=DarkGray]{0}[/color]", packet.Callstack);
            }

            UsLogging.Printf(LogWndOpt.NetLog, ret);
        }
    }
}
