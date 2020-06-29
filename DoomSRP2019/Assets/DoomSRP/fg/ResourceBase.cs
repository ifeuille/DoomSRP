using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoomSRP.FG
{
    public class ResourceBase:IDisposable
    {
        protected uint id;
        protected string name;
        protected RenderTaskBase creator;
        protected List<RenderTaskBase> readers;
        protected List<RenderTaskBase> writers;
        protected uint refCount;

        protected virtual void realize() { }
        protected virtual void derealize() { }

        public uint Id { get { return this.id; } }
        public string Name { get { return name; } set { name = value; } }
        public bool Transient() { return creator != null; }
        public ResourceBase(string name, RenderTaskBase creator)
        {
            this.name = name;
            this.creator = creator;
            this.refCount = 0;
            this.id = ID.GenerateID();
        }

        public void Dispose()
        {
            ID.returnID(this.id);
        }

    }
}
