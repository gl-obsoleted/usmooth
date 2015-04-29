using UnityEngine;
using System.Collections;
using LostPolygon.GoodOldSockets.Examples;
using System;
using usmooth.common;
using System.Collections.Generic;

public class UsMain : MonoBehaviour {

	private ushort _serverPort = 5555;
	private long _currentTimeInMilliseconds = 0;
	private long _tickNetLast = 0;
	private long _tickNetInterval = 200;

	void Start () 
    {
		Application.runInBackground = true;

		UsNet.Instance = new UsNet(_serverPort);

        UsvStart.Instance = new UsvStart(UsNet.Instance);

		UsMainHandlers.Instance.RegisterHandlers (UsNet.Instance.CmdExecutor);
	}
	
	void Update () {
		_currentTimeInMilliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

		if (_currentTimeInMilliseconds - _tickNetLast > _tickNetInterval)
		{
            Debug.LogWarning(string.Format("warning t-{0}", _currentTimeInMilliseconds));

			if (UsNet.Instance != null) {
				UsNet.Instance.Update ();
			}

			_tickNetLast = _currentTimeInMilliseconds;
		}
	}
}
