using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lattice.Core.Network.Pipelines
{
    public class DxPipeLine : IDxPipe
    {
        public PipeWriter Input => _outputPipe.Input;
        public PipeReader Output => _outputPipe.Output;

        private IDxPipe _outputPipe;

        private List<IDxPipe> _pipes;

        public DxPipeLine(params IDxPipe[] pipes)
        {
            _pipes = pipes.ToList();

            IDxPipe p = null;

            foreach (var pipe in pipes)
            {
                if (p != null)
                    pipe.ConnectPipe(p);

                p = pipe;
            }

            _outputPipe = p;
        }
        
        public async Task RunAsync()
        {
            List<Task> tasks = new List<Task>();
            foreach (var pipe in _pipes)
                tasks.Add(pipe.RunAsync());

            await Task.WhenAll(tasks);
        }

        public void ConnectPipe(IDxPipe source)
        {
            throw new NotImplementedException();
        }
    }
}
