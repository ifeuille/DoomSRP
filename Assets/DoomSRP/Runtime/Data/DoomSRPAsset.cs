using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
#endif

namespace DoomSRP
{
    public enum SampleCount
    {
        One = 1,
        Two = 2,
        Four = 4,
    }

    public enum Downsampling
    {
        None,
        _2xBilinear,
        _4xBox,
        _4xBilinear
    }

    public class DoomSRPAsset : RenderPipelineAsset, ISerializationCallbackReceiver
    {

        [SerializeField] DoomSRPResources m_ResourcesAsset = null;

        public DoomSRPResources resources
        {
            get
            {
#if UNITY_EDITOR
                if (m_ResourcesAsset == null)
                    m_ResourcesAsset = LoadResourceFile<DoomSRPResources>();
#endif
                return m_ResourcesAsset;
            }
        }
  
#if UNITY_EDITOR
        [NonSerialized]
        DoomSRPEditorResources m_EditorResourcesAsset;

        static readonly string s_SearchPathProject = "Assets/DoomSRP";
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812")]
        internal class CreateDoomSRPAsset : EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                var instance = CreateInstance<DoomSRPAsset>();
                instance.m_EditorResourcesAsset = LoadResourceFile<DoomSRPEditorResources>();
                instance.m_ResourcesAsset = LoadResourceFile<DoomSRPResources>();
                AssetDatabase.CreateAsset(instance, pathName);
            }
        }

        [MenuItem("DoomSRP/Assert/Rendering/Pipeline Asset")]
        static void CreateDoomSRPPipeline()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, CreateInstance<CreateDoomSRPAsset>(),
                "DoomSRPAsset.asset", null, null);
        }

        [MenuItem("DoomSRP/Assert/Rendering/Pipeline Resource")]
        static void CreateDoomSRPResources()
        {
            var instance = CreateInstance<DoomSRPResources>();
            AssetDatabase.CreateAsset(instance, string.Format("Assets/{0}.asset", typeof(DoomSRPResources).Name));
        }

        [MenuItem("DoomSRP/Assert/Rendering/Pipeline Editor Resources")]
        static void CreateLightweightPipelineEditorResources()
        {
            var instance = CreateInstance<DoomSRPEditorResources>();
            AssetDatabase.CreateAsset(instance, string.Format("Assets/{0}.asset", typeof(DoomSRPEditorResources).Name));
        }

        DoomSRPEditorResources editorResources
        {
            get
            {
                if (m_EditorResourcesAsset == null)
                    m_EditorResourcesAsset = LoadResourceFile<DoomSRPEditorResources>();

                return m_EditorResourcesAsset;
            }
        }

        static T LoadResourceFile<T>() where T : ScriptableObject
        {
            T resourceAsset = null;
            var guids = AssetDatabase.FindAssets(typeof(T).Name + " t:scriptableobject", new[] { s_SearchPathProject });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                resourceAsset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (resourceAsset != null)
                    break;
            }

            return resourceAsset;
        }
#endif

        public void OnAfterDeserialize()
        {
            
        }

        public void OnBeforeSerialize()
        {
            
        }

        protected override IRenderPipeline InternalCreatePipeline()
        {
            return new DoomSRPPipeline(this);
        }
    }

}