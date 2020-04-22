using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DoomSRP
{

    //[RequireComponent(typeof(Projector))]
    [RequireComponent(typeof(Light))]
#if UNITY_EDITOR
    [CanEditMultipleObjects]
#endif
    [ExecuteAlways]
    [Serializable]  
    public class ProjectorLight:MonoBehaviour
    {
        public Light light;
        public Transform cacheTransform;

        //[HideInInspector][SerializeField]public LightData lightData;
        [HideInInspector][SerializeField]public Projector iFPipelineProjector;
        [HideInInspector][SerializeField]public SpritesAtlas spritesAtlas;
        [HideInInspector][SerializeField]public string mprojSpriteName;
        [HideInInspector][SerializeField]public SpriteData projSpriteData;

        [HideInInspector] [SerializeField] public string mfalloffSpriteName;
        [HideInInspector] [SerializeField] public SpriteData falloffSpriteData;
        #region color settings
        /**
        * Color temperature in Kelvin of the blackbody illuminant.
        * White (D65) is 6500K.
        */
        [Range(1700.0f, 12000.0f)] [HideInInspector] [SerializeField] float temperature = 6500;
        [HideInInspector][SerializeField] bool useTemperature = false;
        /** 
         * Total energy that the light emits.  
         * For point/spot lights with inverse squared falloff, this is in units of lumens.  1700 lumens corresponds to a 100W lightbulb. 
         * For other lights, this is just a brightness multiplier. 
         */
        [Range(0, 20)] [HideInInspector] [SerializeField] float intensity = 3.14f;
        [Range(0, 20)] [HideInInspector] [SerializeField] public float specMultiplier = 1f;
        [Range(0, 20)] [HideInInspector] [SerializeField] public float shadowBleedReduce = 0f;

        [HideInInspector] [SerializeField] public bool biasCustom = false;
        [Range(0, 10)] [HideInInspector] [SerializeField] public float shadowBias = 1.0f;
        [Range(0, 10)] [HideInInspector] [SerializeField] public float shadowNormalBias = 1.0f;
        [Range(0.1f, 10)] [HideInInspector] [SerializeField] public float shadowNearPlane = 0.01f;
        [HideInInspector] [SerializeField] public bool softShadow = false;
 
        /** 
         * Filter color of the light.
         * Note that this can change the light's effective intensity.
         */
        [HideInInspector] [SerializeField] Color lightColor = Color.white;
        
        public float Temperature
        {
            set
            {
                temperature = value;
                temperature = Mathf.Clamp(temperature, 1700, 12000);
            }
            get
            {
                return temperature;
            }
        }

        public float Intensity
        {
            set
            {
                intensity = value;
                intensity = Mathf.Clamp(intensity, 0, 20);
            }
            get { return intensity; } }

        public Color LightColor
        {
            set { lightColor = value; }
            get { return lightColor; }
        }
        public bool UseTemperature
        {
            set { useTemperature = value; }
            get { return useTemperature; }
        }

        public Color FinalColor
        {
            get { return ColorTools.GetColorFromIntensityAndTemperature(lightColor, intensity, temperature, useTemperature); }
        }

        #endregion

        #region params
        [HideInInspector] [SerializeField] public bool lightParms_Shadow = false;//4  
        [HideInInspector] [SerializeField] public bool lightParms_Rect = true;//32
        [HideInInspector] [SerializeField] public bool lightParms_Circle = false;//16
        [HideInInspector] [SerializeField] public bool lightParms_SkipSkipModel = true;//是否允许忽略模型忽略 64
        //[HideInInspector] [SerializeField] public bool lightParams_IsArea = false;//48=32+16 夹角接近超过90°是否还计算, 也就是是否是区域光,如果是则继续判断,因为区域光的lightPos要偏移
        [HideInInspector] [SerializeField] public bool lightParams_NoDiffuse = false;//256，将diffuse light color置为0

        public int LightParams
        {
            get
            {
                int value = 0;
                value |= lightParms_Shadow ? 4 : 0;
                value |= lightParms_Rect ? 32 : 0;
                value |= lightParms_Circle ? 16 : 0;
                value |= lightParms_SkipSkipModel ? 64 : 0;
                value |= lightParams_NoDiffuse ? 256 : 0;
                return value;
            }
        }

        [HideInInspector] [SerializeField] public float area_width = 10;
        [HideInInspector] [SerializeField] public float area_height = 10;
        [HideInInspector] [SerializeField] public float area_falloff = 1;


        #endregion





        private void Awake()
        {
            light = GetComponent<Light>();
            light.type = LightType.Directional;
            //light.lightmapBakeType = LightmapBakeType.Realtime;
            cacheTransform = transform;
            projSpriteData = spritesAtlas.GetSprite(mprojSpriteName);
            falloffSpriteData = spritesAtlas.GetSprite(mfalloffSpriteName);
            iFPipelineProjector.Awake(cacheTransform);
        }

        private void OnEnable()
        {
            //var lightsMgr = LightsManager.Instance;
            //if (lightsMgr != null)
            //{
            //    lightsMgr.RegisterLight(this);
            //}
        }
#if UNITY_EDITOR
        LightDataInAll cacheLightData;
        Vector3 posa, posb, posc, posd;
#endif
        public LightDataInAll GetLightData(Matrix4x4 c2w)
        {
            LightDataInAll lightDataInAll = new LightDataInAll();

            var ps = iFPipelineProjector.GetProjectorSettings;
            {
                var bound = new SFiniteLightBound();
#if UNITY_EDITOR
                bound.frustumMatrix = ps.frustumMatrix;
#endif
                bound.planes = Projector.GetCullingPlanes(ps.frustumMatrix, c2w);//world to view
                lightDataInAll.sFiniteLightBound = bound;
            }

            LightData lightData = new LightData();
            lightData.pos = cacheTransform.position;
            lightData.lightParms = (uint)LightParams;
            lightData.posShadow = new Vector4();
            lightData.falloffR = ps.falloffR;
            lightData.projS = ps.projectMatrixX;
            lightData.projT = ps.projectMatrixY;
            lightData.projQ = ps.projectMatrixW;
            lightData.colorPacked = ColorTools.packRGBE(ColorTools.Color2Vec3(FinalColor));
            var unpack = ColorTools.unpackRGBE(2256963201);
            //Debug.Log(unpack.ToString());
            //Debug.Log(FinalColor.ToString());
            lightData.specMultiplier = specMultiplier;
            lightData.shadowBleedReduce = shadowBleedReduce;

            int sizex = spritesAtlas.width;
            int sizey = spritesAtlas.height;
            {
                SpriteData spriteData = projSpriteData;
                float x = spriteData.x;
                float y = sizey - spriteData.y - spriteData.height;//GPU得纹理坐标y轴反的,原点左上角
                float width = spriteData.width;
                float height = spriteData.height;
                float scalex = width / sizex;//todo:optimize
                float scaley = height / sizey;
                float offsetx = x / sizex;
                float offsety = y / sizey;
                ColorTools.packR15G15B15A15(
                    out lightData.scaleBias.x,
                    out lightData.scaleBias.y,
                    new Vector4(scalex, scaley, offsetx, offsety));
            }

            {
                SpriteData spriteData = falloffSpriteData;
                float x = spriteData.x;
                float y = sizey - spriteData.y - spriteData.height;//GPU得纹理坐标y轴反的,原点左上角
                float width = spriteData.width;
                float height = spriteData.height;
                float scalex = width / sizex;//todo:optimize
                float scaley = height / sizey;
                float offsetx = x / sizex;
                float offsety = y / sizey;
                ColorTools.packR15G15B15A15(
                     out lightData.scaleBias.z,
                     out lightData.scaleBias.w,
                     new Vector4(scalex, scaley, offsetx, offsety));
            }

            //public Vector4 boxMin;
            //public Vector4 boxMax;
            //public Vector4 areaPlane;
            {
                Plane plane = lightDataInAll.sFiniteLightBound.planes.Near;
                Vector3 normalws = plane.normal.normalized;
                plane.distance = Vector3.Dot(lightData.pos, normalws);
                plane.normal = normalws * area_falloff;
                lightData.areaPlane = new Vector4(
                    plane.normal.x,
                    plane.normal.y,
                    plane.normal.z,
                    plane.distance);

                //问题:已知plane，求面上偏移width,height的点
                Vector3 topright = new Vector3(area_width, area_height, 0);
                Vector3 bottomleft = new Vector3(-area_width, -area_height, 0);
                Matrix4x4 local2world = cacheTransform.localToWorldMatrix;
                topright = local2world.MultiplyPoint3x4(topright);
                bottomleft = local2world.MultiplyPoint3x4(bottomleft);
#if UNITY_EDITOR
                posa = bottomleft;
                posc = topright;
                posb = new Vector3(-area_width, area_height, 0);
                posb = local2world.MultiplyPoint3x4(posb);
                posd = new Vector3(area_width, -area_height, 0);
                posd = local2world.MultiplyPoint3x4(posd);
#endif
                Vector3 boxMin = new Vector3(
                    Mathf.Min(bottomleft.x, topright.x),
                    Mathf.Min(bottomleft.y, topright.y),
                    Mathf.Min(bottomleft.z, topright.z)
                    );
                Vector3 boxMax = new Vector3(
                   Mathf.Max(bottomleft.x, topright.x),
                   Mathf.Max(bottomleft.y, topright.y),
                   Mathf.Max(bottomleft.z, topright.z)
                   );

                lightData.boxMin = boxMin;
                lightData.boxMax = boxMax;
            }

            lightDataInAll.lightData = lightData;
#if UNITY_EDITOR
            cacheLightData = lightDataInAll;
#endif

            // shadow
            {
                LightData_Shadow shadowData = new LightData_Shadow();
                if(lightParms_Shadow)
                {
                    shadowData.projMatrix = iFPipelineProjector.GetProjectorSettings.projMatrix;
                    shadowData.viewMatrix = iFPipelineProjector.GetProjectorSettings.viewMatrix;
#if UNITY_EDITOR
                    shadowData.planes = cacheLightData.sFiniteLightBound.planes;
#endif
                }
                lightDataInAll.shadowData = shadowData;
            }

            return lightDataInAll;
        }

        private void Update()
        {
            iFPipelineProjector.Update();
#if UNITY_EDITOR
            projSpriteData = spritesAtlas.GetSprite(mprojSpriteName);
            falloffSpriteData = spritesAtlas.GetSprite(mfalloffSpriteName);

            iFPipelineProjector.Reset(cacheTransform);
            //unityLight = GetComponent<Light>();
            //FillLightData();
#endif
        }

        private void OnDisable()
        {
            //var lightsMgr = LightsManager.Instance;
            //if (lightsMgr != null)
            //{
            //    lightsMgr.RemoveLight(this);
            //}
        }

        private void Reset()
        {
            iFPipelineProjector.Reset(cacheTransform);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if(Selection.Contains(gameObject.GetInstanceID()))
            {
                iFPipelineProjector.OnDrawGizmos();
                Vector3 boxMin = cacheLightData.lightData.boxMin;
                Vector3 boxMax = cacheLightData.lightData.boxMax;
                Vector3 center = MathTools.GetCenter(boxMin, boxMax);
                Vector3 ext = MathTools.GetExtent(boxMin, boxMax);

                Gizmos.DrawWireCube(center, ext);

                Gizmos.DrawLine(posa, posb);
                Gizmos.DrawLine(posb, posc);
                Gizmos.DrawLine(posc, posd);
                Gizmos.DrawLine(posd, posa);

                Ray ray = new Ray();
                ray.origin = cacheLightData.lightData.pos;
                ray.direction = cacheLightData.lightData.areaPlane;
                Gizmos.DrawRay(ray);

            }
        }
#endif
    }
}
