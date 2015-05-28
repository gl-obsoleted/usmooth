using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using usmooth.common;

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

    public void PostEvent_EffectStressTest(string effectName, int effectCount)
    {
        SysPost.InvokeMulticast(this, EffectStressTest, new UsEffectStressTestEventArgs(effectName, effectCount));
    }

    public void PostEvent_EffectStressTestResult(string effectName, float avgMilliseconds)
    {
        SysPost.InvokeMulticast(this, EffectStressTestResult, new UsEffectStressTestResultEventArgs(effectName, avgMilliseconds));
    }

    public event SysPost.StdMulticastDelegation EffectStressTest;
    public event SysPost.StdMulticastDelegation EffectStressTestResult;
}
