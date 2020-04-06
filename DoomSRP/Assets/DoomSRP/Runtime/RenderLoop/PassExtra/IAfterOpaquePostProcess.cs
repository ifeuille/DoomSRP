using UnityEngine;

namespace DoomSRP
{
    public interface IAfterOpaquePostProcess
    {
        ScriptableRenderPass GetPassToEnqueue(RenderTextureDescriptor baseDescriptor, RenderTargetHandle colorHandle,
            RenderTargetHandle depthHandle);
    }
}
