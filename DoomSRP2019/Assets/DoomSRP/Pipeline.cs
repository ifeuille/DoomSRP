using Lightmapping = UnityEngine.Experimental.GlobalIllumination.Lightmapping;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Experimental.Rendering;
using System.Collections.Generic;
using UnityEngine.Rendering;
using System.Collections;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DoomSRP
{
    public class DoomSRPPipeline : RenderPipeline
    {
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            GraphicsSettings.lightsUseLinearIntensity = true;
            //SortCameras(cameras);
            //foreach (Camera camera in cameras)
            //{
            //    CommandBuffer cmd = CommandBufferPool.Get(k_RenderCameraTag);
            //    using (new ProfilingSample(cmd, k_RenderCameraTag))
            //    {
            //        BeginCameraRendering(camera);
            //        foreach (var beforeCamera in camera.GetComponents<IBeforeCameraRender>())
            //            beforeCamera.ExecuteBeforeCameraRender(this, context, camera);

            //        RenderSingleCamera(this, context, camera, ref m_CullResults, camera.GetComponent<IRendererSetup>());

            //    }
            //}
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

        }
    }
}