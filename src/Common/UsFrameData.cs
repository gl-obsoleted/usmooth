using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using usmooth.common;

public class FrameData {
	public int _frameCount = Time.frameCount;
	
	// time info
	public float _frameDeltaTime = Time.deltaTime;
	public float _frameRealTime = Time.realtimeSinceStartup;
	public float _frameStartTime = Time.time;
	
	// actual data
	public List<int> _frameMeshes = new List<int>();
	public List<int> _frameMaterials = new List<int>();
	public List<int> _frameTextures = new List<int>();

	public UsCmd CreatePacket() {
		UsCmd cmd = new UsCmd();
		cmd.WriteNetCmd (eNetCmd.SV_FrameDataV2);
		cmd.WriteInt32 (_frameCount);
		cmd.WriteFloat (_frameDeltaTime);
		cmd.WriteFloat (_frameRealTime);
		cmd.WriteFloat (_frameStartTime);

		cmd.WriteInt32 (_frameMeshes.Count);
		foreach (var item in _frameMeshes) {
			cmd.WriteInt32 (item);
		}
		cmd.WriteInt32 (_frameMaterials.Count);
		foreach (var item in _frameMaterials) {
			cmd.WriteInt32 (item);
		}
		cmd.WriteInt32 (_frameTextures.Count);
		foreach (var item in _frameMaterials) {
			cmd.WriteInt32 (item);
		}
		return cmd;
	}
}
