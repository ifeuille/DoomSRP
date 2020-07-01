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
        protected List<RenderTaskBase> readers = new List<RenderTaskBase>();
        protected List<RenderTaskBase> writers = new List<RenderTaskBase>();
        protected uint refCount = 0;

        public RenderTaskBase Creator { get { return creator; } }
        public List<RenderTaskBase> Readers { get { return readers; } }
        public List<RenderTaskBase> Writers { get { return writers; } }
        public uint ReferenceCount { get { return refCount; } set { refCount = value; } }

        public virtual void realize() { }
        public virtual void derealize() { }

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
        public void UpdateReferenceCount()
        {
            refCount = (uint)(readers.Count);
        }


    }
}
