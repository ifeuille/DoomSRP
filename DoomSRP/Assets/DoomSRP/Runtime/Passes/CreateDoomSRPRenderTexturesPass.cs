using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace DoomSRP
{
    class CreateDoomSRPRenderTexturesPass : ScriptableRenderPass
    {
        const string k_CreateRenderTexturesTag = "Create Render Textures";
        const int k_DepthStencilBufferBits = 32;
        private RenderTargetHandle colorAttachmentHandle { get; set; }
        private RenderTargetHandle depthAttachmentHandle { get; set; }
        private RenderTextureDescriptor descriptor { get; set; }
        private SampleCount samples { get; set; }

        /// <summary>
        /// Configure the pass
        /// </summary>
        public void Setup(
            RenderTextureDescriptor baseDescriptor,
            RenderTargetHandle colorAttachmentHandle,
            RenderTargetHandle depthAttachmentHandle,
            SampleCount samples)
        {
            this.colorAttachmentHandle = colorAttachmentHandle;
            this.depthAttachmentHandle = depthAttachmentHandle;
            this.samples = samples;
            descriptor = baseDescriptor;
        }

        /// <inheritdoc/>
        public override void Execute(ScriptableRenderer renderer, ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (renderer == null)
                throw new ArgumentNullException("renderer");

            CommandBuffer cmd = CommandBufferPool.Get(k_CreateRenderTexturesTag);
            if (colorAttachmentHandle != RenderTargetHandle.CameraTarget)
            {
                var colorDescriptor = descriptor;
                colorDescriptor.depthBufferBits = 0;
                colorDescriptor.sRGB = true;
                colorDescriptor.msaaSamples = (int)samples;
                cmd.GetTemporaryRT(colorAttachmentHandle.id, colorDescriptor, FilterMode.Bilinear);
            }

            if (depthAttachmentHandle != RenderTargetHandle.CameraTarget)
            {
                var depthDescriptor = descriptor;
                depthDescriptor.colorFormat = RenderTextureFormat.Depth;
                depthDescriptor.depthBufferBits = k_DepthStencilBufferBits;
                depthDescriptor.msaaSamples = (int)samples;
                depthDescriptor.bindMS = (int)samples > 1 && !SystemInfo.supportsMultisampleAutoResolve && (SystemInfo.supportsMultisampledTextures != 0);
                cmd.GetTemporaryRT(depthAttachmentHandle.id, depthDescriptor, FilterMode.Point);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        /// <inheritdoc/>
        public override void FrameCleanup(CommandBuffer cmd)
        {
            if (cmd == null)
                throw new ArgumentNullException("cmd");

            if (colorAttachmentHandle != RenderTargetHandle.CameraTarget)
            {
                cmd.ReleaseTemporaryRT(colorAttachmentHandle.id);
                colorAttachmentHandle = RenderTargetHandle.CameraTarget;
            }

            if (depthAttachmentHandle != RenderTargetHandle.CameraTarget)
            {
                cmd.ReleaseTemporaryRT(depthAttachmentHandle.id);
                depthAttachmentHandle = RenderTargetHandle.CameraTarget;
            }
        }
    }
}
