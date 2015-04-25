using System.Collections;
using System.Collections.Generic;
using usmooth.common;
using System;

public class UsGeneric {
	public static IEnumerable<List<T>> Slice<T>(List<T> objList, int slice) {
		for (int i = 0; i < objList.Count; i += slice) {
			yield return objList.GetRange(i, Math.Min(objList.Count - i, slice));
		}
	}
}
