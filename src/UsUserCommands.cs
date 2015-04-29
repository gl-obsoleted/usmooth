using UnityEngine;
using System.Collections;
using System;
using usmooth.common;
using System.Collections.Generic;
	
public enum eUserCmdResult
{
	OK,
	Error,
}

public class UsUserCommands {
	public static UsUserCommands Instance = new UsUserCommands();

	public KeyValuePair<eUserCmdResult, string> Execute(string cmd) {
		//Debug.Log ("executing command: " + cmd);

		string[] ca = cmd.Split ();
		if (ca.Length == 0) {
			return new KeyValuePair<eUserCmdResult, string> (eUserCmdResult.Error, "empty command.");
		} else if (ca [0] == "showmesh") {
			int instID = 0;
			if (int.TryParse(ca[1], out instID)) {
				return ShowMesh(instID, true);
			}
		} else if (ca [0] == "hidemesh") {
			int instID = 0;
			if (int.TryParse(ca[1], out instID)) {
				return ShowMesh(instID, false);
			}
		}

		return new KeyValuePair<eUserCmdResult, string> (eUserCmdResult.Error, "unknown command.");
	}
	
	private KeyValuePair<eUserCmdResult, string> ShowMesh(int instID, bool visible) {
		MeshRenderer[] meshRenderers = UnityEngine.Object.FindObjectsOfType(typeof(MeshRenderer)) as MeshRenderer[];
		foreach (MeshRenderer mr in meshRenderers) {
			if (mr.gameObject.GetInstanceID() == instID) {
				mr.enabled = visible;
				return new KeyValuePair<eUserCmdResult, string> (eUserCmdResult.OK, "");
			}
		}

		return new KeyValuePair<eUserCmdResult, string> (eUserCmdResult.Error, "mesh not found. <ShowMesh>");
	}
}
