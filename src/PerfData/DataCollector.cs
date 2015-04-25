using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using usmooth.common;

public class DataCollector {
	public static DataCollector Instance = new DataCollector ();

	public FrameData CollectFrameData() {
		
		//Debug.Log(string.Format("creating frame data. {0}", Time.frameCount));
		_currentFrame = new FrameData ();
		_currentFrame._frameCount = Time.frameCount;
		_currentFrame._frameDeltaTime = Time.deltaTime;
		_currentFrame._frameRealTime = Time.realtimeSinceStartup;
		_currentFrame._frameStartTime = Time.time;

		MeshRenderer[] meshRenderers = UnityEngine.Object.FindObjectsOfType(typeof(MeshRenderer)) as MeshRenderer[];
		foreach (MeshRenderer mr in meshRenderers) {
			if (mr.isVisible) {
				GameObject go = mr.gameObject;
				if (_meshLut.AddMesh(go)) {
					//Debug.Log(string.Format("CollectFrameData(): adding game object. {0}", go.GetInstanceID()));
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
	private List<FrameData> _frames = new List<FrameData>();
}
