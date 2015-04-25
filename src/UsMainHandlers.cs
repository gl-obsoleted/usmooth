using UnityEngine;
using System.Collections;
using LostPolygon.GoodOldSockets.Examples;
using System;
using UnityEditor;
using usmooth.common;
using System.Collections.Generic;

public class UsMainHandlers : MonoBehaviour {
	public static UsMainHandlers Instance = new UsMainHandlers();

	public void RegisterHandlers(UsCmdParsing exec) {
		exec.RegisterHandler (eNetCmd.CL_RequestFrameData, NetHandle_RequestFrameData); 
		exec.RegisterHandler (eNetCmd.CL_FrameV2_RequestMeshes, NetHandle_FrameV2_RequestMeshes); 
		exec.RegisterHandler (eNetCmd.CL_FrameV2_RequestNames, NetHandle_FrameV2_RequestNames); 
	}

	private int SLICE_COUNT = 50;
	private bool NetHandle_RequestFrameData(eNetCmd cmd, UsCmd c) {
		if (UsPerfManager.Instance == null) 
			return true;

//		var objects = UsPerfManager.Instance.VisibleObjects;
//		if (objects.Count == 0) 
//			return true;
//
//		UsCmd begin = new UsCmd();
//		begin.WriteNetCmd(eNetCmd.SV_FrameData_Mesh);
//		begin.WriteInt16 ((short)eSubCmd_TransmitStage.DataBegin);
//		begin.WriteInt32 (UsPerfManager.Instance.VisibleObjects.Count);
//		UsNet.Instance.SendCommand (begin);
//
//		for (int i = 0; i < objects.Count; i += SLICE_COUNT) {
//			var slice = objects.GetRange(i, Math.Min(objects.Count - i, SLICE_COUNT));
//
//			UsCmd fragment = new UsCmd();
//			fragment.WriteNetCmd(eNetCmd.SV_FrameData_Mesh);
//			fragment.WriteInt16 ((short)eSubCmd_TransmitStage.DataSlice);
//			fragment.WriteInt32 (slice.Count);
//			foreach (GameObject gameobject in slice) {
//				UsPerfManager.Instance.WriteMesh(gameobject, fragment);
//			}
//			UsNet.Instance.SendCommand (fragment);
//		}
//
//		UsCmd end = new UsCmd();
//		end.WriteNetCmd(eNetCmd.SV_FrameData_Mesh);
//		end.WriteInt16 ((short)eSubCmd_TransmitStage.DataEnd);
//		UsNet.Instance.SendCommand (end);

		UsNet.Instance.SendCommand(UsPerfManager.Instance.CreateMaterialCmd());
		UsNet.Instance.SendCommand(UsPerfManager.Instance.CreateTextureCmd());
		
		if (DataCollector.Instance != null) {
			FrameData data = DataCollector.Instance.CollectFrameData();
			UsNet.Instance.SendCommand (data.CreatePacket());
			//Debug.Log(string.Format("creating frame packet: id {0} mesh count {1}", eNetCmd.SV_FrameDataV2, data._frameMeshes.Count));
		}

		return true;
	}
	
	private bool NetHandle_FrameV2_RequestMeshes(eNetCmd cmd, UsCmd c) {
		if (DataCollector.Instance != null) {
			List<int> meshIDs = UsCmdUtil.ReadIntList(c);
			//Debug.Log(string.Format("requesting meshes - count ({0})", meshIDs.Count));
			foreach (var slice in UsGeneric.Slice(meshIDs, SLICE_COUNT)) {
				UsCmd fragment = new UsCmd();
				fragment.WriteNetCmd(eNetCmd.SV_FrameDataV2_Meshes);
				fragment.WriteInt32 (slice.Count);
				foreach (int meshID in slice) {
					DataCollector.Instance.MeshTable.WriteMesh(meshID, fragment);
				}
				UsNet.Instance.SendCommand (fragment);
			}
		}

		return true;
	}
	
	private bool NetHandle_FrameV2_RequestNames(eNetCmd cmd, UsCmd c) {
		if (DataCollector.Instance != null) {
			List<int> instIDs = UsCmdUtil.ReadIntList(c);
			foreach (var slice in UsGeneric.Slice(instIDs, SLICE_COUNT)) {
				UsCmd fragment = new UsCmd();
				fragment.WriteNetCmd(eNetCmd.SV_FrameDataV2_Names);
				fragment.WriteInt32 (slice.Count);
				foreach (int instID in slice) {
					DataCollector.Instance.WriteName(instID, fragment);
				}
				UsNet.Instance.SendCommand (fragment);
			}
		}

		return true;
	}
}
