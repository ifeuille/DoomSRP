using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RotateScript : MonoBehaviour
{
    public float delta = 1.0f;
    public float secondsForRandColor = 3.0f;
    float currentTime;
    public DoomSRP.ProjectorLight projectorLight;
    // Start is called before the first frame update
    void Start()
    {
        currentTime = Time.time;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.Rotate(Vector3.up, delta);
        if(projectorLight && Time.time - currentTime > secondsForRandColor)
        {
            currentTime = Time.time;
            var lc = projectorLight.LightColor;
            lc.r = Random.Range(0.0f, 1);
            lc.g = Random.Range(0.0f, 1);
            lc.b = Random.Range(0.0f, 1);
            projectorLight.LightColor = lc;
        }
    }
}
