using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoomSRP.FG
{
    public class Resource<DescriptionType, ActualType>:ResourceBase where ActualType : class,new()
    {
        DescriptionType description;
        ActualType actual;

        public DescriptionType Description { get { return description; } }
        public ActualType Actual { get { return actual; } }
        public Resource(string name, RenderTaskBase creator, DescriptionType description) : base(name, creator)
        {
            this.actual = new ActualType();//?
        }
        public Resource(string name, DescriptionType description, ActualType act) : base(name, null)
        {
            this.actual = act;
            if (act == null)
            {
                this.actual = GlobalRealize.RealizeDes<DescriptionType, ActualType>(description);
            }
        }

        public override void realize()
        {
            if(Transient()) this.actual = GlobalRealize.RealizeDes<DescriptionType, ActualType>(description);
        }
        public override void derealize()
        {
            if (Transient()) this.actual = null;
        }
    }
}
