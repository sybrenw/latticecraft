using Lattice.Core.Network;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LatticeCraft.GameServer.Network.ServerPackets.Login
{
    public class S_0x00_Disconnect : SendablePacket
    {
        public override int PacketId { get { return 0x00; } }

        public string Reason { get; }

        public S_0x00_Disconnect(string reason)
        {
            Reason = reason;
        }

        protected override void Write()
        {
            dynamic obj = new JObject();
            obj.text = Reason;
            obj.bold = true;

            var str = obj.ToString();
            WriteString(str);
        }

    }

}
