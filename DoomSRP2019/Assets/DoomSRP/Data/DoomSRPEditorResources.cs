using System;
using System.Collections.Generic;
using UnityEngine;

namespace DoomSRP
{
    //todo use property to find the shader
    public class DoomSRPEditorResources : ScriptableObject
    {
        [SerializeField]public Shader copyDepthShader;
        [SerializeField] public Shader samplingShader;
        [SerializeField] public Shader blitShader;
        [SerializeField] public Shader screenSpaceShadowShader;
    }
}
