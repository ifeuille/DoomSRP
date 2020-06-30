using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//https://github.com/acdemiralp/fg.git
namespace DoomSRP.FG
{
    public partial class FrameGraph
    {
        protected struct Step
        {
            public RenderTaskBase renderTask;
            public List<ResourceBase> relizedResources;
            public List<ResourceBase> derealizedResources;
            public Step(RenderTaskBase rendTask, List<ResourceBase> relRes, List<ResourceBase> derelRes)
            {
                this.renderTask = rendTask;
                this.relizedResources = relRes;
                this.derealizedResources = derelRes;
            }
        }

        protected List<RenderTaskBase> renderTasks = new List<RenderTaskBase>();
        protected List<ResourceBase> resources = new List<ResourceBase>();
        protected List<Step> timeline = new List<Step>();// Computed through framegraph compilation.
                                                         //===============
       public RenderTask<DataType> AddRenderTask<DataType>(string name, Action<DataType, RenderTaskBuilder> setupAc, Action<DataType> exeAc)
        {
            var renderTask = new RenderTask<DataType>(name, setupAc, exeAc);
            renderTasks.Add(renderTask);
            RenderTaskBuilder builder = new RenderTaskBuilder(this, renderTask);
            renderTask.Setup(builder);
            return renderTask;
        }
        Resource<DescriptionType,ActualType> AddRetainedResource<DescriptionType, ActualType>(string name, DescriptionType description, ActualType actual = null) where ActualType : class, new()
        {
            var res = new Resource<DescriptionType, ActualType>(name, description, actual);
            resources.Add(res);
            return res;
        }
        public void Compile()
        {
            //core
            // Reference counting.
            foreach (var renderTask in renderTasks) renderTask.UpdateReferenceCount();
            foreach (var resource in resources) resource.UpdateReferenceCount();

        }
        public void Execute()
        {
            foreach(var step in timeline)
            {
                foreach (var resource in step.relizedResources) resource.realize();
                foreach (var resource in step.derealizedResources) resource.derealize();
            }

        }
        public void Clear()
        {
            renderTasks.Clear();
            resources.Clear();
        }
        public void ExportGraphviz(string filePath)
        {

        }



    }

}
