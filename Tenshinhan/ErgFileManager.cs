using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicrosoftGraph;

namespace Tenshinhan
{
    public class ErgFileManager
    {
        private MicrosoftGraph.MicrosoftGraph graph;
        private bool isReady = false;
        public ErgFileManager(MicrosoftGraph.MicrosoftGraph graph)
        {
            this.graph = graph;

        }

    }
}
