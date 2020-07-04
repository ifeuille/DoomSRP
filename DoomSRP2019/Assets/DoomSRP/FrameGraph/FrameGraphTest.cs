using DoomSRP.FG.glr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using buffer_resource = DoomSRP.FG.Resource<DoomSRP.FG.glr.buffer_description, DoomSRP.FG.gl.buffer>;
using texture_1d_resource = DoomSRP.FG.Resource<DoomSRP.FG.glr.texture_description, DoomSRP.FG.gl.texture_1d>;
using texture_2d_resource = DoomSRP.FG.Resource<DoomSRP.FG.glr.texture_description, DoomSRP.FG.gl.texture_2d>;
using texture_3d_resource = DoomSRP.FG.Resource<DoomSRP.FG.glr.texture_description, DoomSRP.FG.gl.texture_3d>;
using UnityEngine.Rendering;


namespace DoomSRP.FG
{
    namespace gl
    {
        public class glbase { public uint value; }
        public class buffer : glbase { }
        public class texture_1d : glbase { }
        public class texture_2d : glbase { }
        public class texture_3d : glbase { }
    }
    namespace glr
    {

        public struct buffer_description
        {
            public uint size;
        }
        public struct texture_description
        {
            public uint levels;
            public int formats;
            public Vector3Int size;
        }

    }
    [ExecuteAlways]
    public class FrameGraphTest:MonoBehaviour
    {
        class render_task_1_data
        {
            public texture_2d_resource output1;
            public texture_2d_resource output2;
            public texture_2d_resource output3;
            public texture_2d_resource output4;
        }
        class render_task_2_data
        {
            public texture_2d_resource input1;
            public texture_2d_resource input2;
            public texture_2d_resource output1;
            public texture_2d_resource output2;
        }
        class render_task_3_data
        {
            public texture_2d_resource input1;
            public texture_2d_resource input2;
            public texture_2d_resource output;
        }
        void Test()
        {
            FrameGraph framegraph = new FrameGraph();
            var retainedResource = framegraph.AddRetainedResource<glr.texture_description, gl.texture_2d>("Retained Resource 1", new glr.texture_description(), null);
            var render_task_1 = framegraph.AddRenderTask<render_task_1_data>(
                "Render Task 1",
                (render_task_1_data data,RenderTaskBuilder builder) => 
                {
                    data.output1 = builder.Create< glr.texture_description, gl.texture_2d>("Resource 1", new glr.texture_description());
                    data.output2 = builder.Create<glr.texture_description, gl.texture_2d>("Resource 2", new glr.texture_description());
                    data.output3 = builder.Create<glr.texture_description, gl.texture_2d>("Resource 3", new glr.texture_description());
                    data.output4 = builder.Create<glr.texture_description, gl.texture_2d>("Resource 4", new glr.texture_description());
                },
                (render_task_1_data data) => 
                {
                    var actual1 = data.output1.Actual;
                    var actual2 = data.output2.Actual;
                    var actual3 = data.output3.Actual;
                    var actual4 = data.output4.Actual;
                }
            );
            var data_1 = render_task_1.Data;
            //Debug.Assert(data_1.output1.Id == 1);
            //Debug.Assert(data_1.output2.Id == 2);
            //Debug.Assert(data_1.output3.Id == 3);

            var render_task_2 = framegraph.AddRenderTask<render_task_2_data>(
                "Render Task 2",
                (render_task_2_data data, RenderTaskBuilder builder) =>
                {
                    data.input1 = builder.Read(data_1.output1);// how to share data cross render task?
                    data.input2 = builder.Read(data_1.output2);
                    data.output1 = builder.Write(data_1.output3);
                    data.output2 = builder.Create<glr.texture_description, gl.texture_2d>("Resource 4", new glr.texture_description());
                },
                (render_task_2_data data) =>
                {
                    var actual1 = data.input1.Actual;
                    var actual2 = data.input2.Actual;
                    var actual3 = data.output1.Actual;
                    var actual4 = data.output2.Actual;
                }
            );
            var data_2 = render_task_2.Data;
            //Debug.Assert(data_2.output2.Id == 4);
            var render_task_3 = framegraph.AddRenderTask<render_task_3_data>(
                "Render Task 3",
                (render_task_3_data data, RenderTaskBuilder builder) =>
                {
                    data.input1 = builder.Read(data_2.output1);
                    data.input2 = builder.Read(data_2.output2);
                    data.output = builder.Write(retainedResource);                   
                },
                (render_task_3_data data) =>
                {
                    var actual1 = data.input1.Actual;
                    var actual2 = data.input2.Actual;
                    var actual3 = data.output.Actual;
                }
            );
            framegraph.Compile();
            for(int i = 0; i < 100; ++i)
            {
                framegraph.Execute();
            }
            framegraph.ExportGraphviz("framegraph.gv");
            framegraph.Clear();

        }

        public bool test = false;
        private void Update()
        {
            if(test)
            {
                Test();
                test = false;
            }
        }

        private void Unused()
        {
             
        }
    }
}

namespace DoomSRP.FG
{
    //https://stackoverflow.com/questions/600978/how-to-do-template-specialization-in-c-sharp
    public partial class Realize :
        IRealize<glr.buffer_description, gl.buffer>,
        IRealize<glr.texture_description, gl.texture_2d>
    {
        gl.buffer IRealize<glr.buffer_description, gl.buffer>.RealizeDes(glr.buffer_description description)
        {
            return new gl.buffer { value = description.size };
        }

        gl.texture_2d IRealize<glr.texture_description, gl.texture_2d>.RealizeDes(glr.texture_description description)
        {
            return new gl.texture_2d { value = description.levels };
        }

    }
}

