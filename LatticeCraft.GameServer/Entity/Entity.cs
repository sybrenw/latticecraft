using System;
using System.Collections.Generic;
using System.Text;

namespace LatticeCraft.GameServer.Entity
{
    class Entity
    {
        public int Id { get; set; }

        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public float Yaw { get; set; }
        public float Pitch { get; set; }
        public bool OnGround { get; set; }

    }
}
