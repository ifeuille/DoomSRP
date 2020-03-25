using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Light))]
[CanEditMultipleObjects]
public class ProjectorLightEditor : Editor
{
    override public void OnInspectorGUI()
    {

    }
}

namespace DoomSRP
{
    [CustomEditor(typeof(ProjectorLight))]
    [CanEditMultipleObjects]
    public class ProjectorLightEditor : Editor
    {
        void OnSelectAtlas(UnityEngine.Object obj)
        {
            serializedObject.Update();
            SerializedProperty sp = serializedObject.FindProperty("spritesAtlas");
            sp.objectReferenceValue = obj;
            serializedObject.ApplyModifiedProperties();
            Tools.SetDirty(serializedObject.targetObject);
            IFPipelineSettings.atlas = obj as SpritesAtlas;
        }
        void SelectProjSprite(string spriteName)
        {
            serializedObject.Update();
            SerializedProperty sp = serializedObject.FindProperty("mprojSpriteName");
            sp.stringValue = spriteName;
            //ProjectorLight l = target as ProjectorLight;
            //l.projSpriteData = l.spritesAtlas.GetSprite(spriteName);
            serializedObject.ApplyModifiedProperties();
            Tools.SetDirty(serializedObject.targetObject);
            IFPipelineSettings.selectedSprite = spriteName;
        }
        void SelectFalloffSprite(string spriteName)
        {
            serializedObject.Update();
            SerializedProperty sp = serializedObject.FindProperty("mfalloffSpriteName");
            sp.stringValue = spriteName;
            //ProjectorLight l = target as ProjectorLight;
            //l.falloffSpriteData = l.spritesAtlas.GetSprite(spriteName);
            serializedObject.ApplyModifiedProperties();
            Tools.SetDirty(serializedObject.targetObject);
            IFPipelineSettings.selectedSprite = spriteName;
        }
        void DrawProjector()
        {
            ProjectorLight l = target as ProjectorLight;
            GUILayout.Label("Shape");
            GUILayout.BeginVertical( "box");
            float NearClipPlane = EditorGUILayout.FloatField("Near Clip Plane", l.iFPipelineProjector.nearClipPlane);
            if (NearClipPlane > 0 && NearClipPlane < l.iFPipelineProjector.farClipPlane + 0.01f)
            {
                l.iFPipelineProjector.nearClipPlane = NearClipPlane;
            }

            float FarClipPlane = EditorGUILayout.FloatField("Far Clip Plane", l.iFPipelineProjector.farClipPlane);
            if (FarClipPlane > 0 && FarClipPlane > l.iFPipelineProjector.nearClipPlane + 0.01f)
            {
                l.iFPipelineProjector.farClipPlane = FarClipPlane;
            }

            l.iFPipelineProjector.fieldOfView = EditorGUILayout.FloatField("Field Of View", l.iFPipelineProjector.fieldOfView);
            l.iFPipelineProjector.aspectRatio = EditorGUILayout.FloatField("Aspect Ratio", l.iFPipelineProjector.aspectRatio);

            l.iFPipelineProjector.orthographic = EditorGUILayout.Toggle("Orthographic", l.iFPipelineProjector.orthographic);
            l.iFPipelineProjector.orthoGraphicSize = EditorGUILayout.FloatField("Orthographic Size", l.iFPipelineProjector.orthoGraphicSize);
            
            GUILayout.EndVertical();
        }

        void DrawAtlas()
        {
            EditorGUILayout.Space();
            ProjectorLight l = target as ProjectorLight;
            GUILayout.Label("Sprite");
            GUILayout.BeginVertical( "box");
            GUILayout.BeginHorizontal();
            if (IFPipelineEditorTools.DrawPrefixButton("Atlas"))
                ComponentSelector.Show<SpritesAtlas>(OnSelectAtlas);
            SerializedProperty atlas = IFPipelineEditorTools.DrawProperty("", serializedObject, "spritesAtlas", GUILayout.MinWidth(20f));
            GUILayout.EndHorizontal();

            if (atlas != null)
            {
                SpritesAtlas atl = atlas.objectReferenceValue as SpritesAtlas;
                if (atl != null && atl != l.spritesAtlas)
                {
                    l.spritesAtlas = atl;
                }
                GUILayout.Label("Project Sprite");
                GUILayout.BeginHorizontal();
                SerializedProperty sp = serializedObject.FindProperty("mprojSpriteName");
                IFPipelineEditorTools.DrawAdvancedSpriteField(atlas.objectReferenceValue as SpritesAtlas, sp.stringValue, SelectProjSprite, false);
                GUILayout.EndHorizontal();
                GUILayout.Label("Falloff Sprite");
                GUILayout.BeginHorizontal();
                SerializedProperty spfalloff = serializedObject.FindProperty("mfalloffSpriteName");
                IFPipelineEditorTools.DrawAdvancedSpriteField(atlas.objectReferenceValue as SpritesAtlas, spfalloff.stringValue, SelectFalloffSprite, false);
                GUILayout.EndHorizontal();
            }
            else
            {
                l.mprojSpriteName = "";
                l.mfalloffSpriteName = "";
                l.falloffSpriteData = new SpriteData();
                l.projSpriteData = new SpriteData();
                Tools.SetDirty(serializedObject.targetObject);
            }
            GUILayout.EndVertical();
        }

        void DrawLightProperties()
        {
            EditorGUILayout.Space();
            ProjectorLight l = target as ProjectorLight;
            GUILayout.Label("Properties");
            GUILayout.BeginVertical("box");

            {
                GUILayout.Label("Color");
                GUILayout.BeginVertical("box");
                SerializedProperty intensity = IFPipelineEditorTools.DrawProperty("Intensity", serializedObject, "intensity", GUILayout.MinWidth(20f));
                if (intensity.floatValue != l.Intensity)
                {
                    l.Intensity = intensity.floatValue;
                }
                SerializedProperty lightColor = IFPipelineEditorTools.DrawProperty("Color", serializedObject, "lightColor", GUILayout.MinWidth(20f));
                if (lightColor.colorValue != l.LightColor)
                {
                    l.LightColor = lightColor.colorValue;
                }

                bool useTemperature = EditorGUILayout.Toggle("Use Temperature", l.UseTemperature, GUILayout.MinWidth(20f));
                if (l.UseTemperature)
                {
                    l.Temperature = EditorGUILayout.FloatField("Temperature", l.Temperature, GUILayout.MinWidth(20f));
                }

                if (useTemperature != l.UseTemperature)
                {
                    l.UseTemperature = useTemperature;
                }
                l.specMultiplier = EditorGUILayout.FloatField("Specular Multiplier", l.specMultiplier, GUILayout.MinWidth(20f));
                l.shadowBleedReduce = EditorGUILayout.FloatField("Shadow Bleed Reduce", l.shadowBleedReduce, GUILayout.MinWidth(20f));

                GUILayout.EndVertical();
            }

            {
                GUILayout.Label("Parameters");
                GUILayout.BeginVertical("box");
                l.lightParms_Shadow = EditorGUILayout.Toggle("Shadow", l.lightParms_Shadow, GUILayout.MinWidth(20f));
                l.lightParms_Rect = EditorGUILayout.Toggle("Rect", l.lightParms_Rect, GUILayout.MinWidth(20f));
                l.lightParms_Circle = EditorGUILayout.Toggle("Circle", l.lightParms_Circle, GUILayout.MinWidth(20f));
                l.lightParms_SkipSkipModel = EditorGUILayout.Toggle("Not Skip", l.lightParms_SkipSkipModel, GUILayout.MinWidth(20f));
                //l.lightParams_IsArea = EditorGUILayout.Toggle("Area", l.lightParams_IsArea, GUILayout.MinWidth(20f));
                l.lightParams_NoDiffuse = EditorGUILayout.Toggle("No Diffuse", l.lightParams_NoDiffuse, GUILayout.MinWidth(20f));

                l.area_width = EditorGUILayout.FloatField("Area Width", l.area_width, GUILayout.MinWidth(20f));
                l.area_height = EditorGUILayout.FloatField("Area Height", l.area_height, GUILayout.MinWidth(20f));
                l.area_falloff = EditorGUILayout.FloatField("Area Falloff", l.area_falloff, GUILayout.MinWidth(20f));

                GUILayout.EndVertical();
            }

            GUILayout.EndVertical();
        }

        override public void OnPreviewGUI(Rect rect, GUIStyle background)
        {
            ProjectorLight l = target as ProjectorLight;
            if (l.spritesAtlas == null) return;
            Texture2D tex = l.spritesAtlas.texture as Texture2D;
            if (tex == null) return;
            //GUILayout.Label("Preview");
            //GUILayout.BeginVertical("box");
            SpriteData sd = l.spritesAtlas.GetSprite(l.mprojSpriteName);
            IFPipelineEditorTools.DrawSprite(tex, rect, sd, Color.white);
            //GUILayout.EndVertical();
        }
        override public void OnInspectorGUI()
        {
            DrawProjector();
            DrawAtlas();
            DrawLightProperties();
            OnPreviewGUI(GUILayoutUtility.GetRect(240, 240), EditorStyles.whiteLabel);
            //l.iFPipelineProjector.SetDirty();
            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }
    }
}
