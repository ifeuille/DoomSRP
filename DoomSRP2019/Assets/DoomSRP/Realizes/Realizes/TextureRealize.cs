using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DoomSRP.FG
{
    public partial class Realize :
        IRealize<DES.Texture2DDescriptor, Texture2D>
    {
        public void Derealize(ref Texture2D actual, DES.Texture2DDescriptor des)
        {
            RuntimeUtilities.Destroy(actual);
            actual = null;
        }

        Texture2D IRealize<DES.Texture2DDescriptor, Texture2D>.RealizeDes(DES.Texture2DDescriptor description)
        {
            var tex = new Texture2D(description.width, description.height, description.textureFormat, description.mipCount, description.linear);
            tex.name = description.name;
            return tex;
        }
    }

    public partial class Realize :
        IRealize<DES.Texture2DArrayDescriptior, Texture2DArray>
    {
        public void Derealize(ref Texture2DArray actual, DES.Texture2DArrayDescriptior des)
        {
            RuntimeUtilities.Destroy(actual);
            actual = null;
        }

        Texture2DArray IRealize<DES.Texture2DArrayDescriptior, Texture2DArray>.RealizeDes(DES.Texture2DArrayDescriptior description)
        {
            var tex = new Texture2DArray(description.width, description.height, description.depth, description.textureFormat, description.mipCount, description.linear);
            tex.name = description.name;
            return tex;
        }
    }

    public partial class Realize :
        IRealize<DES.Texture3DDescriptior, Texture3D>
    {
        public void Derealize(ref Texture3D actual, DES.Texture3DDescriptior des)
        {
            RuntimeUtilities.Destroy(actual);
            actual = null;
        }

        Texture3D IRealize<DES.Texture3DDescriptior, Texture3D>.RealizeDes(DES.Texture3DDescriptior description)
        {
            var tex = new Texture3D(description.width, description.height, description.depth, description.textureFormat, description.mipCount);
            tex.name = description.name;
            return tex;
        }
    }

    public partial class Realize :
    IRealize<DES.CubemapDescriptor, Cubemap>
    {
        public void Derealize(ref Cubemap actual, DES.CubemapDescriptor des)
        {
            RuntimeUtilities.Destroy(actual);
            actual = null;
        }

        Cubemap IRealize<DES.CubemapDescriptor, Cubemap>.RealizeDes(DES.CubemapDescriptor description)
        {
            var tex = new Cubemap(description.width, description.textureFormat, description.mipCount);
            tex.name = description.name;
            return tex;
        }
    }
}
