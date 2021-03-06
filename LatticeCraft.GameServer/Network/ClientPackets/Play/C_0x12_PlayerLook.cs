﻿using Lattice.Core.Network;
using LatticeCraft.GameServer.Buffers;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LatticeCraft.GameServer.Network.ClientPackets.Play
{
    class C_0x12_PlayerLook : ReceivablePacket
    {
        public static int PacketId { get; } = 0x12;
        
        public override void Read(ref SequenceReader<byte> reader)
        {
            var client = Client as MinecraftClient;
            client.Player.Yaw = reader.ReadFloat();
            client.Player.Pitch = reader.ReadFloat();
            client.Player.OnGround = reader.ReadBoolean();
        }

        public override async Task ProcessAsync()
        {

        }
    }
}
