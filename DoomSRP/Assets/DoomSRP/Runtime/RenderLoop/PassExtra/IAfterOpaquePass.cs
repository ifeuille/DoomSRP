using UnityEngine;

namespace DoomSRP
{
    public interface IAfterOpaquePass
    {
        ScriptableRenderPass GetPassToEnqueue(
            RenderTextureDescriptor baseDescriptor,
            RenderTargetHandle colorAttachmentHandle,
            RenderTargetHandle depthAttachmentHandle);
    }
}
