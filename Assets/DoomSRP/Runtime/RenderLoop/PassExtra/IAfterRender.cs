using UnityEngine;

namespace DoomSRP
{
    public interface IAfterRender
    {
        ScriptableRenderPass GetPassToEnqueue();
    }
}
