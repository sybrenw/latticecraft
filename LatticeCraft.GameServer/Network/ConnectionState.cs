using System;
using System.Collections.Generic;
using System.Text;

namespace LatticeCraft.GameServer.Network
{
    internal enum ConnectionState
    {
        Unknown,
        Status,
        Unauthenticated,
        Authenticated
    }
}
