using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoomSRP.FG
{
    public class RenderTask<DataType> : RenderTaskBase where DataType:class,new()
    {
        public delegate void SteupDelegate(DataType dataType, RenderTaskBuilder builder);
        public delegate void ExecuteDelegate(DataType dataType);
        protected DataType data = new DataType();
        protected Action<DataType, RenderTaskBuilder> setupAction;
        protected Action<DataType> executeAction;

        public DataType Data { get { return data; } }

        public RenderTask(string name, Action<DataType, RenderTaskBuilder> setupAc, Action<DataType> exeAc):base(name)
        {
            setupAction = setupAc;
            executeAction = exeAc;
        }


        public override void Setup(RenderTaskBuilder builder)
        {
            setupAction?.Invoke(data, builder);
        }
        public override void Execute()
        {
            executeAction?.Invoke(data);
        }



    }
}
