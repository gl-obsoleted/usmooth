
using System.Collections.Generic;
using usmooth.common;

public class UsCmdUtil 
{
	public static List<int> ReadIntList(UsCmd c) {
		List<int> ret = new List<int>();
		int count = c.ReadInt32 ();
		for (int i = 0; i < count; i++) {
			ret.Add(c.ReadInt32());
		}
		return ret;
	}
	
	public static void WriteIntList(UsCmd c, List<int> l) {
		c.WriteInt32 (l.Count);
		foreach (var item in l) {
			c.WriteInt32 (item);
		}
	}
}

