using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.Rendering;
using UnityEditor;

namespace DoomSRP
{
    [CustomEditor(typeof(DoomSRPAssetEditor))]
    public class DoomSRPAssetEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
