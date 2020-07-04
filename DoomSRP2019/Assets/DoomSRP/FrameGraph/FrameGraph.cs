using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        public RenderTask<DataType> AddRenderTask<DataType>(string name, Action<DataType, RenderTaskBuilder> setupAc, Action<DataType> exeAc) where DataType:class,new()
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
                step.renderTask.Execute();
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
            using(FileStream F = new FileStream(filePath, FileMode.OpenOrCreate))
            {
                F.SetLength(0);
                using (StreamWriter stream = new StreamWriter(F))
                {
                    stream.WriteLine("digraph framegraph \n{\n");

                    stream.WriteLine("rankdir = LR\n");
                    stream.WriteLine("bgcolor = black\n\n");
                    stream.WriteLine("node [shape=rectangle, fontname=\"helvetica\", fontsize=12]\n\n");

                    foreach (var  render_task in renderTasks)
                        stream.WriteLine("\"" + render_task.Name + "\" [label=\"" + render_task.Name + "\\nRefs: " + render_task.ReferenceCount + "\", style=filled, fillcolor=darkorange]\n");
                    stream.WriteLine("\n");

                    foreach (var  resource in resources   )
                        stream.WriteLine("\"" + resource.Name + "\" [label=\"" + resource.Name + "\\nRefs: " + resource.ReferenceCount + "\\nID: " + resource.Id + "\", style=filled, fillcolor= " + (resource.Transient() ? "skyblue" : "steelblue") + "]\n");
                    stream.WriteLine("\n");

                    foreach (var  render_task in renderTasks)
                    {
                        stream.WriteLine("\"" + render_task.Name + "\" -> { ");
                        foreach (var  resource in render_task.Creates)
                            stream.WriteLine( "\"" + resource.Name + "\" ");
                        stream.WriteLine("} [color=seagreen]\n");

                        stream.WriteLine("\"" + render_task.Name + "\" -> { ");
                        foreach (var  resource in render_task.Writes)
                            stream.WriteLine("\"" + resource.Name + "\" ");
                        stream.WriteLine("} [color=gold]\n");
                    }
                    stream.WriteLine("\n");

                    foreach (var  resource in resources)
                    {
                        stream.WriteLine("\"" + resource.Name + "\" -> { ");
                        foreach (var  render_task in resource.Readers)
                            stream.WriteLine("\"" + render_task.Name + "\" ");
                        stream.WriteLine("} [color=firebrick]\n");
                    }
                    stream.WriteLine("}");
                }
            }

         

            ////https://blog.csdn.net/yenange/article/details/7940043

            //ProcessStartInfo info = new ProcessStartInfo()
            //{
            //    FileName = "dot.exe",
            //    WorkingDirectory = Path.GetDirectoryName(@"D:\workspace\mine\DoomSRP\DoomSRP2019\Assets"),
            //    Arguments = string.Concat("-Tpng -o ", "abc.pnng", " ", "abc.dot"),
            //    RedirectStandardInput = false,
            //    RedirectStandardOutput = false,
            //    RedirectStandardError = true,
            //    UseShellExecute = false,
            //    CreateNoWindow = true
            //};
            //using (Process exe = Process.Start(info))
            //{
            //    exe.WaitForExit();
            //    if (0 == exe.ExitCode)
            //    {
            //        //System.Web.HttpContext.Current.Response.Write(pngFile);
            //    }
            //    else
            //    {
            //        string errMsg;
            //        using (StreamReader stdErr = exe.StandardError)
            //        {
            //            errMsg = stdErr.ReadToEnd();
            //        }
            //        //System.Web.HttpContext.Current.Response.Write(errMsg);
            //    }
            //}
        }



    }

}
