using Lattice.Core.Network;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LatticeCraft.GameServer.Network.ServerPackets.Status
{
    public class S_0x00_StatusResponse : SendablePacket
    {
        public override int PacketId { get { return 0x00; } }

        public S_0x00_StatusResponse()
        {

        }

        protected override void Write()
        {
            dynamic obj = new JObject();
            obj.version = new JObject();
            obj.version.name = "1.13.2";
            obj.version.protocol = 404;
            obj.players = new JObject();
            obj.players.max = 9999999999;
            obj.players.online = 786786585;
            obj.players.sample = new JArray();
            obj.description = new JObject();
            obj.description.text = "Lattice craft test server";

            var str = obj.ToString();
            WriteString(str);
        }

    }

}
