using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using usmooth.common;

public class DataCollector : MonoBehaviour {
	public static DataCollector Instance;

	public FrameData CollectFrameData() {
		MeshRenderer[] meshRenderers = UnityEngine.Object.FindObjectsOfType(typeof(MeshRenderer)) as MeshRenderer[];
		
		_currentFrame = new FrameData ();
		foreach (MeshRenderer mr in meshRenderers) {
			if (mr.isVisible) {
				GameObject go = mr.gameObject;
				if (_meshLut.AddMesh(go)) {
					_currentFrame._frameMeshes.Add (go.GetInstanceID());
					_nameLut[go.GetInstanceID()] = go.name;
				}
			}
		}
		_frames.Add (_currentFrame);
		return _currentFrame;
	}

	public void WriteName(int instID, UsCmd cmd) {
		string data;
		if (_nameLut.TryGetValue(instID, out data)) {
			cmd.WriteInt32 (instID);
			cmd.WriteStringStripped (data);
		}
	}

	public MeshLut MeshTable { get { return _meshLut; } }
	private MeshLut _meshLut = new MeshLut();
	private Dictionary<int, string> _nameLut = new Dictionary<int, string>();

	private FrameData _currentFrame;
	private List<FrameData> _frames;
}
