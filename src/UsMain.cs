using UnityEngine;
using System.Collections;
using LostPolygon.GoodOldSockets.Examples;
using System;
using UnityEditor;
using usmooth.common;
using System.Collections.Generic;

public class UsMain : MonoBehaviour {

	private UsNet _net;
	private ushort _serverPort = 5555;

	private long _currentTimeInMilliseconds = 0;
	private long _tickNetLast = 0;
	private long _tickNetInterval = 200;

	// Use this for initialization
	void Start () {
		Application.runInBackground = true;

		DataCollector.Instance = new DataCollector ();

		UsPerfManager.Instance = new UsPerfManager();

		UsNet.Instance = new UsNet(_serverPort);

		UsMainHandlers.Instance.RegisterHandlers (UsNet.Instance.CmdExecutor);
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
			
			if (UsNet.Instance != null) {
				UsNet.Instance.Update ();
			}

			_tickNetLast = _currentTimeInMilliseconds;
		}
	}

	private int SLICE_COUNT = 50;
	private bool NetHandle_RequestFrameData(eNetCmd cmd, UsCmd c) {
		if (UsPerfManager.Instance == null) 
			return true;

		var objects = UsPerfManager.Instance.VisibleObjects;
		if (objects.Count == 0) 
			return true;

		UsCmd begin = new UsCmd();
		begin.WriteNetCmd(eNetCmd.SV_FrameData_Mesh);
		begin.WriteInt16 ((short)eSubCmd_TransmitStage.DataBegin);
		begin.WriteInt32 (UsPerfManager.Instance.VisibleObjects.Count);
		UsNet.Instance.SendCommand (begin);

		for (int i = 0; i < objects.Count; i += SLICE_COUNT) {
			var slice = objects.GetRange(i, Math.Min(objects.Count - i, SLICE_COUNT));

			UsCmd fragment = new UsCmd();
			fragment.WriteNetCmd(eNetCmd.SV_FrameData_Mesh);
			fragment.WriteInt16 ((short)eSubCmd_TransmitStage.DataSlice);
			fragment.WriteInt32 (slice.Count);
			foreach (GameObject gameobject in slice) {
				UsPerfManager.Instance.WriteMesh(gameobject, fragment);
			}
			UsNet.Instance.SendCommand (fragment);
		}

		UsCmd end = new UsCmd();
		end.WriteNetCmd(eNetCmd.SV_FrameData_Mesh);
		end.WriteInt16 ((short)eSubCmd_TransmitStage.DataEnd);
		UsNet.Instance.SendCommand (end);

		UsNet.Instance.SendCommand(UsPerfManager.Instance.CreateMaterialCmd());
		UsNet.Instance.SendCommand(UsPerfManager.Instance.CreateTextureCmd());
		
		if (DataCollector.Instance != null) {
			FrameData data = DataCollector.Instance.CollectFrameData();
			UsNet.Instance.SendCommand (data.CreatePacket());
		}

		return true;
	}
}
