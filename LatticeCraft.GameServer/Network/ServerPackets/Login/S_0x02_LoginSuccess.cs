using Lattice.Core.Network;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LatticeCraft.GameServer.Network.ServerPackets.Login
{
    public class S_0x02_LoginSuccess : SendablePacket
    {
        public override int PacketId { get { return 0x02; } }

        public string UUID { get; }
        public string Username { get; }

        public S_0x02_LoginSuccess(string uuid, string username)
        {
            UUID = uuid;
            Username = username;
        }

        protected override void Write()
        {
            WriteString(UUID);
            WriteString(Username);
        }

    }

}
