
using System.Collections.Generic;
using usmooth.common;
using System.Text;
using System;
using System.Runtime.InteropServices;

public enum UsCmdIOErrorCode 
{
	ReadOverflow,
	WriteOverflow,
	TypeMismatched,
}

public class UsCmdIOError : Exception
{
	static Dictionary<UsCmdIOErrorCode, string> InfoLut = new Dictionary<UsCmdIOErrorCode, string> () {
		{ UsCmdIOErrorCode.ReadOverflow, "Not enough space for reading." },
		{ UsCmdIOErrorCode.WriteOverflow, "Not enough space for writing." },
		{ UsCmdIOErrorCode.TypeMismatched, "Reading/writing a string as a primitive." },
	};

	public UsCmdIOError(UsCmdIOErrorCode code) : base("[UsCmdIOError] " + InfoLut[code]) 
	{
		ErrorCode = code;
	}

	public UsCmdIOErrorCode ErrorCode;
}

public class UsCmd
{
	public const int STRIP_NAME_MAX_LEN = 64;
	public const int BUFFER_SIZE = 16 * 1024;
	
	public UsCmd()
	{
		_buffer = new byte[BUFFER_SIZE];
	}
	
	public UsCmd(byte[] given)
	{
		_buffer = given;
	}

	public byte[] Buffer { get { return _buffer; }	}
	public int WrittenLen { get { return _writeOffset; } }

	public object ReadPrimitive<T>()
	{
		if (typeof(T) == typeof(string)) 
			throw new UsCmdIOError(UsCmdIOErrorCode.TypeMismatched);
		
		if (_readOffset + Marshal.SizeOf(typeof(T)) > _buffer.Length)
			throw new UsCmdIOError(UsCmdIOErrorCode.ReadOverflow);

		object val = UsGeneric.Convert<T> (_buffer, _readOffset);
		_readOffset += Marshal.SizeOf(typeof(T));
		return val;
	}

	public string ReadString()
	{
		short strLen = ReadInt16();
		if (_readOffset + (int)strLen > _buffer.Length)
			throw new UsCmdIOError(UsCmdIOErrorCode.ReadOverflow);
		
		string ret = Encoding.Default.GetString(_buffer, _readOffset, (int)strLen);
		_readOffset += (int)strLen;
		return ret;
	}
	
	public void WritePrimitive<T>(T value) 
	{
		if (typeof(T) == typeof(string)) 
			throw new UsCmdIOError(UsCmdIOErrorCode.TypeMismatched);

		if (_writeOffset + Marshal.SizeOf(typeof(T)) > _buffer.Length)
			throw new UsCmdIOError(UsCmdIOErrorCode.WriteOverflow);
		
		byte[] byteArray = UsGeneric.Convert(value);
		if (byteArray == null) 
			throw new UsCmdIOError(UsCmdIOErrorCode.TypeMismatched);

		byteArray.CopyTo(_buffer, _writeOffset);
		_writeOffset += Marshal.SizeOf(typeof(T));
	}

	public void WriteStringStripped(string value, short stripLen)
	{
		string stripped = value.Length > stripLen ? value.Substring(0, stripLen) : value;
		WritePrimitive ((short)stripped.Length);

		byte[] byteArray = Encoding.Default.GetBytes(stripped);
		byteArray.CopyTo(_buffer, _writeOffset);
		_writeOffset += stripped.Length;
	}
	
	public eNetCmd ReadNetCmd() 			{ return (eNetCmd)ReadInt16(); }
	public short ReadInt16() 				{ return (short)ReadPrimitive<short>();	}
	public int ReadInt32() 					{ return (int)ReadPrimitive<int>();	}
	public float ReadFloat()				{ return (float)ReadPrimitive<float>();	}
	
	public void WriteNetCmd(eNetCmd cmd)	{	WritePrimitive((short)cmd);	}
	public void WriteInt16(short value)		{	WritePrimitive(value);	}
	public void WriteInt32(int value) 		{	WritePrimitive(value);	}
	public void WriteFloat(float value)		{	WritePrimitive(value);	}
	public void WriteStringStripped(string value) 	{ WriteStringStripped (value, STRIP_NAME_MAX_LEN); }
	public void WriteString(string value) 			{ WriteStringStripped (value, short.MaxValue); }
	
	private int _writeOffset = 0;
	private int _readOffset = 0;
	private byte[] _buffer;
}

