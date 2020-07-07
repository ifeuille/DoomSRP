using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace DoomSRP.FG.DES
{
    public struct RenderTextureDescriptor
    {
        public int width;
        public int height;
        public int volumeDepth;
        public int depthBuffer;
        public RenderTextureFormat format;
        public bool sRGB;
        public bool readWrite;
        public int antiAliasing;
        public RenderTextureMemoryless memorylessMode;
        public GraphicsFormat stencilFormat;
        public VRTextureUsage vrUsage;
        public bool useDynamicScale;
        public bool autoGenMip;
        public bool useMipMap;
        public int mipCount;
        public int msaaSamples;
        public ShadowSamplingMode shadowSamplingMode;
        public string name;
        public RenderTextureDescriptor(int width, int height, int volumeDepth, int depthBuffer,
            RenderTextureFormat format, bool sRGB, bool autoGenMip, bool useMipMap, int mipCount, bool readWrite,
            int antiAliasing, GraphicsFormat stencilFormat, RenderTextureMemoryless memorylessMode, VRTextureUsage vrUsage, bool useDynamicScale, 
            int msaaSamples,
            ShadowSamplingMode shadowSamplingMode,
            string name)
        {
            this.width = width;
            this.height = height;
            this.volumeDepth = volumeDepth;
            this.depthBuffer = depthBuffer;
            this.format = format;
            this.sRGB = sRGB;
            this.autoGenMip = autoGenMip;
            this.useMipMap = useMipMap;
            this.mipCount = mipCount;
            this.readWrite = readWrite;
            this.antiAliasing = antiAliasing;
            this.memorylessMode = memorylessMode;
            this.vrUsage = vrUsage;
            this.useDynamicScale = useDynamicScale;
            this.stencilFormat = stencilFormat;
            this.msaaSamples = msaaSamples;
            this.shadowSamplingMode = shadowSamplingMode;
            this.name = name;
        }
        public UnityEngine.RenderTextureDescriptor EngineDescriptor
        {
            get
            {
                UnityEngine.RenderTextureDescriptor desc = new UnityEngine.RenderTextureDescriptor(width,height, format, depthBuffer,mipCount);
                desc.enableRandomWrite = readWrite;
                desc.autoGenerateMips = autoGenMip;
                desc.sRGB = sRGB;
                desc.useDynamicScale = useDynamicScale;
                desc.vrUsage = vrUsage;
                desc.volumeDepth = volumeDepth;
                desc.stencilFormat = stencilFormat;
                desc.memoryless = memorylessMode;
                desc.useMipMap = useMipMap;
                desc.shadowSamplingMode = shadowSamplingMode;
                return desc;
            }
            
        }

    }
    public struct TemporaryRenderTextureDescriptor
    {
        public int width;
        public int height;
        public int depthBuffer;
        public RenderTextureFormat format;
        public RenderTextureReadWrite readWrite;
        public int antiAliasing;
        public RenderTextureMemoryless memorylessMode;
        public VRTextureUsage vrUsage;
        public bool useDynamicScale;
        public string name;
        public TemporaryRenderTextureDescriptor(int width, int height, int depthBuffer,
            RenderTextureFormat format, RenderTextureReadWrite readWrite, 
            int antiAliasing, RenderTextureMemoryless memorylessMode, VRTextureUsage vrUsage, bool useDynamicScale,
            string name)
        {
            this.width = width;
            this.height = height;
            this.depthBuffer = depthBuffer;
            this.format = format;
            this.readWrite = readWrite;
            this.antiAliasing = antiAliasing;
            this.memorylessMode = memorylessMode;
            this.vrUsage = vrUsage;
            this.useDynamicScale = useDynamicScale;
            this.name = name;
        }

    }
}
