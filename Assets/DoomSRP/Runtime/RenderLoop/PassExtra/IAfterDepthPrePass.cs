using UnityEngine;

namespace DoomSRP
{
    public interface IAfterDepthPrePass
    {
        ScriptableRenderPass GetPassToEnqueue(RenderTextureDescriptor baseDescriptor, RenderTargetHandle depthAttachmentHandle);
    }
}
