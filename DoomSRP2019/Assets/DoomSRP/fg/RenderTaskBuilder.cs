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

        public RenderTaskBuilder(FrameGraph framgGrph, RenderTaskBase rendreTask)
        {
            this.framegraph = framgGrph;
            this.renderTask = rendreTask;
        }

        public ResourceType Create<ResourceType, DescriptionType>(string name, DescriptionType description) where ResourceType : ResourceBase,new()
        {
            var res = new ResourceType();
            framegraph.Resources.Add(res);
            res.Init(name, renderTask, description);
            //TODO
            return null;
        }

        public ResourceType Read<ResourceType>(ResourceType resource)
        {
            //TODO
            return resource;
        }

        public ResourceType Write<ResourceType>(ResourceType resource)
        {
            //TODO
            return resource;
        }
    }
}
