//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2015 Tasharen Entertainment
//----------------------------------------------

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace DoomSRP
{
    /// <summary>
    /// Editor component used to display a list of sprites.
    /// </summary>

    public class SpriteSelector : ScriptableWizard
    {
        static public SpriteSelector instance;

        void OnEnable() { instance = this; }
        void OnDisable() { instance = null; }

        public delegate void Callback(string sprite);

        SerializedObject mObject;
        SerializedProperty mProperty;

        //UISprite mSprite;
        Vector2 mPos = Vector2.zero;
        Callback mCallback;
        float mClickTime = 0f;

        /// <summary>
        /// Draw the custom wizard.
        /// </summary>

        void OnGUI()
        {
            IFPipelineEditorTools.SetLabelWidth(80f);

            if (IFPipelineSettings.atlas == null)
            {
                GUILayout.Label("No Atlas selected.", "LODLevelNotifyText");
            }
            else
            {
                SpritesAtlas atlas = IFPipelineSettings.atlas;
                bool close = false;
                GUILayout.Label(atlas.name + " Sprites", "LODLevelNotifyText");
                IFPipelineEditorTools.DrawSeparator();

                GUILayout.BeginHorizontal();
                GUILayout.Space(84f);

                string before = IFPipelineSettings.partialSprite;
                string after = EditorGUILayout.TextField("", before, "SearchTextField");
                if (before != after) IFPipelineSettings.partialSprite = after;

                if (GUILayout.Button("", "SearchCancelButton", GUILayout.Width(18f)))
                {
                    IFPipelineSettings.partialSprite = "";
                    GUIUtility.keyboardControl = 0;
                }
                GUILayout.Space(84f);
                GUILayout.EndHorizontal();

                Texture2D tex = atlas.texture as Texture2D;

                if (tex == null)
                {
                    GUILayout.Label("The atlas doesn't have a texture to work with");
                    return;
                }

                BetterList<string> sprites = atlas.GetListOfSprites(IFPipelineSettings.partialSprite);

                float size = 80f;
                float padded = size + 10f;
                int columns = Mathf.FloorToInt(Screen.width / padded);
                if (columns < 1) columns = 1;

                int offset = 0;
                Rect rect = new Rect(10f, 0, size, size);

                GUILayout.Space(10f);
                mPos = GUILayout.BeginScrollView(mPos);
                int rows = 1;

                while (offset < sprites.size)
                {
                    GUILayout.BeginHorizontal();
                    {
                        int col = 0;
                        rect.x = 10f;

                        for (; offset < sprites.size; ++offset)
                        {
                            SpriteData sprite = atlas.GetSprite(sprites[offset]);
                            if (sprite == null) continue;

                            // Button comes first
                            if (GUI.Button(rect, ""))
                            {
                                if (Event.current.button == 0)
                                {
                                    float delta = Time.realtimeSinceStartup - mClickTime;
                                    mClickTime = Time.realtimeSinceStartup;

                                    if (IFPipelineSettings.selectedSprite != sprite.name)
                                    {
                                        //if (mSprite != null)
                                        //{
                                        //    IFPipelineEditorTools.RegisterUndo("Atlas Selection", mSprite);
                                        //    mSprite.MakePixelPerfect();
                                        //    EditorUtility.SetDirty(mSprite.gameObject);
                                        //}

                                        IFPipelineSettings.selectedSprite = sprite.name;
                                        IFPipelineEditorTools.RepaintSprites();
                                        mCallback?.Invoke(sprite.name);
                                    }
                                    else if (delta < 0.5f) close = true;
                                }
                                else
                                {
                                    //NGUIContextMenu.AddItem("Edit", false, EditSprite, sprite);
                                    //NGUIContextMenu.AddItem("Delete", false, DeleteSprite, sprite);
                                    //NGUIContextMenu.Show();
                                }
                            }

                            if (Event.current.type == EventType.Repaint)
                            {
                                // On top of the button we have a checkboard grid
                                IFPipelineEditorTools.DrawTiledTexture(rect, IFPipelineEditorTools.backdropTexture);
                                Rect uv = new Rect(sprite.x, sprite.y, sprite.width, sprite.height);
                                uv = Math.ConvertToTexCoords(uv, tex.width, tex.height);

                                // Calculate the texture's scale that's needed to display the sprite in the clipped area
                                float scaleX = rect.width / uv.width;
                                float scaleY = rect.height / uv.height;

                                // Stretch the sprite so that it will appear proper
                                float aspect = (scaleY / scaleX) / ((float)tex.height / tex.width);
                                Rect clipRect = rect;

                                if (aspect != 1f)
                                {
                                    if (aspect < 1f)
                                    {
                                        // The sprite is taller than it is wider
                                        float padding = size * (1f - aspect) * 0.5f;
                                        clipRect.xMin += padding;
                                        clipRect.xMax -= padding;
                                    }
                                    else
                                    {
                                        // The sprite is wider than it is taller
                                        float padding = size * (1f - 1f / aspect) * 0.5f;
                                        clipRect.yMin += padding;
                                        clipRect.yMax -= padding;
                                    }
                                }

                                GUI.DrawTextureWithTexCoords(clipRect, tex, uv);

                                // Draw the selection
                                if (IFPipelineSettings.selectedSprite == sprite.name)
                                {
                                    IFPipelineEditorTools.DrawOutline(rect, new Color(0.4f, 1f, 0f, 1f));
                                }
                            }

                            GUI.backgroundColor = new Color(1f, 1f, 1f, 0.5f);
                            GUI.contentColor = new Color(1f, 1f, 1f, 0.7f);
                            GUI.Label(new Rect(rect.x, rect.y + rect.height, rect.width, 32f), sprite.name, "ProgressBarBack");
                            GUI.contentColor = Color.white;
                            GUI.backgroundColor = Color.white;

                            if (++col >= columns)
                            {
                                ++offset;
                                break;
                            }
                            rect.x += padded;
                        }
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.Space(padded);
                    rect.y += padded + 26;
                    ++rows;
                }
                GUILayout.Space(rows * 26);
                GUILayout.EndScrollView();

                if (close) Close();
            }
        }

        /// <summary>
        /// Edit the sprite (context menu selection)
        /// </summary>

        void EditSprite(object obj)
        {
            if (this == null) return;
            SpriteData sd = obj as SpriteData;
            IFPipelineEditorTools.SelectSprite(sd.name);
            Close();
        }

        /// <summary>
        /// Delete the sprite (context menu selection)
        /// </summary>

        //void DeleteSprite(object obj)
        //{
        //    if (this == null) return;
        //    SpriteData sd = obj as SpriteData;

        //    List<SpritesAtlasMaker.SpriteEntry> sprites = new List<SpritesAtlasMaker.SpriteEntry>();
        //    SpritesAtlasMaker.ExtractSprites(IFPipelineSettings.atlas, sprites);

        //    for (int i = sprites.Count; i > 0;)
        //    {
        //        SpritesAtlasMaker.SpriteEntry ent = sprites[--i];
        //        if (ent.name == sd.name)
        //            sprites.RemoveAt(i);
        //    }
        //    SpritesAtlasMaker.UpdateAtlas(IFPipelineSettings.atlas, sprites);
        //    IFPipelineEditorTools.RepaintSprites();
        //}

        /// <summary>
        /// Property-based selection result.
        /// </summary>

        void OnSpriteSelection(string sp)
        {
            if (mObject != null && mProperty != null)
            {
                mObject.Update();
                mProperty.stringValue = sp;
                mObject.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Show the sprite selection wizard.
        /// </summary>

        static public void ShowSelected()
        {
            if (IFPipelineSettings.atlas != null)
            {
                Show(delegate (string sel) { IFPipelineEditorTools.SelectSprite(sel); });
            }
        }

        /// <summary>
        /// Show the sprite selection wizard.
        /// </summary>

        static public void Show(SerializedObject ob, SerializedProperty pro, SpritesAtlas atlas)
        {
            if (instance != null)
            {
                instance.Close();
                instance = null;
            }

            if (ob != null && pro != null && atlas != null)
            {
                SpriteSelector comp = ScriptableWizard.DisplayWizard<SpriteSelector>("Select a Sprite");
                IFPipelineSettings.atlas = atlas;
                IFPipelineSettings.selectedSprite = pro.hasMultipleDifferentValues ? null : pro.stringValue;
                //comp.mSprite = null;
                comp.mObject = ob;
                comp.mProperty = pro;
                comp.mCallback = comp.OnSpriteSelection;
            }
        }

        /// <summary>
        /// Show the selection wizard.
        /// </summary>

        static public void Show(Callback callback)
        {
            if (instance != null)
            {
                instance.Close();
                instance = null;
            }

            SpriteSelector comp = ScriptableWizard.DisplayWizard<SpriteSelector>("Select a Sprite");
            //comp.mSprite = null;
            comp.mCallback = callback;
        }
    }

}

