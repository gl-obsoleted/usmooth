using UnityEngine;
using System.Collections;
using LostPolygon.GoodOldSockets.Examples;
using System;
using UnityEditor;

public class UsMain : MonoBehaviour {

	private UsNet _net;
	private ushort _serverPort = 5555;

	private long _currentTimeInMilliseconds = 0;
	private long _tickNetLast = 0;
	private long _tickNetInterval = 200;

	// Use this for initialization
	void Start () {
		Application.runInBackground = true;

		UsPerfManager.Instance = new UsPerfManager();

		_net = new UsNet(_serverPort);
	}
	
	// Update is called once per frame
	void Update () {
		_currentTimeInMilliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

		if (_currentTimeInMilliseconds - _tickNetLast > _tickNetInterval)
		{
			if (UsPerfManager.Instance != null) {
				UsPerfManager.Instance.RefreshVisibleMeshes();
				// UsPerfManager.Instance.DumpAllInfo();
			}

			if (_net != null) {
				_net.Update ();
			}

			_tickNetLast = _currentTimeInMilliseconds;
		}
	}
}
