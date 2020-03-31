using DoomSRP;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class ClusterDebug : MonoBehaviour
{

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
        if (null == DoomSRPPipeline.doomSRPPipeline) return;
        Gizmos.color = Color.white;
        var boxArray = DoomSRPPipeline.doomSRPPipeline.GetLightLoop().culsterDataGenerator.ClustersAABBs;
        for(int i = 0; i < boxArray.Length; ++i)
        {
            Gizmos.DrawWireCube(GetCenter(boxArray[i].Min, boxArray[i].Max),
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
