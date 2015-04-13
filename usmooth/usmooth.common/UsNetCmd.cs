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
        SV_CmdEnd,
    }
}
