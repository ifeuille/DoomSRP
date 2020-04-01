#if UNITY_EDITOR
using DoomSRP;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

[ExecuteInEditMode]
public class ClusterDebug : MonoBehaviour
{
    public static Camera selectCamera;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
 
    }

    private void OnDrawGizmos()
    {
        selectCamera = null;
        if (!this.isActiveAndEnabled)
            return;
        if (Selection.activeGameObject)
        {
            selectCamera = Selection.activeGameObject.GetComponent<Camera>();
        }
        if (null == DoomSRPPipeline.doomSRPPipeline || selectCamera == null) return;
        Gizmos.color = Color.white;
        var boxArray = DoomSRPPipeline.doomSRPPipeline.GetLightLoop().culsterDataGenerator.ClustersAABBsCache;
        Matrix4x4 cameraW = selectCamera.cameraToWorldMatrix* Matrix4x4.Scale(new Vector3(1, 1, -1));

        for (int i = 0; i < boxArray.Length; ++i)
        {
            Gizmos.DrawWireCube(cameraW.MultiplyPoint3x4(GetCenter(boxArray[i].Min, boxArray[i].Max)),
               GetExtent(boxArray[i].Min, boxArray[i].Max));
        }

    }
    Vector3 GetCenter(Vector3 min, Vector3 max)
    {
        return (min + max) * 0.5f;
    }
    Vector3 GetExtent(Vector3 min, Vector3 max)
    {
        return new Vector3(Mathf.Abs(min.x - max.x) / 2.0f, Mathf.Abs(min.y - max.y) / 2.0f, Mathf.Abs(min.z - max.z) / 2.0f);
    }
}
#endif