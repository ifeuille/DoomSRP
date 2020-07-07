using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DoomSRP.FG.DES
{

    public struct Texture2DDescriptor
    {
        public TextureFormat textureFormat;
        public int width;
        public int height;
        public bool linear;
        public int mipCount;
        public string name;

        public Texture2DDescriptor(TextureFormat format, int w, int h, bool linear, int mipCount, string name)
        {
            this.textureFormat = format;
            this.width = w;
            this.height = h;
            this.linear = linear;
            this.mipCount = mipCount;
            this.name = name;
        }
    }
    public struct Texture2DArrayDescriptior
    {
        public TextureFormat textureFormat;
        public int width;
        public int height;
        public int depth;
        public int mipCount;
        public bool linear;
        public string name;

        public Texture2DArrayDescriptior(TextureFormat format, int w, int h, int d, int mipCount, bool linear, string name)
        {
            this.textureFormat = format;
            this.width = w;
            this.height = h;
            this.depth = d;
            this.mipCount = mipCount;
            this.linear = linear;
            this.name = name;
        }
    }
    public struct Texture3DDescriptior
    {
        public TextureFormat textureFormat;
        public int width;
        public int height;
        public int depth;
        public int mipCount;
        public string name;

        public Texture3DDescriptior(TextureFormat format, int w, int h, int d, int mipCount,string name)
        {
            this.textureFormat = format;
            this.width = w;
            this.height = h;
            this.depth = d;
            this.mipCount = mipCount;
            this.name = name;
        }
    }
    public struct CubemapDescriptor
    {
        public TextureFormat textureFormat;
        public int width;
        public int mipCount;
        public string name;

        public CubemapDescriptor(TextureFormat format, int w, int mipCount, string name)
        {
            this.textureFormat = format;
            this.width = w;
            this.mipCount = mipCount;
            this.name = name;
        }
    }
}
