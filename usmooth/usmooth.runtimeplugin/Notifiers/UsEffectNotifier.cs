using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using usmooth.common;

public class UsEffectListEventArgs : EventArgs
{
    public UsEffectListEventArgs(string[] effectNameList)
    {
        _effectNameList = effectNameList;
    }

    public string[] _effectNameList;
}

public class UsEffectStressTestEventArgs : EventArgs
{
    public UsEffectStressTestEventArgs(string effectName, int effectCount)
    {
        _effectName = effectName;
        _effectCount = effectCount;
    }

    public string _effectName;
    public int _effectCount = 0;
}

public class UsEffectStressTestResultEventArgs : EventArgs
{
    public UsEffectStressTestResultEventArgs(string effectName, float avgMilliseconds)
    {
        _effectName = effectName;
        _avgMilliseconds = avgMilliseconds;
    }

    public string _effectName;
    public float _avgMilliseconds = 0;
}

public class UsEffectNotifier
{
    public static UsEffectNotifier Instance = new UsEffectNotifier();

    public void PostEvent_QueryEffectList()
    {
        SysPost.InvokeMulticast(this, QueryEffectList);
    }

    public void PostEvent_QueryEffectListResult(string[] effectNameList)
    {
        SysPost.InvokeMulticast(this, QueryEffectListResult, new UsEffectListEventArgs(effectNameList));
    }

    public void PostEvent_RunEffectStressTest(string effectName, int effectCount)
    {
        SysPost.InvokeMulticast(this, RunEffectStressTest, new UsEffectStressTestEventArgs(effectName, effectCount));
    }

    public void PostEvent_RunEffectStressTestResult(string effectName, float avgMilliseconds)
    {
        SysPost.InvokeMulticast(this, RunEffectStressTestResult, new UsEffectStressTestResultEventArgs(effectName, avgMilliseconds));
    }

    public event SysPost.StdMulticastDelegation QueryEffectList;
    public event SysPost.StdMulticastDelegation QueryEffectListResult;
    public event SysPost.StdMulticastDelegation RunEffectStressTest;
    public event SysPost.StdMulticastDelegation RunEffectStressTestResult;
}
