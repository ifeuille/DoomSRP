using UnityEngine;

namespace DoomSRP
{
    public interface IBeforeRender
    {
        ScriptableRenderPass GetPassToEnqueue(RenderTextureDescriptor baseDescriptor,
            RenderTargetHandle colorHandle, RenderTargetHandle depthHandle, ClearFlag clearFlag);
    }
}
