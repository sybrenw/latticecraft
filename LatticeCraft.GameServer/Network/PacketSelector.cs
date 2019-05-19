using Lattice.Core;
using Lattice.Core.Network;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace LatticeCraft.GameServer.Network
{
    public class PacketSelector
    {
        private static ILogger Logger { get; } = AppLogging.CreateLogger<NetworkListener>();

        private Dictionary<int, Func<ReceivablePacket>> _loginPackets;
        private Dictionary<int, Func<ReceivablePacket>> _statusPackets;
        private Dictionary<int, Func<ReceivablePacket>> _playPackets;

        public PacketSelector()
        {
            _statusPackets = SearchPackets("LatticeCraft.GameServer.Network.ClientPackets.Status");
            _loginPackets = SearchPackets("LatticeCraft.GameServer.Network.ClientPackets.Login");
            _playPackets = SearchPackets("LatticeCraft.GameServer.Network.ClientPackets.Play");
        }
               
        private Dictionary<int, Func<ReceivablePacket>> SearchPackets(string @namespace)
        {
            var packets = new Dictionary<int, Func<ReceivablePacket>>(); 

            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(t => t.GetTypes())
                .Where(t => t.IsClass && t.Namespace == @namespace);

            foreach (var type in types)
            {
                var field = type.GetProperty("PacketId");

                if (field == null)
                    continue;

                var packetId = field.GetValue(null, null);
                     
                if (packetId is int)
                {
                    Logger.LogInformation("Found packet 0x{0:X2} - {1}", packetId, type.Name);
                    var creator = Expression.Lambda<Func<ReceivablePacket>>(Expression.New(type.GetConstructor(Type.EmptyTypes))).Compile();
                    packets[(int)packetId] = creator;
                }
            }

            return packets;
        }


        public ReceivablePacket SelectStatusPacket(int packetId)
        {
            if (!_statusPackets.TryGetValue(packetId, out var creator))
            {
                Logger.LogWarning("Unknown packet 0x{0:X2}", packetId);
                return null;
            }

            return creator();
        }

        public ReceivablePacket SelectLoginPacket(int packetId)
        {
            if (!_loginPackets.TryGetValue(packetId, out var creator))
            {
                Logger.LogWarning("Unknown packet 0x{0:X2}", packetId);
                return null;
            }

            return creator();
        }

        public ReceivablePacket SelectPlayPacket(int packetId)
        {
            if (!_playPackets.TryGetValue(packetId, out var creator))
            {
                Logger.LogWarning("Unknown packet 0x{0:X2}", packetId);
                return null;
            }

            return creator();
        }
    }
}
