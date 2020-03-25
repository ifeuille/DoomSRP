using System;
using UnityEngine.Rendering;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace DoomSRP
{
    /// <summary>
    /// Draw the skybox into the given color buffer using the given depth buffer for depth testing.
    ///
    /// This pass renders the standard Unity skybox.
    /// </summary>
    public class ClusterDebugPass : ScriptableRenderPass
    {

        private RenderTargetHandle colorAttachmentHandle { get; set; }
        private RenderTargetHandle depthAttachmentHandle { get; set; }

        public Material material;

        public ClusterDebugPass()
        {
        }

        /// <summary>
        /// Configure the color and depth passes to use when rendering the skybox
        /// </summary>
        /// <param name="colorHandle">Color buffer to use</param>
        /// <param name="depthHandle">Depth buffer to use</param>
        public void Setup(Material material_, 
            RenderTargetHandle colorAttachmentHandle,
            RenderTargetHandle depthAttachmentHandle)
        {
            this.colorAttachmentHandle = colorAttachmentHandle;
            this.depthAttachmentHandle = depthAttachmentHandle;
            material = material_;
        }

        /// <inheritdoc/>
        public override void Execute(ScriptableRenderer renderer, ScriptableRenderContext context, ref RenderingData renderingData)
        {
            GL.wireframe = true;
            //material.SetPass(0);
            //Graphics.DrawProcedural(MeshTopology.Points, LightDefins.NumClusterX * LightDefins.NumClusterY * LightDefins.NumClusterZ);

            CommandBuffer cmd = CommandBufferPool.Get("Debug Cluster");
            cmd.SetRenderTarget(colorAttachmentHandle.Identifier(), depthAttachmentHandle.Identifier());
            cmd.DrawProcedural(Matrix4x4.identity, material,0, MeshTopology.LineStrip, LightDefins.NumClusterX * LightDefins.NumClusterY * LightDefins.NumClusterZ);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
            GL.wireframe = false;
        }

        public ScriptableRenderPass GetPassToEnqueue()
        {
            return this;
        }
    }
}
