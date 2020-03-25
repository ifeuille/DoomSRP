using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


namespace DoomSRP
{
    public class DoomSRPEditorResources : ScriptableObject
    {
        [FormerlySerializedAs("DefaultMaterial"), SerializeField]
        Material m_LitMaterial = null;

        public Material litMaterial
        {
            get { return m_LitMaterial; }
        }
    }

}