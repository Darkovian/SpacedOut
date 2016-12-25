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
    [RequireComponent(typeof(Renderer))]
    public abstract class WeatherMakerCloudScript : MonoBehaviour
    {
        /// <summary>
        /// Whether clouds should have per-pixel lighting
        /// </summary>
        public static bool EnablePerPixelLighting;

        [Header("Rendering")]
        [Tooltip("Material for the clouds")]
        public Material Material;
        private Material sharedMaterial;
        private Material lastMaterial;

        [Tooltip("Override the texture for the material")]
        public Texture2D MaterialTexture;

        [Tooltip("Override the mask texture for the material")]
        public Texture2D MaterialMaskTexture;

        [Tooltip("Tint color for the clouds")]
        public Color TintColor = Color.white;

        [Range(1, 16)]
        [Tooltip("Number of rows in the cloud material")]
        public int MaterialRows = 1;

        [Range(1, 16)]
        [Tooltip("Number of columns in the cloud material")]
        public int MaterialColumns = 1;

        /// <summary>
        /// Camera, where the clouds should hover over
        /// </summary>
        [HideInInspector]
        [System.NonSerialized]
        public Camera Camera;

        [Header("Positioning")]
        [Tooltip("Whether to anchor the clouds to the Anchor (i.e. main camera) or not")]
        public bool AnchorClouds = true;

        [Tooltip("Offset from the anchor that the clouds should position themselves at")]
        public Vector3 AnchorOffset = new Vector3(0.0f, -700.0f, 0.0f);

        [Header("Count")]
        [Range(0, 16250)]
        [Tooltip("The total number of clouds to create")]
        public int NumberOfClouds = 1000;

        private void UpdateTransform()
        {
            if (Camera != null && AnchorClouds)
            {
                Vector3 pos = Camera.transform.position;
                Vector3 curPos = gameObject.transform.position;
                curPos.x = pos.x + AnchorOffset.x;
                curPos.y = AnchorOffset.y;
                curPos.z = (Camera.orthographic ? curPos.z : pos.z + AnchorOffset.z);
                gameObject.transform.position = curPos;
            }
        }

        private void UpdateMaterial()
        {
            if (Material != lastMaterial)
            {
                Renderer renderer = GetComponent<Renderer>();
                sharedMaterial = (Material == null ? null : new Material(Material));
                if (renderer != null)
                {
                    renderer.sharedMaterial = sharedMaterial;
                }
                lastMaterial = Material;
            }
            if (sharedMaterial != null)
            {
                sharedMaterial.mainTexture = (MaterialTexture == null ? sharedMaterial.mainTexture : MaterialTexture);
                sharedMaterial.SetTexture("_MaskTex", MaterialMaskTexture);
                sharedMaterial.SetColor("_TintColor", TintColor);
                if (EnablePerPixelLighting)
                {
                    sharedMaterial.EnableKeyword("PER_PIXEL_LIGHTING");
                }
                else
                {
                    sharedMaterial.DisableKeyword("PER_PIXEL_LIGHTING");
                }
                OnUpdateMaterial(sharedMaterial);
            }
        }

        protected virtual void OnUpdateMaterial(Material m) { }

        protected virtual void Start()
        {
            UpdateMaterial();
            UpdateTransform();
        }

        protected virtual void Update()
        {
            UpdateMaterial();
            UpdateTransform();
        }

        /// <summary>
        /// Reset the clouds
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// Create clouds
        /// </summary>
        public abstract void CreateClouds();

        /// <summary>
        /// Remove clouds
        /// </summary>
        public abstract void RemoveClouds();
    }
}