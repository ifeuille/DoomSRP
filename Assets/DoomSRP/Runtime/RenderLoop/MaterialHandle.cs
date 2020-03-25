namespace DoomSRP
{
    public enum MaterialHandle
    {
        Error,
        CopyDepth,
        Sampling,
        Blit,
        ScreenSpaceShadow,

#if UNITY_EDITOR
        ClusterDebug,
#endif
        Count,
    }
}
