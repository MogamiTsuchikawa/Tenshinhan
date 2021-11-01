using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tenshinhan.DataClass
{
    class ErgSave: Erg
    {
        DateTime lastUpdateTime { get; set; }
        string fileId { get; set; }
    }
}
