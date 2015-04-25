using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using usmooth.common;

public class UsFlyToObjectEventArgs : EventArgs {
	public UsFlyToObjectEventArgs(int instID) {
		_instID = instID;
	}

	public int _instID = 0;
}

// the notifier would do nothing in game 
// 		and notify the editor if the event is handled
public class UsEditorNotifer {
	public static UsEditorNotifer Instance = new UsEditorNotifer ();

	public void PostMessage_FlyToObject(int instID) {
		SysPost.InvokeMulticast (this, OnFlyToObject, new UsFlyToObjectEventArgs(instID));
	}

	public event SysPost.StdMulticastDelegation OnFlyToObject;
}
