using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoomSRP
{
    public interface IRendererSetup
    {
        void Setup(ScriptableRenderer renderer, ref RenderingData renderingData);
    }
}
