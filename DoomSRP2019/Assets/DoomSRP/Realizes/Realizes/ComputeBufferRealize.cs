using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DoomSRP.FG
{
    public partial class Realize :
        IRealize<DES.ComputeBufferDescriptor, ComputeBuffer>
    {
        void IRealize<DES.ComputeBufferDescriptor, ComputeBuffer>.Derealize(ref ComputeBuffer actual, DES.ComputeBufferDescriptor des)
        {
            if (actual != null)
            {
                actual.Release();
                actual = null;
            }
        }

        ComputeBuffer IRealize<DES.ComputeBufferDescriptor, ComputeBuffer>.RealizeDes(DES.ComputeBufferDescriptor description)
        {
            var tex = new ComputeBuffer(description.count, description.stride, description.computeBufferType, description.computeBufferMode);
            tex.name = description.name;
            return tex;
        }
    }
}
