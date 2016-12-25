//
// Weather Maker for Unity
// (c) 2016 Digital Ruby, LLC
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 

using UnityEngine;
using System.Collections;

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Sky modes
    /// </summary>
    public enum WeatherMakeSkyMode
    {
        /// <summary>
        /// Textured - day, dawn/dusk and night are all done via textures
        /// </summary>
        Textured = 0,

        /// <summary>
        /// Procedural sky - day and dawn/dusk textures are overlaid on top of procedural sky, night texture is used as is
        /// </summary>
        ProceduralTextured,

        /// <summary>
        /// Procedural sky - day, dawn/dusk textures are ignored, night texture is used as is
        /// </summary>
        Procedural
    }

    /// <summary>
    /// Sun modes
    /// </summary>
    public enum WeatherMakerSunMode
    {
        /// <summary>
        /// No sun
        /// </summary>
        Disabled,

        /// <summary>
        /// High quality sun
        /// </summary>
        HighQuality,

        /// <summary>
        /// Fast to render sun
        /// </summary>
        Fast
    }

    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class WeatherMakerSkySphereScript : MonoBehaviour
    {
        [Tooltip("The camera the sky sphere should follow")]
        public Camera Camera;

        [Tooltip("The sky mode. 'Textured' uses a texture for day, dawn/dusk and night. " +
            "'Procedural textured' combines a procedural sky with the day and dawn/dusk textures using alpha, and uses the night texture as is. " +
            "'Procedural' uses the night texture as is and does everything else procedurally.")]
        public WeatherMakeSkyMode SkyMode = WeatherMakeSkyMode.Textured;

        [Tooltip("The sun mode. Disabled, HighQuality or Fast.")]
        public WeatherMakerSunMode SunMode = WeatherMakerSunMode.HighQuality;

        [Header("Sun")]
        [Tooltip("Sun")]
        public Light Sun;

        [Range(0.0f, 10.0f)]
        [Tooltip("The size of the sun in the sky, 0 to disable the sun.")]
        public float SunSize = 0.02f;

        [Header("Positioning")]
        [Range(-0.5f, 0.5f)]
        [Tooltip("Offset the sky this amount from the camera y. This value is multiplied by the height of the sky sphere.")]
        public float YOffsetMultiplier = 0.0f;

        [Range(0.1f, 0.99f)]
        [Tooltip("Place the sky sphere at this amount of the far clip plane")]
        public float FarClipPlaneMultiplier = 0.8f;

        [Header("Textures - dawn/dusk not used in procedural sky.")]
        [Tooltip("The daytime texture")]
        public Texture2D DayTexture;

        [Tooltip("The dawn / dusk texture (not used for procedural sky) - this MUST be set if DawnDuskFadeDegrees is not 0, otherwise things will look funny.")]
        public Texture2D DawnDuskTexture;

        [Tooltip("The night time texture")]
        public Texture2D NightTexture;

        [Range(0.0f, 180.0f)]
        [Tooltip("It is fully day (i.e. no more night) when the sun x is at least this degrees above 0. 180 is high noon.")]
        public float DayDegrees = 95.0f;

        [Range(0.0f, 30.0f)]
        [Tooltip("The number of degrees that it fades from day to dawn/dusk before starting to fade to night. Set to 0 to fade from day and night directly. " +
            "For equal transitions from day to dusk and night, set this equal to NightFadeDegrees, but this is not required.")]
        public float DawnDuskFadeDegrees = 0.0f;

        [Range(0.0f, 90.0f)]
        [Tooltip("The number of degrees that it fades from day or dawn/dusk to night before becoming fully night")]
        public float NightFadeDegrees = 30.0f;

        [Range(0.0f, 1.0f)]
        [Tooltip("Night pixels must have an R, G or B value greater than or equal to this to be visible. Raise this value if you want to hide dimmer elements " +
            "of your night texture or there is a lot of light pollution, i.e. a city.")]
        public float NightVisibilityThreshold = 0.0f;

        [Tooltip("Weather maker script")]
        public WeatherMakerScript WeatherScript;

#if UNITY_EDITOR

        [Header("Generation of Sphere")]
        [Range(2, 6)]
        [Tooltip("Resolution of sphere. The higher the more triangles.")]
        public int Resolution = 4;

        [UnityEngine.HideInInspector]
        [UnityEngine.SerializeField]
        private int lastResolution = -1;

        [Tooltip("UV mode for sphere generation")]
        public UVMode UVMode = UVMode.PanoramaMirrorDown;

        [UnityEngine.HideInInspector]
        [UnityEngine.SerializeField]
        private UVMode lastUVMode = (UVMode)int.MaxValue;

#endif

        private MeshRenderer meshRenderer;
        private MeshFilter meshFilter;

        private void DestroyMesh()
        {
            if (meshFilter.sharedMesh != null)
            {
                GameObject.DestroyImmediate(meshFilter.sharedMesh, true);
                meshFilter.sharedMesh = null;
            }
        }

        private void SetShaderSunParameters()
        {
            if (SkyMode == WeatherMakeSkyMode.Textured)
            {
                meshRenderer.sharedMaterial.SetVector("_SunNormal", Sun.transform.forward);
            }
            else
            {
                meshRenderer.sharedMaterial.SetVector("_SunNormal", -Sun.transform.forward);
            }
            meshRenderer.sharedMaterial.SetColor("_SunColor", Sun.color);
            meshRenderer.sharedMaterial.SetFloat("_SunSize", SunSize);

            if (Sun.enabled && Sun.intensity > 0.0f && SunSize > 0.0f)
            {
                if (SunMode == WeatherMakerSunMode.HighQuality)
                {
                    meshRenderer.sharedMaterial.EnableKeyword("ENABLE_SUN_HIGH_QUALITY");
                    meshRenderer.sharedMaterial.DisableKeyword("ENABLE_SUN_FAST");
                }
                else if (SunMode == WeatherMakerSunMode.Fast)
                {
                    meshRenderer.sharedMaterial.EnableKeyword("ENABLE_SUN_FAST");
                    meshRenderer.sharedMaterial.DisableKeyword("ENABLE_SUN_HIGH_QUALITY");
                }
                else
                {
                    meshRenderer.sharedMaterial.DisableKeyword("ENABLE_SUN_HIGH_QUALITY");
                    meshRenderer.sharedMaterial.DisableKeyword("ENABLE_SUN_FAST");
                }
            }
            else
            {
                meshRenderer.sharedMaterial.DisableKeyword("ENABLE_SUN_HIGH_QUALITY");
                meshRenderer.sharedMaterial.DisableKeyword("ENABLE_SUN_FAST");
            }
        }

        private void SetShaderSkyParameters()
        {
            meshRenderer.sharedMaterial.mainTexture = DayTexture;
            meshRenderer.sharedMaterial.SetTexture("_DawnDuskTex", DawnDuskTexture);
            meshRenderer.sharedMaterial.SetTexture("_NightTex", NightTexture);
            if (SkyMode == WeatherMakeSkyMode.Textured)
            {
                meshRenderer.sharedMaterial.DisableKeyword("ENABLE_PROCEDURAL_TEXTURED_SKY");
                meshRenderer.sharedMaterial.DisableKeyword("ENABLE_PROCEDURAL_SKY");
            }
            else if (SkyMode == WeatherMakeSkyMode.Procedural)
            {
                meshRenderer.sharedMaterial.EnableKeyword("ENABLE_PROCEDURAL_SKY");
                meshRenderer.sharedMaterial.DisableKeyword("ENABLE_PROCEDURAL_TEXTURED_SKY");
            }
            else if (SkyMode == WeatherMakeSkyMode.ProceduralTextured)
            {
                meshRenderer.sharedMaterial.EnableKeyword("ENABLE_PROCEDURAL_TEXTURED_SKY");
                meshRenderer.sharedMaterial.DisableKeyword("ENABLE_PROCEDURAL_SKY");
            }
        }

        private void SetShaderLightParameters()
        {
            // always fully day between these two values
            float dayMaximum = 360.0f - DayDegrees;
            float dayMultiplier;
            float dawnDuskMultiplier;
            float nightMultiplier;
            float sunRotation = (Sun == null ? 90.0f : Sun.transform.eulerAngles.x) + 90.0f;

            // fade to night faster for procedural sky
            float dawnDuskFadeDegrees = (SkyMode == WeatherMakeSkyMode.Textured ? DawnDuskFadeDegrees : DawnDuskFadeDegrees * 0.5f);

            if (sunRotation > 360.0f)
            {
                sunRotation -= 360.0f;
            }

            if (sunRotation >= DayDegrees && sunRotation <= dayMaximum)
            {
                dayMultiplier = 1.0f;

                // fully day, these are 0
                dawnDuskMultiplier = nightMultiplier = 0.0f;
            }
            else if (sunRotation < (DayDegrees - NightFadeDegrees - dawnDuskFadeDegrees) || sunRotation > (dayMaximum + NightFadeDegrees + dawnDuskFadeDegrees))
            {
                nightMultiplier = 1.0f;

                // fully night, these are 0
                dayMultiplier = dawnDuskMultiplier = 0.0f;
            }
            else if (sunRotation < DayDegrees)
            {
                if (dawnDuskFadeDegrees == 0.0f)
                {
                    dawnDuskMultiplier = 0.0f;
                    dayMultiplier = Mathf.Lerp(1.0f, 0.0f, 1.0f - ((sunRotation - (DayDegrees - NightFadeDegrees)) / NightFadeDegrees));
                    nightMultiplier = 1.0f - dayMultiplier;
                }
                else if (sunRotation < DayDegrees - dawnDuskFadeDegrees)
                {
                    dayMultiplier = 0.0f;

                    // fade from dawn/dusk to night
                    dawnDuskMultiplier = Mathf.Lerp(1.0f, 0.0f, 1.0f - ((sunRotation - (DayDegrees - dawnDuskFadeDegrees - NightFadeDegrees)) / NightFadeDegrees));
                    nightMultiplier = 1.0f - dawnDuskMultiplier;
                }
                else
                {
                    nightMultiplier = 0.0f;

                    // fade from day to dawn/dusk
                    dayMultiplier = Mathf.Lerp(1.0f, 0.0f, 1.0f - ((sunRotation - (DayDegrees - dawnDuskFadeDegrees)) / dawnDuskFadeDegrees));
                    dawnDuskMultiplier = 1.0f - dayMultiplier;
                }
            }
            else
            {
                if (dawnDuskFadeDegrees == 0.0f)
                {
                    dawnDuskMultiplier = 0.0f;
                    dayMultiplier = Mathf.Lerp(1.0f, 0.0f, 1.0f - ((sunRotation - (360.0f - NightFadeDegrees)) / NightFadeDegrees));
                    nightMultiplier = 1.0f - dayMultiplier;
                }
                else if (sunRotation > (dayMaximum + dawnDuskFadeDegrees))
                {
                    dayMultiplier = 0.0f;

                    // fade from dawn/dusk to night
                    dawnDuskMultiplier = Mathf.Lerp(1.0f, 0.0f, 1.0f - ((sunRotation - (360.0f - dawnDuskFadeDegrees - NightFadeDegrees)) / NightFadeDegrees));
                    nightMultiplier = 1.0f - dawnDuskMultiplier;
                }
                else
                {
                    nightMultiplier = 0.0f;

                    // fade from day to dawn/dusk
                    dayMultiplier = Mathf.Lerp(1.0f, 0.0f, 1.0f - ((sunRotation - (360.0f - dawnDuskFadeDegrees)) / dawnDuskFadeDegrees));
                    dawnDuskMultiplier = 1.0f - dayMultiplier;
                }
            }

            // Debug.LogFormat("Day: {0}, Dawn: {1}, Night: {2}", dayMultiplier, dawnDuskMultiplier, nightMultiplier);

            meshRenderer.sharedMaterial.SetFloat("_DayMultiplier", dayMultiplier);
            meshRenderer.sharedMaterial.SetFloat("_DawnDuskMultiplier", dawnDuskMultiplier);
            meshRenderer.sharedMaterial.SetFloat("_NightMultiplier", nightMultiplier);
            meshRenderer.sharedMaterial.SetFloat("_NightVisibilityThreshold", NightVisibilityThreshold);
        }

        private void SetupSkySphere()
        {

#if UNITY_EDITOR

            Camera = (Camera == null ? (Camera.main == null ? Camera.current : Camera.main) : Camera);

            if (Resolution != lastResolution)
            {
                lastResolution = Resolution;
                DestroyMesh();
            }
            if (UVMode != lastUVMode)
            {
                lastUVMode = UVMode;
                DestroyMesh();
            }
            Mesh mesh = meshFilter.sharedMesh;
            if (mesh == null)
            {
                meshFilter.sharedMesh = SphereCreator.Create(Resolution, UVMode);
            }

#endif

            float farPlane = FarClipPlaneMultiplier * Camera.farClipPlane;
            float yOffset = farPlane * YOffsetMultiplier;
            gameObject.transform.position = Camera.transform.position + new Vector3(0.0f, yOffset, 0.0f);
            float scale = farPlane * ((Camera.farClipPlane - Mathf.Abs(yOffset)) / Camera.farClipPlane);
            gameObject.transform.localScale = new Vector3(scale, scale, scale);
        }

        private void UpdateSkySphere()
        {

#if UNITY_EDITOR

            if (meshRenderer.sharedMaterial == null)
            {
                Debug.LogError("Sky sphere material not set");
                return;
            }

#endif

            SetupSkySphere();
            SetShaderSunParameters();
            SetShaderSkyParameters();
            SetShaderLightParameters();            
        }

        private void Start()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            meshFilter = GetComponent<MeshFilter>();
            UpdateSkySphere();
        }

        private void Update()
        {
            UpdateSkySphere();
        }
    }
}