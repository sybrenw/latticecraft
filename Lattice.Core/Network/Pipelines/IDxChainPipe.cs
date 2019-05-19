using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;

namespace Lattice.Core.Network.Pipelines
{
    interface IDxChainPipe : IDxPipe
    {
        IDuplexPipe Source { get; }

        void Connect(IDuplexPipe source);
    }
}
