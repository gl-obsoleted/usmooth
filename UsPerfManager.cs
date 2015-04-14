using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using UnityEditor;
using usmooth.common;

public class UsPerfManager {

	public static UsPerfManager Instance;

	#region Gathered GameObjects/Materials/Textures

	public List<GameObject> VisibleObjects { get { return m_visibleObjects; } }
	private List<GameObject> m_visibleObjects = new List<GameObject>();
	
	public Dictionary<Material, HashSet<GameObject>> VisibleMaterials { get { return m_visibleMaterials; } }
	private Dictionary<Material, HashSet<GameObject>> m_visibleMaterials = new Dictionary<Material, HashSet<GameObject>>();
	
	public Dictionary<Texture, HashSet<Material>> VisibleTextures { get { return m_visibleTextures; } }
	private Dictionary<Texture, HashSet<Material>> m_visibleTextures = new Dictionary<Texture, HashSet<Material>>();

	private Dictionary<Texture, int> m_textureSizeLut = new Dictionary<Texture, int>();

	#endregion Gathered GameObjects/Materials/Textures

	public void GotoObject(int instID) {
		foreach (var obj in VisibleObjects) {
			if (obj.GetInstanceID() == instID) {
				SceneView.currentDrawingSceneView.LookAt(obj.transform.position);
				Selection.activeGameObject=obj;
			}
		}
	}

	public void RefreshVisibleMeshes() {

		m_visibleObjects.Clear ();
		m_visibleMaterials.Clear ();
		m_visibleTextures.Clear ();

		MeshRenderer[] mrs = UnityEngine.Object.FindObjectsOfType(typeof(MeshRenderer)) as MeshRenderer[];
		foreach (MeshRenderer mr in mrs) {
			if (mr.isVisible) {
				m_visibleObjects.Add(mr.gameObject);

				foreach (var mat in mr.sharedMaterials) {
					AddVisibleMaterial(mat, mr.gameObject);

					if (mat != null) {
						if (Application.isEditor) {
							int cnt = ShaderUtil.GetPropertyCount(mat.shader);
							for (int i = 0; i < cnt; i++) {
								if (ShaderUtil.GetPropertyType(mat.shader, i) == ShaderUtil.ShaderPropertyType.TexEnv) {
									string propName = ShaderUtil.GetPropertyName(mat.shader, i);
									AddVisibleTexture(mat.GetTexture(propName), mat);
								}
							}
						} else {
							AddVisibleTexture(mat.mainTexture, mat);
						}
					}
				}
			}
		}
	}

	private void AddVisibleMaterial(Material mat, GameObject gameobject)
	{
		if (mat != null) {
			if (!m_visibleMaterials.ContainsKey(mat)) {
				m_visibleMaterials.Add(mat, new HashSet<GameObject>());
			}
			m_visibleMaterials[mat].Add(gameobject);
		}
	}

	private void AddVisibleTexture(Texture texture, Material ownerMat)
	{
		if (texture != null) {
			if (!m_visibleTextures.ContainsKey(texture)) {
				m_visibleTextures.Add(texture, new HashSet<Material>());
			}
			m_visibleTextures[texture].Add(ownerMat);

			// refresh the size
			if (!m_textureSizeLut.ContainsKey(texture)) {
				m_textureSizeLut[texture] = UsTextureUtil.CalculateTextureSizeBytes(texture);
			}
		}
	}

	public void DumpAllInfo() {

		Debug.Log (string.Format ("{0} visible meshes ({1}), visible materials ({2}), visible textures ({3})",
		                        DateTime.Now.ToLongTimeString (),
		                        VisibleObjects.Count, 
		                        VisibleMaterials.Count,
		                        VisibleTextures.Count));
		
		string objectInfo = "";
		foreach (GameObject gameobject in VisibleObjects) {
			MeshFilter mf = (MeshFilter)gameobject.GetComponent (typeof(MeshFilter));
			int matCount = gameobject.renderer.sharedMaterials.Length;
			float size = mf.mesh.bounds.size.magnitude;
			objectInfo += string.Format ("{0} {1} {2} {3} {4}\n", gameobject.name, mf.mesh.name, mf.mesh.vertexCount, matCount, size);
		}
		Debug.Log (objectInfo);
		
		string matInfo = "";
		foreach (KeyValuePair<Material, HashSet<GameObject>> kv in VisibleMaterials) {
			matInfo += string.Format ("{0} {1} {2}\n", kv.Key.name, kv.Key.shader.name, kv.Value.Count);
		}
		Debug.Log (matInfo);
		
		string texInfo = "";
		foreach (KeyValuePair<Texture, HashSet<Material>> kv in VisibleTextures) {
			Texture tex = kv.Key;
			texInfo += string.Format ("{0} {1} {2} {3} {4}\n", tex.name, tex.width, tex.height, kv.Value.Count, UsTextureUtil.FormatSizeString(m_textureSizeLut[tex] / 1024));
		}
		Debug.Log (texInfo);
	}
	
	public UsCmd CreateMeshCmd() {
		UsCmd cmd = new UsCmd();
		cmd.WriteNetCmd(eNetCmd.SV_FrameData_Mesh);
		// Debug.Log (string.Format("mesh count: {0}", VisibleObjects.Count));
		cmd.WriteInt32 (VisibleObjects.Count);
		foreach (GameObject gameobject in VisibleObjects) {
			MeshFilter mf = (MeshFilter)gameobject.GetComponent (typeof(MeshFilter));
			cmd.WriteInt32 (gameobject.GetInstanceID());
			cmd.WriteString (gameobject.name);
			cmd.WriteString (mf.mesh.name);
			cmd.WriteInt32 (mf.mesh.vertexCount);
			cmd.WriteInt32 (gameobject.renderer.sharedMaterials.Length);
			cmd.WriteInt32 ((int)mf.mesh.bounds.size.magnitude);
		}
		return cmd;
	}
	
	public UsCmd CreateMaterialCmd() {
		UsCmd cmd = new UsCmd();
		cmd.WriteNetCmd(eNetCmd.SV_FrameData_Material);
		cmd.WriteInt32 (VisibleMaterials.Count);
		
		foreach (KeyValuePair<Material, HashSet<GameObject>> kv in VisibleMaterials) {
			//Debug.Log (string.Format("current_material: {0} - {1} - {2}", kv.Key.GetInstanceID(), kv.Key.name.Length, kv.Key.name));
			cmd.WriteInt32 (kv.Key.GetInstanceID());
			cmd.WriteString (kv.Key.name);
			cmd.WriteString (kv.Key.shader.name);

			cmd.WriteInt32 (kv.Value.Count);
			foreach (var item in kv.Value) {
				cmd.WriteInt32 (item.GetInstanceID());
			}
		}
		return cmd;
	}
	
	public UsCmd CreateTextureCmd() {
		UsCmd cmd = new UsCmd();
		cmd.WriteNetCmd(eNetCmd.SV_FrameData_Texture);
		cmd.WriteInt32 (VisibleTextures.Count);
		
		foreach (KeyValuePair<Texture, HashSet<Material>> kv in VisibleTextures) {
			cmd.WriteInt32 (kv.Key.GetInstanceID());
			cmd.WriteString (kv.Key.name);
			cmd.WriteString (string.Format("{0}x{1}", kv.Key.width, kv.Key.height));
			cmd.WriteString (UsTextureUtil.FormatSizeString(m_textureSizeLut[kv.Key] / 1024));

			cmd.WriteInt32 (kv.Value.Count);
			foreach (var item in kv.Value) {
				cmd.WriteInt32 (item.GetInstanceID());
			}
		}

		return cmd;
	}
}
