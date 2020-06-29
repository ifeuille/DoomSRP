using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoomSRP.FG
{
    public class ResourceBase
    {
        protected UInt32 id;
        protected string name;
        protected RenderTaskBase creator;
        protected List<RenderTaskBase> readers;
        protected List<RenderTaskBase> writers;
        protected UInt32 refCount;



    }
}
