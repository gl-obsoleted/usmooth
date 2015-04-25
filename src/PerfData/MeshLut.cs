using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using usmooth.common;

public class MeshLut {
	public bool AddMesh(GameObject go) {
		if (_lut.ContainsKey(go.GetInstanceID())) {
			return true;
		}

		// returns false if renderer is not available
		if (go.renderer == null) {
			return false;
		}
		
		// returns false if not a mesh
		MeshFilter mf = (MeshFilter)go.GetComponent (typeof(MeshFilter));
		if (mf == null) {
			return false;
		}

		MeshData md = new MeshData ();
		md._instID = go.GetInstanceID ();
		md._vertCount = mf.mesh.vertexCount;
		md._triCount = mf.mesh.triangles.Length / 3;
		md._materialCount = go.renderer.sharedMaterials.Length;
		md._boundSize = go.renderer.bounds.size.magnitude;
		_lut.Add (md._instID, md);
		return true;
	}

	public void WriteMesh(int instID, UsCmd cmd) {
		MeshData data;
		if (_lut.TryGetValue(instID, out data)) {
			data.Write(cmd);
		}
	}

	Dictionary<int, MeshData> _lut = new Dictionary<int, MeshData>();
}
