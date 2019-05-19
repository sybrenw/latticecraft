using System;
using System.Collections.Generic;
using System.Text;

namespace LatticeCraft.GameServer.World
{
    class Chunk
    {
        public int X { get; set; }
        public int Z { get; set; }

        public int LayerMask { get; set; }

        public List<ChunkLayer> Layers { get; } 

        public Chunk()
        {
            Layers = new List<ChunkLayer>();
        }

    }

    class ChunkLayer
    {
        public byte BitsPerBlock { get; } = 14;



        public ChunkLayer()
        {
            
        }
    }

    class DummyChunk : Chunk
    {
        public DummyChunk()
        {
            LayerMask = 1;


        }
    }
}
