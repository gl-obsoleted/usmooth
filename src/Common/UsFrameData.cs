using System.Collections;
using System.Collections.Generic;
using usmooth.common;

public class FrameData {
	public int _frameCount = 0;
	
	// time info
	public float _frameDeltaTime = 0.0f;
	public float _frameRealTime = 0.0f;
	public float _frameStartTime = 0.0f;
	
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
