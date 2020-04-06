using System;
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
    public class Tools
    {
        static public void RegisterUndo(UnityEngine.Object obj, string name)
        {
#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(obj, name);
            Tools.SetDirty(obj);
#endif
        }

        static public string GetTypeName<T>()
        {
            string s = typeof(T).ToString();
            if (s.StartsWith("IFPipeline")) s = s.Substring(2);
            else if (s.StartsWith("UnityEngine.")) s = s.Substring(12);
            return s;
        }
        static public string GetTypeName(UnityEngine.Object obj)
        {
            if (obj == null) return "Null";
            string s = obj.GetType().ToString();
            if (s.StartsWith("IFPipeline")) s = s.Substring(2);
            else if (s.StartsWith("UnityEngine.")) s = s.Substring(12);
            return s;
        }

        static public T[] FindActive<T>() where T : Component
        {
            return GameObject.FindObjectsOfType(typeof(T)) as T[];
        }

        static public void SetDirty(UnityEngine.Object obj)
        {
#if UNITY_EDITOR
            if (obj)
            {
                UnityEditor.EditorUtility.SetDirty(obj);
            }
#endif
        }
        static public void DestroyImmediate(UnityEngine.Object obj)
        {
            if (obj != null)
            {
                if (Application.isEditor) UnityEngine.Object.DestroyImmediate(obj);
                else UnityEngine.Object.Destroy(obj);
            }
        }
    }
}
