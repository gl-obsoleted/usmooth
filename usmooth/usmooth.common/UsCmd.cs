using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace usmooth.common
{
    public class UsCmd
    {
        public const int DefaultCmdSize = 1024;

        public UsCmd()
        {
            m_buffer = new byte[DefaultCmdSize];
        }

        public UsCmd(byte[] given)
        {
            m_buffer = given;
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

            float cmd = BitConverter.ToSingle(m_buffer, m_readOffset);
            m_readOffset += sizeof(float);
            return cmd;
        }

        public string ReadString()
        {
            short strLen = ReadInt16();
            if (m_readOffset + (int)strLen > m_buffer.Length)
            {
                throw new System.Exception("无法读取，已到达消息末尾。");
            }

            string ret = Encoding.Default.GetString(m_buffer, m_readOffset, (int)strLen);
            if (ret[ret.Length - 1] == 0)
            {
                char[] cStrTail = { '\0' };
                ret = ret.TrimEnd(cStrTail);
            }

            m_readOffset += (int)strLen;
            return ret;
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

        public void WriteString(string value)
        {
            short len = (short)value.Length;
            len += 1; // 为了和 EtCmdPacket::WriteCStr() 保持一致

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


        int m_writeOffset = 0;
        int m_readOffset = 0;
        byte[] m_buffer;
    }
}
