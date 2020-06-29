using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoomSRP.FG
{
    // The interface between the framegraph and a render task.
    public class RenderTaskBuilder
    {
        protected FrameGraph framegraph;
        protected RenderTaskBase renderTask;

        ResourceType create<ResourceType, DescriptionType>(string name, DescriptionType description) where ResourceType : class
        {
            //TODO
            return null;
        }

        ResourceType read<ResourceType>(ResourceType resource)
        {
            //TODO
            return resource;
        }

        ResourceType write<ResourceType>(ResourceType resource)
        {
            //TODO
            return resource;
        }
    }
}
