using System;
using System.Collections.Generic;
using UnityEngine;

namespace DoomSRP
{
    //todo use property to find the shader
    public class DoomSRPResources : ScriptableObject
    {
        [SerializeField]
        Material litMat;

        public Material litMaterial { get { return litMat; } }


    }
}
