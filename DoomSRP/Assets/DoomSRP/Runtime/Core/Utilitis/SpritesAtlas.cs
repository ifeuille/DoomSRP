using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DoomSRP
{
#if UNITY_EDITOR
    [CustomEditor(typeof(SpritesAtlas))]
    [CanEditMultipleObjects]
    public class SpritesAtlasEditor : Editor
    {
        override public void OnInspectorGUI()
        {
            SpritesAtlas atlas = target as SpritesAtlas;
            EditorGUILayout.Space();
            GUILayout.BeginVertical("box");

            Texture2D texture = EditorGUILayout.ObjectField("Atlas Texture", atlas.texture, typeof(Texture), false) as Texture2D;
            if(texture)
            {
                if(texture != atlas.texture)
                {
                    Undo.RecordObject(atlas, "Atlas Change");
                    atlas.texture = texture;
                    //atlas.ClearAtlas();
                }
                else
                {
                    Undo.RecordObject(atlas, "Import Sprites");
                    TextAsset text = EditorGUILayout.ObjectField("Sprite Import", null, typeof(TextAsset), false) as TextAsset;
                    if (text != null)
                    {
                        atlas.LoadAllAtlasEntryData(text);
                        EditorUtility.SetDirty(target);
                    }
                }              
            }
            else
            {
                //atlas.ClearAtlas();
                atlas.texture = null;
            }
            GUILayout.EndVertical();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }
    }
#endif
    //[AddComponentMenu("IFpipeline/Component/SpritesAtlas")]
    [System.Serializable]
    public class SpritesAtlas: CacheBehaviour
    {
        [HideInInspector] [SerializeField] public Texture2D texture;
        [HideInInspector] [SerializeField] public List<SpriteData> mSprites = new List<SpriteData>();
        [HideInInspector] [SerializeField] public float mPixelSize = 1f;
        [HideInInspector] [SerializeField] public int width = 0;
        [HideInInspector] [SerializeField] public int height = 0;

        Dictionary<string, int> mSpriteIndices = new Dictionary<string, int>();

        public List<SpriteData> spriteList
        {
            get
            {
                return mSprites;
            }
            set
            {
                mSprites = value;
                MarkAsChanged();
            }
        }

        public float pixelSize
        {
            get
            {
                return mPixelSize;
            }
            set
            {
                float val = Mathf.Clamp(value, 0.25f, 4f);
                if(mPixelSize != val)
                {
                    mPixelSize = val;
                    MarkAsChanged();
                }
            }
        }

        public SpriteData GetSprite(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                if (mSprites.Count == 0) return null;
#if UNITY_EDITOR
                if(Application.isPlaying)
#endif
                {
                    // The number of indices differs from the sprite list? Rebuild the indices.
                    if (mSpriteIndices.Count != mSprites.Count)
                        MarkSpriteListAsChanged();
                    int index;
                    if (mSpriteIndices.TryGetValue(name, out index))
                    {
                        // If the sprite is present, return it as-is
                        if (index > -1 && index < mSprites.Count) return mSprites[index];
                        // The sprite index was out of range -- perhaps the sprite was removed? Rebuild the indices.
                        MarkSpriteListAsChanged();
                        // Try to look up the index again
                        return mSpriteIndices.TryGetValue(name, out index) ? mSprites[index] : null;
                    }

                }

                // Sequential O(N) lookup.
                for (int i = 0, imax = mSprites.Count; i < imax; ++i)
                {
                    SpriteData s = mSprites[i];
                    // string.Equals doesn't seem to work with Flash export
                    if (!string.IsNullOrEmpty(s.name) && name == s.name)
                    {
#if UNITY_EDITOR
                        if (!Application.isPlaying) return s;
#endif
                        // If this point was reached then the sprite is present in the non-indexed list,
                        // so the sprite indices should be updated.
                        MarkSpriteListAsChanged();
                        return s;
                    }
                }
            }
            return null;
        }

        public string GetRandomSprite(string startsWith)
        {
            if (GetSprite(startsWith) == null)
            {
                System.Collections.Generic.List<SpriteData> sprites = spriteList;
                System.Collections.Generic.List<string> choices = new System.Collections.Generic.List<string>();

                foreach (SpriteData sd in sprites)
                {
                    if (sd.name.StartsWith(startsWith))
                        choices.Add(sd.name);
                }
                return (choices.Count > 0) ? choices[UnityEngine.Random.Range(0, choices.Count)] : null;
            }
            return startsWith;
        }

        public void MarkSpriteListAsChanged()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
#endif
            {
                mSpriteIndices.Clear();
                for (int i = 0, imax = mSprites.Count; i < imax; ++i)
                    mSpriteIndices[mSprites[i].name] = i;
            }
        }

        public static System.Comparison<SpriteData> comparison_SpriteData_CompareFunc = 
            new Comparison<SpriteData>(delegate (SpriteData s1, SpriteData s2) { return s1.name.CompareTo(s2.name); });

        public void SortAlphabetically()
        {
            mSprites.Sort(comparison_SpriteData_CompareFunc/*delegate(UISpriteData s1, UISpriteData s2) { return s1.name.CompareTo(s2.name); }*/);
#if UNITY_EDITOR
            Tools.SetDirty(this);
#endif
        }

        public BetterList<string> GetListOfSprites()
        {
            BetterList<string> list = new BetterList<string>();

            for (int i = 0, imax = mSprites.Count; i < imax; ++i)
            {
                SpriteData s = mSprites[i];
                if (s != null && !string.IsNullOrEmpty(s.name)) list.Add(s.name);
            }
            return list;
        }

        public BetterList<string> GetListOfSprites(string match)
        {
            if (string.IsNullOrEmpty(match)) return GetListOfSprites();
            
            BetterList<string> list = new BetterList<string>();

            // First try to find an exact match
            for (int i = 0, imax = mSprites.Count; i < imax; ++i)
            {
                SpriteData s = mSprites[i];

                if (s != null && !string.IsNullOrEmpty(s.name) && string.Equals(match, s.name, StringComparison.OrdinalIgnoreCase))
                {
                    list.Add(s.name);
                    return list;
                }
            }

            // No exact match found? Split up the search into space-separated components.
            string[] keywords = match.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < keywords.Length; ++i) keywords[i] = keywords[i].ToLower();

            // Try to find all sprites where all keywords are present
            for (int i = 0, imax = mSprites.Count; i < imax; ++i)
            {
                SpriteData s = mSprites[i];

                if (s != null && !string.IsNullOrEmpty(s.name))
                {
                    string tl = s.name.ToLower();
                    int matches = 0;

                    for (int b = 0; b < keywords.Length; ++b)
                    {
                        if (tl.Contains(keywords[b])) ++matches;
                    }
                    if (matches == keywords.Length) list.Add(s.name);
                }
            }
            return list;
        }

        bool References(SpritesAtlas atlas)
        {
            if (atlas == null) return false;
            if (atlas == this) return true;
            return false;
        }

        static public bool CheckIfRelated(SpritesAtlas a, SpritesAtlas b)
        {
            if (a == null || b == null) return false;
            return a == b || a.References(b) || b.References(a);
        }

        void MarkAsChanged()
        {
#if UNITY_EDITOR
            Tools.SetDirty(gameObject);
#endif

//            UISprite[] list = IFPipelineTools.FindActive<UISprite>();

//            for (int i = 0, imax = list.Length; i < imax; ++i)
//            {
//                UISprite sp = list[i];

//                if (CheckIfRelated(this, sp.atlas))
//                {
//                    SpritesAtlas atl = sp.atlas;
//                    sp.atlas = null;
//                    sp.atlas = atl;
//#if UNITY_EDITOR
//                    NGUITools.SetDirty(sp);
//#endif
//                }
//            }

        }

        public void ClearAtlas()
        {
            if(mSprites.Count > 0)
            {
                mSprites.Clear();
                Debug.Log("Atlas has clear!!");
            }
        }

        public void LoadAllAtlasEntryData(TextAsset textAsset)
        {
            if (textAsset == null)
                return;

            string jsonString = textAsset.text;
            if (string.IsNullOrEmpty(jsonString))
                return;

            Hashtable decodedHash = JsonHelper.jsonDecode(jsonString) as Hashtable;
            if (decodedHash == null)
                return;
            mSprites = new List<SpriteData>();

            ClearAtlas();
            Hashtable frames = (Hashtable)decodedHash["frames"];
            foreach (System.Collections.DictionaryEntry item in frames)
            {
                SpriteData atlasEntry = new SpriteData();

                atlasEntry.name = item.Key.ToString();
                atlasEntry.name = StringTools.GetFileNameWithoutExtension(atlasEntry.name);

                Hashtable table = (Hashtable)item.Value;
                Hashtable frame = (Hashtable)table["frame"];
                int frameX = int.Parse(frame["x"].ToString());
                int frameY = int.Parse(frame["y"].ToString());
                int frameW = int.Parse(frame["w"].ToString());
                int frameH = int.Parse(frame["h"].ToString());

                atlasEntry.x = frameX;
                atlasEntry.y = frameY;
                atlasEntry.width = frameW;
                atlasEntry.height = frameH;

                spriteList.Add(atlasEntry);
            }
            Hashtable meta = (Hashtable)decodedHash["meta"];
            Hashtable wh = (Hashtable)meta["size"];
            width = int.Parse(wh["w"].ToString());
            height = int.Parse(wh["h"].ToString());
            Debug.Log("Atlas has Import!!");

            //MarkSpriteListAsChanged();
            MarkAsChanged();
        }

        void Start()
        {

        }

    }
}
