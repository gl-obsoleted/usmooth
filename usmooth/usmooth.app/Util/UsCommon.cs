using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace usmooth.common
{
    public enum eNetCmd
    {
        None,

        CL_CmdBegin             = 1000,
        CL_Handshake,
        CL_KeepAlive,
        CL_ExecCommand,
        CL_RequestFrameData,
        CL_FlyToObject,
        CL_CmdEnd,

        SV_CmdBegin             = 2000,
        SV_HandshakeResponse,
        SV_KeepAliveResponse,
        SV_ExecCommandResponse,
        SV_FrameData_Mesh,
        SV_FrameData_Material,
        SV_FrameData_Texture,
        SV_Editor_SelectionChanged,
        SV_CmdEnd,
    }

    public enum eSubCmd_TransmitStage
    {
        DataBegin,
        DataSlice,
        DataEnd,
    }


    public class UsCmd
    {
        public const int STRIP_NAME_MAX_LEN = 64;
        public const int DefaultCmdSize = 16 * 1024;

        public UsCmd()
        {
            m_buffer = new byte[DefaultCmdSize];
        }

        public UsCmd(byte[] given)
        {
            m_buffer = given;
        }

        public eNetCmd ReadNetCmd()
        {
            if (m_readOffset != 0)
                throw new System.Exception("net command should be read as the first 2 bytes.");

            short val = ReadInt16();
            return (eNetCmd)val;
        }

        public short ReadInt16()
        {
            if (m_readOffset + sizeof(short) > m_buffer.Length)
            {
                throw new System.Exception("无法读取，已到达消息末尾。");
            }

            short cmd = BitConverter.ToInt16(m_buffer, m_readOffset);
            m_readOffset += sizeof(short);
            return cmd;
        }

        public int ReadInt32()
        {
            if (m_readOffset + sizeof(int) > m_buffer.Length)
            {
                throw new System.Exception("无法读取，已到达消息末尾。");
            }

            int cmd = BitConverter.ToInt32(m_buffer, m_readOffset);
            m_readOffset += sizeof(int);
            return cmd;
        }

        public float ReadFloat()
        {
            if (m_readOffset + sizeof(float) > m_buffer.Length)
            {
                throw new System.Exception("无法读取，已到达消息末尾。");
            }

            float val = BitConverter.ToSingle(m_buffer, m_readOffset);
            m_readOffset += sizeof(float);
            return val;
        }

        public string ReadString()
        {
            short strLen = ReadInt16();
            if (m_readOffset + (int)strLen > m_buffer.Length)
            {
                throw new System.Exception("无法读取，已到达消息末尾。");
            }

            string ret = Encoding.Default.GetString(m_buffer, m_readOffset, (int)strLen);
            //if (ret[ret.Length - 1] == 0)
            //{
            //    char[] cStrTail = { '\0' };
            //    ret = ret.TrimEnd(cStrTail);
            //}

            m_readOffset += (int)strLen;
            return ret;
        }

        public void WriteNetCmd(eNetCmd cmd)
        {
            if (m_writeOffset != 0)
                throw new System.Exception("net command should be written as the first 2 bytes.");

            WriteInt16((short)cmd);
        }

        public void WriteInt16(short value)
        {
            if (m_writeOffset + sizeof(short) > m_buffer.Length)
            {
                throw new System.Exception("无法写入，已到达消息末尾。");
            }

            byte[] byteArray = BitConverter.GetBytes(value);

            byteArray.CopyTo(m_buffer, m_writeOffset);

            m_writeOffset += sizeof(short);
        }

        public void WriteInt32(int value)
        {
            if (m_writeOffset + sizeof(int) > m_buffer.Length)
            {
                throw new System.Exception("无法写入，已到达消息末尾。");
            }

            byte[] byteArray = BitConverter.GetBytes(value);

            byteArray.CopyTo(m_buffer, m_writeOffset);

            m_writeOffset += sizeof(int);
        }

        public void WriteFloat(float value)
        {
            if (m_writeOffset + sizeof(float) > m_buffer.Length)
            {
                throw new System.Exception("无法写入，已到达消息末尾。");
            }

            byte[] byteArray = BitConverter.GetBytes(value);

            byteArray.CopyTo(m_buffer, m_writeOffset);

            m_writeOffset += sizeof(float);
        }

        public void WriteStringStripped(string value)
        {
            string toBeWritten = value;
            if (value.Length > STRIP_NAME_MAX_LEN)
                toBeWritten = value.Substring(0, STRIP_NAME_MAX_LEN);

            short len = (short)toBeWritten.Length;
            WriteInt16(len);

            byte[] byteArray = Encoding.Default.GetBytes(toBeWritten);
            byteArray.CopyTo(m_buffer, m_writeOffset);
            m_writeOffset += toBeWritten.Length;
        }

        public void WriteString(string value)
        {
            short len = (short)value.Length;
            //len += 1; // 为了和 EtCmdPacket::WriteCStr() 保持一致

            if (len >= short.MaxValue)
            {
                throw new System.Exception("字符串长度超过了最大值。");
            }

            WriteInt16(len);

            byte[] byteArray = Encoding.Default.GetBytes(value);
            byteArray.CopyTo(m_buffer, m_writeOffset);
            m_writeOffset += value.Length;
        }


        public byte[] Buffer
        {
            get { return m_buffer; }
        }

        public int WrittenLen { get { return m_writeOffset; } }

        int m_writeOffset = 0;
        int m_readOffset = 0;
        byte[] m_buffer;
    }

    public delegate bool EtCmdHandler(eNetCmd cmd, UsCmd c);

    public enum UsCmdExecResult
    {
        Succ,
        Failed,
        HandlerNotFound,
    }

    public class UsCmdParsing
    {
        public void RegisterHandler(eNetCmd cmd, EtCmdHandler handler)
        {
            m_handlers[cmd] = handler;
        }

        public UsCmdExecResult Execute(UsCmd c)
        {
            try
            {
                eNetCmd cmd = c.ReadNetCmd();
                EtCmdHandler handler;
                if (!m_handlers.TryGetValue(cmd, out handler))
                {
                    return UsCmdExecResult.HandlerNotFound;
                }

                if (handler(cmd, c))
                {
                    return UsCmdExecResult.Succ;
                }
                else
                {
                    return UsCmdExecResult.Failed;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[cmd] Execution failed. ({0})", ex.Message);
                return UsCmdExecResult.Failed;
            }
        }

        Dictionary<eNetCmd, EtCmdHandler> m_handlers = new Dictionary<eNetCmd, EtCmdHandler>();
    }
}
