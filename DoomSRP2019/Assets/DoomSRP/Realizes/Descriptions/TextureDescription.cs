using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DoomSRP.FG.DES
{

    public struct Texture2DDescription
    {
        public TextureFormat textureFormat;
        public int width;
        public int height;
        public bool linear;
        public int mipCount;

        public Texture2DDescription(TextureFormat format, int w, int h, bool linear, int mipCount)
        {
            this.textureFormat = format;
            this.width = w;
            this.height = h;
            this.linear = linear;
            this.mipCount = mipCount;
        }
    }

    public struct Texture3DDescription
    {
        public TextureFormat textureFormat;
        public int width;
        public int height;
        public int depth;
        public int mipCount;

        public Texture3DDescription(TextureFormat format, int w, int h, int d)
        {
            this.textureFormat = format;
            this.width = w;
            this.height = h;
            this.depth = d;
        }
    }
    public struct CubemapDescription
    {
        public TextureFormat textureFormat;
        public Vector3Int dimension;
        public int mipCount;
    }
}
