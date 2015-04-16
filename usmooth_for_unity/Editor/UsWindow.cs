using UnityEditor;
using UnityEngine;
using System;
using usmooth.common;

class UsWindow : EditorWindow {
	[MenuItem("Tools/usmooth")]
	public static void Init() {
		var win = EditorWindow.GetWindow(typeof(UsWindow), true, "usmooth") as UsWindow;
		if (win != null) {
			win.autoRepaintOnSceneChange = true;
			win.Show();
		}
	}
	public void OnGUI() {
		float width = Mathf.Min(390f, Screen.width);
		float height = Mathf.Min(650f, Screen.height);
		GUILayout.BeginArea(
			new Rect(
			Screen.width / 2f - width / 2f,
			Screen.height / 2f - height / 2f, 
			width, 
			height
			),
			"usmooth", 
			"Window"
			);
		
		GUILayout.BeginVertical();
		GUILayout.Space(5f);
		
		if (_selectedIDs != null) {
			EditorGUILayout.LabelField (string.Format("selected object count: {0}", _selectedIDs.Length));	
		}

		GUILayout.EndVertical();
		GUILayout.EndArea();
	}

    private int[] _selectedIDs;
	public void OnSelectionChange() {
		_selectedIDs = Selection.instanceIDs;

		if (UsNet.Instance != null) {
			UsCmd cmd = new UsCmd();
			cmd.WriteNetCmd(eNetCmd.SV_Editor_SelectionChanged);
			cmd.WriteInt32 (_selectedIDs.Length);
			Debug.Log(string.Format("selection count: {0}", _selectedIDs.Length));
			foreach (int item in _selectedIDs) {
				cmd.WriteInt32 (item);
			}
			UsNet.Instance.SendCommand(cmd);		
		}

		Repaint ();
	}
}

