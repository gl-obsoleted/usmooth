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
		UsCmdUtil.WriteIntList (cmd, _frameMeshes);
		UsCmdUtil.WriteIntList (cmd, _frameMaterials);
		UsCmdUtil.WriteIntList (cmd, _frameTextures);
		return cmd;
	}
}

public class MeshData {
	public int _instID;
	public int _vertCount;
	public int _triCount;
	public int _materialCount;
	public float _boundSize;
	
	public void Write(UsCmd cmd) {
		cmd.WriteInt32 (_instID);
		cmd.WriteInt32 (_vertCount);
		cmd.WriteInt32 (_triCount);
		cmd.WriteInt32 (_materialCount);
		cmd.WriteFloat (_boundSize);
	}
}

