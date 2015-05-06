/*!lic_info
The MIT License (MIT)

Copyright (c) 2015 SeaSunOpenSource

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

*/

using UnityEngine;
using System.Collections;


// setting the conditional to the platform of choice will only compile the method for that platform
// alternatively, use the #defines at the top of this file
public class LogMgr
{
	public static void Log( object format, params object[] paramList )
	{
		if( format is string )
			Debug.Log( string.Format( format as string, paramList ) );
		else
			Debug.Log( format );
	}
	
	
	public static void LogWarning( object format, params object[] paramList )
	{
		if( format is string )
			Debug.LogWarning( string.Format( format as string, paramList ) );
		else
			Debug.LogWarning( format );
	}
	
	
	public static void LogError( object format, params object[] paramList )
	{
		if( format is string )
			Debug.LogError( string.Format( format as string, paramList ) );
		else
			Debug.LogError( format );
	}
	
	
	public static void Assert( bool condition )
	{
		Assert( condition, string.Empty, true );
	}

	
	public static void Assert( bool condition, string assertString )
	{
		Assert( condition, assertString, false );
	}
	
	public static void Assert( bool condition, string assertString, bool pauseOnFail )
	{
		if( !condition )
		{
			Debug.LogError( "assert failed! " + assertString );
			
			if( pauseOnFail )
				Debug.Break();
		}
	}

}
