using Lattice.Core.Network;
using LatticeCraft.GameServer.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LatticeCraft.GameServer.Network.ServerPackets.Play
{
    class S_0x22_ChunkData : SendablePacket
    {
        public override int PacketId { get { return 0x22; } }

        public Chunk Chunk { get; }

        public S_0x22_ChunkData(Chunk chunk)
        {
            Chunk = chunk;
        }

        protected override void Write()
        {
            int bitsPerBlock = 4;
            int dataLength = 4096 * bitsPerBlock / 64;

            WriteInt(Chunk.X);
            WriteInt(Chunk.Z);

            // Full chunk?
            WriteByte(1);

            // Layer bitmask
            WriteVarInt(1);
            
            // Layer data size
            WriteVarInt(dataLength * 8 + 2048 * 2 + 256 * 4 + 3 + 5);

            // Layer ---------------


            // Bits per block
            WriteByte((byte)bitsPerBlock);

            // Simple pallette
            WriteVarInt(4);
            WriteVarInt(0);
            WriteVarInt(0x21);
            WriteVarInt(0x0A);
            WriteVarInt(0x09);

            WriteVarInt(dataLength);

            // Blocks
            ulong[] data = new ulong[dataLength];

            uint individualValueMask = (uint)((1 << bitsPerBlock) - 1);

            for (int y = 0; y < 16; y++)
            {
                for (int z = 0; z < 16; z++)
                {
                    for (int x = 0; x < 16; x++)
                    {
                        int blockNumber = (((y * 16) + z) * 16) + x;
                        int startLong = (blockNumber * bitsPerBlock) / 64;
                        int startOffset = (blockNumber * bitsPerBlock) % 64;
                        int endLong = ((blockNumber + 1) * bitsPerBlock - 1) / 64;
                        
                        ulong value = 0x3;

                        value &= individualValueMask;

                        data[startLong] |= (value << startOffset);

                        if (startLong != endLong)
                        {
                            data[endLong] = (value >> (64 - startOffset));
                        }
                    }
                }
            }


            foreach (var value in data)
                WriteULong(value);

            // Block light
            for (int i = 0; i < 2048; i++)
                WriteByte(0xFF);

            // Sky light
            for (int i = 0; i < 2048; i++)
                WriteByte(0xFF);

            // Biomes
            for (int i = 0; i < 256; i++)
                WriteInt(127); // Void
            
            // Block entities ----
            WriteVarInt(0);
        }

        private ulong GetBlockState(byte blockId)
        {
            ulong value = 0;
                        
            value |= blockId;

            return value;
        }

    }
}
