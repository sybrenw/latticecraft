using System;
using System.Collections.Generic;
using System.Text;

namespace LatticeCraft.GameServer.Entity.Player
{
    class Player : Entity
    {
        public Guid Guid { get; }

        public Player(Guid guid)
        {
            Guid = guid;
        }

    }
}
