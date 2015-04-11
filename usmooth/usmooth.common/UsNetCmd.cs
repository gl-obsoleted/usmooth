using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace usmooth.common
{
    public enum eNetCmd
    {
        None,

        CL_CmdBegin,
        CL_Handshake,
        CL_KeepAlive,
        CL_ExecCommand,
        CL_CmdEnd,

        SV_CmdBegin,
        SV_HandshakeResponse,
        SV_KeepAliveResponse,
        SV_ExecCommandResponse,
        SV_FrameData,
        SV_CmdEnd,
    }
}
