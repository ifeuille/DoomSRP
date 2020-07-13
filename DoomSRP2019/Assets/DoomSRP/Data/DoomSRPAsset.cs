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
    [CreateAssetMenu(fileName = "doomsrpasset", menuName = "Create/DoomSRP/Asset", order = 1)]
    public class DoomSRPAsset : RenderPipelineAsset, ISerializationCallbackReceiver
    {
        [NonSerialized]
        public static DoomSRPPipeline PipelineInstance = null;

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

        public void OnAfterDeserialize() { }
        public void OnBeforeSerialize() { }
        protected override RenderPipeline CreatePipeline()
        {
            return new DoomSRPPipeline();
        }
    }
}
