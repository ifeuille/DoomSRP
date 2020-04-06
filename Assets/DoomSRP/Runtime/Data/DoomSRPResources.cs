using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class DoomSRPResources : ScriptableObject
{
    [FormerlySerializedAs("copyDepthShader"), SerializeField] public Shader copyDepthShader = null;
    [FormerlySerializedAs("samplingShader"), SerializeField] public Shader samplingShader = null;
    [FormerlySerializedAs("blitShader"), SerializeField] public Shader blitShader = null;
    [FormerlySerializedAs("screenSpaceShadowShader"), SerializeField] public Shader screenSpaceShadowShader = null;


}
