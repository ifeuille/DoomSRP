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

        public List<ResourceBase> Resources { get { return resources; } }

        protected Stack<ResourceBase> unreferencedResources = new Stack<ResourceBase>();//todo no gc
        //===============
        public RenderTask<DataType> AddRenderTask<DataType>(string name, Action<DataType, RenderTaskBuilder> setupAc, Action<DataType> exeAc)
        {
            var renderTask = new RenderTask<DataType>(name, setupAc, exeAc);
            renderTasks.Add(renderTask);
            RenderTaskBuilder builder = new RenderTaskBuilder(this, renderTask);
            renderTask.Setup(builder);
            return renderTask;
        }
        public Resource<DescriptionType,ActualType> AddRetainedResource<DescriptionType, ActualType>(string name, DescriptionType description, ActualType actual = null) where ActualType : class, new()
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
            // Culling via flood fill from unreferenced resources.
            foreach(var resources in resources)
            {
                if(resources.ReferenceCount == 0 && resources.Transient())
                {
                    unreferencedResources.Push(resources);
                }
            }
            while(unreferencedResources.Count > 0)
            {
                var unreferencedResource = unreferencedResources.Pop();

                var creator = unreferencedResource.Creator;
                if(creator.ReferenceCount > 0)
                {
                    --creator.ReferenceCount;
                }
                if(creator.ReferenceCount == 0 && !creator.CullImmune)
                {
                    foreach(var readResource in creator.Reads)
                    {
                        if(readResource.ReferenceCount > 0)
                        {
                            --readResource.ReferenceCount;
                        }
                        if(readResource.ReferenceCount == 0 && readResource.Transient())
                        {
                            unreferencedResources.Push(readResource);
                        }
                    }
                }

                foreach(var writer in unreferencedResource.Writers)
                {
                    if(writer.ReferenceCount > 0)
                    {
                        --writer.ReferenceCount;
                    }
                    if(writer.ReferenceCount == 0 && !writer.CullImmune)
                    {
                        foreach(var readResource in writer.Reads)
                        {
                            if (readResource.ReferenceCount > 0)
                            {
                                --readResource.ReferenceCount;
                            }
                            if (readResource.ReferenceCount == 0 && readResource.Transient())
                            {
                                unreferencedResources.Push(readResource);
                            }
                        }
                    }
                }
            }
            // Timeline computation.
            timeline.Clear();

            foreach(var renderTask in renderTasks)
            {
                if (renderTask.ReferenceCount == 0 && !renderTask.CullImmune) continue;
                    List<ResourceBase> cacheRealizedResources = new List<ResourceBase>();//todo no gc
                List<ResourceBase> cacheDerealizedResources = new List<ResourceBase>();//todo no gc
                foreach(var resource in renderTask.Creates)
                {
                    cacheRealizedResources.Add(resource);
                    if(resource.Readers.Count == 0 && resource.Writers.Count == 0)
                    {
                        cacheRealizedResources.Add(resource);
                    }
                }
                var reads_writes = renderTask.Reads;
                reads_writes.AddRange(renderTask.Writes);
                foreach(var resource in reads_writes)
                {
                    if (!resource.Transient()) continue;
                    bool valid = false;
                    int last_index = -1;
                    if(resource.Readers.Count != 0)
                    {
                        var lastReads = resource.Readers[resource.Readers.Count - 1];
                        last_index = renderTasks.FindIndex((x)=> { return x == lastReads; });
                        if(last_index >= 0)
                        {
                            valid = true;
                        }
                    }
                    if(resource.Writers.Count != 0)
                    {
                        var lastWrites = resource.Writers[resource.Writers.Count - 1];
                        var index = renderTasks.FindIndex((x) => { return x == lastWrites; });
                        if (index >= 0)
                        {
                            valid = true;
                            last_index = Math.Max(last_index, index);
                        }
                    }
                    if(valid && renderTasks[last_index] == renderTask)
                    {
                        cacheDerealizedResources.Add(resource);
                    }
                }
                timeline.Add(new Step(renderTask, cacheRealizedResources, cacheDerealizedResources) );
            }

            unreferencedResources.Clear();
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
