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

        public Resource<DescriptionType, ActualType> Create<DescriptionType, ActualType>(string name, DescriptionType description) 
            where ActualType : class, new()
        {
            var res = new Resource<DescriptionType, ActualType>(name, renderTask, description);
            framegraph.Resources.Add(res);
            renderTask.Creates.Add(res);
            return res;
        }

        public ResourceType Read<ResourceType>(ResourceType resource) where ResourceType : ResourceBase
        {
            resource.Readers.Add(renderTask);
            renderTask.Reads.Add(resource);
            return resource;
        }

        public ResourceType Write<ResourceType>(ResourceType resource) where ResourceType : ResourceBase
        {
            resource.Writers.Add(renderTask);
            renderTask.Writes.Add(resource);
            return resource;
        }
    }
}
