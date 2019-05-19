using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;

namespace Lattice.Core.Network.Pipelines
{
    public interface IDxPipe
    {
        PipeReader Output { get; }
        PipeWriter Input { get; }

        Task RunAsync();

        void ConnectPipe(IDxPipe source);
    }
}
