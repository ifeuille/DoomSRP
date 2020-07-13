using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DoomSRP.FG
{
    public partial class Realize :
        IRealize<DES.TemporaryRenderTextureDescriptor, RenderTexture>
    {
        void IRealize<DES.TemporaryRenderTextureDescriptor, RenderTexture>.Derealize(ref RenderTexture actual, DES.TemporaryRenderTextureDescriptor des)
        {
            if (actual != null)
            {
                actual.Release();
                actual = null;
            }
        }

        RenderTexture IRealize<DES.TemporaryRenderTextureDescriptor, RenderTexture>.RealizeDes(DES.TemporaryRenderTextureDescriptor description)
        {
            var rt = RenderTexture.GetTemporary(description.width, description.height, description.depthBuffer, description.format, description.readWrite, description.antiAliasing, description.memorylessMode, description.vrUsage, description.useDynamicScale);
            rt.name = description.name;
            return rt;
        }
    }

    public partial class Realize :
    IRealize<DES.RenderTextureDescriptor, RenderTexture>
    {
        void IRealize<DES.RenderTextureDescriptor, RenderTexture>.Derealize(ref RenderTexture actual, DES.RenderTextureDescriptor des)
        {
            if (actual != null)
            {
                actual.Release();
                actual = null;
            }
        }

        RenderTexture IRealize<DES.RenderTextureDescriptor, RenderTexture>.RealizeDes(DES.RenderTextureDescriptor description)
        {
            var rt = new RenderTexture(description.EngineDescriptor);
            rt.name = description.name;
            return rt;
        }
    }
}
