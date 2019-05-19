using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lattice.Core.Network
{
    public abstract class ReceivablePacket
    {
        public NetworkConnection Client { get; set; }

        public int Length { get; set; }

        public abstract void Read(ref SequenceReader<byte> reader);

        public virtual Task ProcessAsync()
        {
            return Task.CompletedTask;
        }
    }
}
