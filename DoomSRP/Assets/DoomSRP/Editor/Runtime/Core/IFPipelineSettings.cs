//----------------------------------------------
//            IFPipeline: Next-Gen UI kit
// Copyright © 2011-2015 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Unity doesn't keep the values of static variables after scripts change get recompiled. One way around this
/// is to store the references in EditorPrefs -- retrieve them at start, and save them whenever something changes.
/// </summary>

namespace DoomSRP
{
    public class IFPipelineSettings
    {
        public enum ColorMode
        {
            Orange,
            Green,
            Blue,
        }

        #region Generic Get and Set methods
        /// <summary>
        /// Save the specified boolean value in settings.
        /// </summary>

        static public void SetBool(string name, bool val) { EditorPrefs.SetBool(name, val); }

        /// <summary>
        /// Save the specified integer value in settings.
        /// </summary>

        static public void SetInt(string name, int val) { EditorPrefs.SetInt(name, val); }

        /// <summary>
        /// Save the specified float value in settings.
        /// </summary>

        static public void SetFloat(string name, float val) { EditorPrefs.SetFloat(name, val); }

        /// <summary>
        /// Save the specified string value in settings.
        /// </summary>

        static public void SetString(string name, string val) { EditorPrefs.SetString(name, val); }

        /// <summary>
        /// Save the specified color value in settings.
        /// </summary>

        static public void SetColor(string name, Color c) { SetString(name, c.r + " " + c.g + " " + c.b + " " + c.a); }

        /// <summary>
        /// Save the specified enum value to settings.
        /// </summary>

        static public void SetEnum(string name, System.Enum val) { SetString(name, val.ToString()); }

        /// <summary>
        /// Save the specified object in settings.
        /// </summary>

        static public void Set(string name, Object obj)
        {
            if (obj == null)
            {
                EditorPrefs.DeleteKey(name);
            }
            else
            {
                if (obj != null)
                {
                    string path = AssetDatabase.GetAssetPath(obj);

                    if (!string.IsNullOrEmpty(path))
                    {
                        EditorPrefs.SetString(name, path);
                    }
                    else
                    {
                        EditorPrefs.SetString(name, obj.GetInstanceID().ToString());
                    }
                }
                else EditorPrefs.DeleteKey(name);
            }
        }

        /// <summary>
        /// Get the previously saved boolean value.
        /// </summary>

        static public bool GetBool(string name, bool defaultValue) { return EditorPrefs.GetBool(name, defaultValue); }

        /// <summary>
        /// Get the previously saved integer value.
        /// </summary>

        static public int GetInt(string name, int defaultValue) { return EditorPrefs.GetInt(name, defaultValue); }

        /// <summary>
        /// Get the previously saved float value.
        /// </summary>

        static public float GetFloat(string name, float defaultValue) { return EditorPrefs.GetFloat(name, defaultValue); }

        /// <summary>
        /// Get the previously saved string value.
        /// </summary>

        static public string GetString(string name, string defaultValue) { return EditorPrefs.GetString(name, defaultValue); }

        /// <summary>
        /// Get a previously saved color value.
        /// </summary>

        static public Color GetColor(string name, Color c)
        {
            string strVal = GetString(name, c.r + " " + c.g + " " + c.b + " " + c.a);
            string[] parts = strVal.Split(' ');

            if (parts.Length == 4)
            {
                float.TryParse(parts[0], out c.r);
                float.TryParse(parts[1], out c.g);
                float.TryParse(parts[2], out c.b);
                float.TryParse(parts[3], out c.a);
            }
            return c;
        }

        /// <summary>
        /// Get a previously saved enum from settings.
        /// </summary>

        static public T GetEnum<T>(string name, T defaultValue)
        {
            string val = GetString(name, defaultValue.ToString());
            string[] names = System.Enum.GetNames(typeof(T));
            System.Array values = System.Enum.GetValues(typeof(T));

            for (int i = 0; i < names.Length; ++i)
            {
                if (names[i] == val)
                    return (T)values.GetValue(i);
            }
            return defaultValue;
        }

        /// <summary>
        /// Get a previously saved object from settings.
        /// </summary>

        static public T Get<T>(string name, T defaultValue) where T : Object
        {
            string path = EditorPrefs.GetString(name);
            if (string.IsNullOrEmpty(path)) return null;

            T retVal = IFPipelineEditorTools.LoadAsset<T>(path);

            if (retVal == null)
            {
                int id;
                if (int.TryParse(path, out id))
                    return EditorUtility.InstanceIDToObject(id) as T;
            }
            return retVal;
        }
        #endregion

        #region Convenience accessor properties

        static public bool showTransformHandles
        {
            get { return GetBool("IFPipeline Transform Handles", false); }
            set { SetBool("IFPipeline Transform Handles", value); }
        }

        static public bool minimalisticLook
        {
            get { return GetBool("IFPipeline Minimalistic", false); }
            set { SetBool("IFPipeline Minimalistic", value); }
        }

        static public bool unifiedTransform
        {
            get { return GetBool("IFPipeline Unified", false); }
            set { SetBool("IFPipeline Unified", value); }
        }

        static public Color color
        {
            get { return GetColor("IFPipeline Color", Color.white); }
            set { SetColor("IFPipeline Color", value); }
        }

        static public Color foregroundColor
        {
            get { return GetColor("IFPipeline FG Color", Color.white); }
            set { SetColor("IFPipeline FG Color", value); }
        }

        static public Color backgroundColor
        {
            get { return GetColor("IFPipeline BG Color", Color.black); }
            set { SetColor("IFPipeline BG Color", value); }
        }

        static public ColorMode colorMode
        {
            get { return GetEnum("IFPipeline Color Mode", ColorMode.Blue); }
            set { SetEnum("IFPipeline Color Mode", value); }
        }

        static public SpritesAtlas atlas
        {
            get { return Get<SpritesAtlas>("IFPipeline Atlas", null); }
            set { Set("IFPipeline Atlas", value); }
        }

        static public Texture texture
        {
            get { return Get<Texture>("IFPipeline Texture", null); }
            set { Set("IFPipeline Texture", value); }
        }

        static public Sprite sprite2D
        {
            get { return Get<Sprite>("IFPipeline Sprite2D", null); }
            set { Set("IFPipeline Sprite2D", value); }
        }

        static public string selectedSprite
        {
            get { return GetString("IFPipeline Sprite", null); }
            set { SetString("IFPipeline Sprite", value); }
        }
        
        static public int atlasPadding
        {
            get { return GetInt("IFPipeline Padding", 1); }
            set { SetInt("IFPipeline Padding", value); }
        }

        static public bool atlasTrimming
        {
            get { return GetBool("IFPipeline Trim", true); }
            set { SetBool("IFPipeline Trim", value); }
        }

        static public bool atlasPMA
        {
            get { return GetBool("IFPipeline PMA", false); }
            set { SetBool("IFPipeline PMA", value); }
        }

        static public bool unityPacking
        {
            get { return GetBool("IFPipeline Packing", true); }
            set { SetBool("IFPipeline Packing", value); }
        }

        static public bool trueColorAtlas
        {
            get { return GetBool("IFPipeline Truecolor", true); }
            set { SetBool("IFPipeline Truecolor", value); }
        }

        static public bool keepPadding
        {
            get { return GetBool("IFPipeline KeepPadding", false); }
            set { SetBool("IFPipeline KeepPadding", value); }
        }

        static public bool forceSquareAtlas
        {
            get { return GetBool("IFPipeline Square", false); }
            set { SetBool("IFPipeline Square", value); }
        }

        static public bool allow4096
        {
            get { return GetBool("IFPipeline 4096", true); }
            set { SetBool("IFPipeline 4096", value); }
        }
        
        static public bool drawGuides
        {
            get { return GetBool("IFPipeline Guides", false); }
            set { SetBool("IFPipeline Guides", value); }
        }

        static public string charsToInclude
        {
            get { return GetString("IFPipeline Chars", ""); }
            set { SetString("IFPipeline Chars", value); }
        }

#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6
	static public string pathToFreeType
	{
		get
		{
			string path = Application.dataPath;
			if (Application.platform == RuntimePlatform.WindowsEditor) path += "/IFPipeline/Editor/FreeType.dll";
			else path += "/IFPipeline/Editor/FreeType.dylib";
			return GetString("IFPipeline FreeType", path);
		}
		set { SetString("IFPipeline FreeType", value); }
	}
#else
        static public string pathToFreeType
        {
            get
            {
                string path = Application.dataPath;
                if (Application.platform == RuntimePlatform.WindowsEditor) path += "/IFPipeline/Editor/FreeType64.dll";
                else path += "/IFPipeline/Editor/FreeType64.dylib";
                return GetString("IFPipeline FreeType64", path);
            }
            set { SetString("IFPipeline FreeType64", value); }
        }
#endif

        static public string searchField
        {
            get { return GetString("IFPipeline Search", null); }
            set { SetString("IFPipeline Search", value); }
        }

        static public string currentPath
        {
            get { return GetString("IFPipeline Path", "Assets/"); }
            set { SetString("IFPipeline Path", value); }
        }

        static public string partialSprite
        {
            get { return GetString("IFPipeline Partial", null); }
            set { SetString("IFPipeline Partial", value); }
        }

        #endregion


    }
}
