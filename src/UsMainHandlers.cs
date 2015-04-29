using UnityEngine;
using System.Collections;
using System;
using usmooth.common;
using System.Collections.Generic;

public class UsMainHandlers {
	public static UsMainHandlers Instance = new UsMainHandlers();

	public void RegisterHandlers(UsCmdParsing exec) {
		exec.RegisterHandler (eNetCmd.CL_Handshake, NetHandle_Handshake); 
		exec.RegisterHandler (eNetCmd.CL_KeepAlive, NetHandle_KeepAlive); 
		exec.RegisterHandler (eNetCmd.CL_ExecCommand, NetHandle_ExecCommand); 
		exec.RegisterHandler (eNetCmd.CL_FlyToObject, NetHandle_FlyToObject); 
		exec.RegisterHandler (eNetCmd.CL_RequestFrameData, NetHandle_RequestFrameData); 
		exec.RegisterHandler (eNetCmd.CL_FrameV2_RequestMeshes, NetHandle_FrameV2_RequestMeshes); 
		exec.RegisterHandler (eNetCmd.CL_FrameV2_RequestNames, NetHandle_FrameV2_RequestNames); 
	}
	
	private bool NetHandle_Handshake(eNetCmd cmd, UsCmd c) {
		Debug.Log ("executing handshake.");
		UsCmd reply = new UsCmd();
		reply.WriteNetCmd(eNetCmd.SV_HandshakeResponse);
		UsNet.Instance.SendCommand(reply);
		return true;
	}
	
	private bool NetHandle_KeepAlive(eNetCmd cmd, UsCmd c) {
		UsCmd reply = new UsCmd();
		reply.WriteNetCmd(eNetCmd.SV_KeepAliveResponse);
		UsNet.Instance.SendCommand(reply);
		return true;
	}

	private bool NetHandle_ExecCommand(eNetCmd cmd, UsCmd c) {
		string read = c.ReadString();

        var result = new KeyValuePair<eUserCmdResult, string>(eUserCmdResult.OK, "");
        if (!UsvStart.Instance.Console.ExecuteCommand(read))
            result = UsUserCommands.Instance.Execute(read);

		UsCmd reply = new UsCmd();
		reply.WriteNetCmd(eNetCmd.SV_ExecCommandResponse);
		reply.WriteInt32 ((int)result.Key);
		reply.WriteString (result.Value);
		UsNet.Instance.SendCommand(reply);
		return true;
	}

	private bool NetHandle_FlyToObject(eNetCmd cmd, UsCmd c) {
		int instID = c.ReadInt32 ();
		UsEditorNotifer.Instance.PostMessage_FlyToObject (instID);
		return true;
	}

	private int SLICE_COUNT = 50;
	private bool NetHandle_RequestFrameData(eNetCmd cmd, UsCmd c) {
		if (DataCollector.Instance == null) 
			return true;

		FrameData data = DataCollector.Instance.CollectFrameData();
		
		UsNet.Instance.SendCommand(data.CreatePacket());
		UsNet.Instance.SendCommand(DataCollector.Instance.CreateMaterialCmd());
		UsNet.Instance.SendCommand(DataCollector.Instance.CreateTextureCmd());

		UsCmd end = new UsCmd();
		end.WriteNetCmd(eNetCmd.SV_FrameDataEnd);
		UsNet.Instance.SendCommand (end);

		//Debug.Log(string.Format("creating frame packet: id {0} mesh count {1}", eNetCmd.SV_FrameDataV2, data._frameMeshes.Count));

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
