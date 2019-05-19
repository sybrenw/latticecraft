using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lattice.Core.Network
{
    internal class ConnectionState
    {
        internal IConnection Connection { get; }
        internal Task Task { get; set; }

        internal ConnectionState(IConnection connection)
        {
            Connection = connection;
        }

    }
}
