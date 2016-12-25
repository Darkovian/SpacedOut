//
// Weather Maker for Unity
// (c) 2016 Digital Ruby, LLC
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 

using UnityEngine;
using System.Collections.Generic;

namespace DigitalRuby.WeatherMaker
{
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class WeatherMakerCloudScript3D : WeatherMakerCloudScript
    {
        private class CloudGenerator
        {
            private struct UVEntry
            {
                public Vector2 UV1;
                public Vector2 UV2;
                public Vector2 UV3;
                public Vector2 UV4;
            }

            private static readonly Vector2 quadUV1 = new Vector2(0.0f, 0.0f);
            private static readonly Vector2 quadUV2 = new Vector2(1.0f, 0.0f);
            private static readonly Vector2 quadUV3 = new Vector2(0.0f, 1.0f);
            private static readonly Vector2 quadUV4 = new Vector2(1.0f, 1.0f);

            private static readonly Color32 whiteColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

            private readonly List<int> indices;
            private readonly List<Vector3> vertices;
            private readonly List<Color32> colors;
            private readonly List<Vector3> velocities;
            private readonly List<Vector4> uvs;
            private readonly List<Vector4> lifeTimes;
            private readonly List<Vector4> others;
            private readonly UVEntry[] uvEntries;

            private Mesh mesh;
            private int indice = -1;
            private float fadeTimeDelay;

            private void CalculateUVEntries()
            {

                for (int y = 0; y < Rows; y++)
                {
                    for (int x = 0; x < Columns; x++)
                    {
                        uvEntries[x + (y * Columns)] = new UVEntry
                        {
                            UV1 = new Vector2((float)x / (float)Columns, (float)y / (float)Rows),
                            UV2 = new Vector2((float)(x + 1) / (float)Columns, (float)y / (float)Rows),
                            UV3 = new Vector2((float)x / (float)Columns, (float)(y + 1) / (float)Rows),
                            UV4 = new Vector2((float)(x + 1) / (float)Columns, (float)(y + 1) / (float)Rows)
                        };
                    }
                }
            }

            public CloudGenerator(Mesh mesh, int numberOfClouds, int rows, int columns)
            {
                NumberOfClouds = numberOfClouds;
                Rows = rows;
                Columns = columns;
                uvEntries = new UVEntry[Rows * Columns];
                int vertexCount = numberOfClouds * 4;
                int indicesCount = (int)((float)vertexCount * 1.5f);
                indices = new List<int>(indicesCount);
                vertices = new List<Vector3>(vertexCount);
                colors = new List<Color32>(vertexCount);
                velocities = new List<Vector3>(vertexCount);
                uvs = new List<Vector4>(vertexCount);
                lifeTimes = new List<Vector4>(vertexCount);
                others = new List<Vector4>(vertexCount);
                this.mesh = mesh;
                mesh.Clear();
                CalculateUVEntries();
            }

            public void CreateCloud()
            {
                float cloudRadius = RadiusRange.Random();
                Vector3 pos = UnityEngine.Random.insideUnitSphere * BoundsRadius;
                pos.y = Mathf.Abs(pos.y * 0.5f) + BoundsHeightRange.Random();
                //pos.y = BoundsHeightRange.Random();

                // for debugging with particle alpha blend shader
                //vertices[0] = pos - new Vector3(radius, 0.0f, radius);
                //vertices[1] = pos - new Vector3(-radius, 0.0f, radius);
                //vertices[2] = pos - new Vector3(radius, 0.0f, -radius);
                //vertices[3] = pos - new Vector3(-radius, 0.0f, -radius);

                vertices.Add(pos);
                vertices.Add(pos);
                vertices.Add(pos);
                vertices.Add(pos);

                UVEntry uv = uvEntries[UnityEngine.Random.Range(0, uvEntries.Length)];
                uvs.Add(new Vector4(uv.UV1.x, uv.UV1.y, quadUV1.x, quadUV1.y));
                uvs.Add(new Vector4(uv.UV2.x, uv.UV2.y, quadUV2.x, quadUV2.y));
                uvs.Add(new Vector4(uv.UV3.x, uv.UV3.y, quadUV3.x, quadUV3.y));
                uvs.Add(new Vector4(uv.UV4.x, uv.UV4.y, quadUV4.x, quadUV4.y));

                float fadeTime = FadeTimeRange.Random();
                float totalLifeTime = LifeTimeRange.Random();
                Vector4 lifeTime = new Vector4(Time.timeSinceLevelLoad + fadeTimeDelay, fadeTime, totalLifeTime); // creation time, fade in/out time in seconds, total life time in seconds
                fadeTimeDelay += FadeTimeDelayRange.Random();
                lifeTimes.Add(lifeTime);
                lifeTimes.Add(lifeTime);
                lifeTimes.Add(lifeTime);
                lifeTimes.Add(lifeTime);

                colors.Add(whiteColor);
                colors.Add(whiteColor);
                colors.Add(whiteColor);
                colors.Add(whiteColor);

                float velocityX = VelocityRangeX.Random();
                float velocityZ = VelocityRangeZ.Random();
                Vector3 velocity = new Vector3(velocityX, 0.0f, velocityZ);
                velocities.Add(velocity);
                velocities.Add(velocity);
                velocities.Add(velocity);
                velocities.Add(velocity);

                float startAngle = UnityEngine.Random.Range(-Mathf.PI, Mathf.PI);
                float angularVelocity = AngularVelocityRange.Random();
                others.Add(new Vector4(startAngle, angularVelocity, -cloudRadius, -cloudRadius));
                others.Add(new Vector4(startAngle, angularVelocity, cloudRadius, -cloudRadius));
                others.Add(new Vector4(startAngle, angularVelocity, -cloudRadius, cloudRadius));
                others.Add(new Vector4(startAngle, angularVelocity, cloudRadius, cloudRadius));

                indices.Add(++indice);
                indices.Add(++indice);
                indices.Add(++indice);
                indices.Add(indice--);
                indices.Add(indice--);
                indices.Add(indice += 3);
            }

            public void CreateClouds()
            {
                for (int i = 0; i < NumberOfClouds; i++)
                {
                    CreateCloud();
                }
            }

            public void Apply()
            {
                mesh.SetVertices(vertices);
                mesh.SetUVs(0, uvs);
                mesh.SetUVs(1, lifeTimes);
                mesh.SetColors(colors);
                mesh.SetNormals(velocities);
                mesh.SetTangents(others);
                mesh.SetTriangles(indices, 0);
                ;

                float padding = RadiusRange.Maximum * 1.1f;
                float maximumRadius = BoundsRadius * 10.0f; // clouds can move 10x the radius distance in velocity before going outside the mesh
                Vector3 center = new Vector3(BoundsCenter.x, BoundsCenter.y + ((BoundsHeightRange.Maximum + BoundsHeightRange.Minimum) * 0.5f), BoundsCenter.z);
                Vector3 size = new Vector3(maximumRadius + padding, ((BoundsHeightRange.Maximum - BoundsHeightRange.Minimum) / 2.0f) + padding, maximumRadius + padding);
                mesh.bounds = new Bounds(center, size);
            }

            /// <summary>
            /// Show clouds from a mesh
            /// </summary>
            /// <param name="mesh">Mesh</param>
            /// <param name="lifeTimeRange">Life time range</param>
            /// <param name="fadeTimeRange">Range of fade time for each cloud</param>
            /// <param name="fadeTimeDelayRange">Amount to add to the delay for fading for each subsequent cloud</param>
            public static void ShowClouds(Mesh mesh, RangeOfFloats lifeTimeRange, RangeOfFloats fadeTimeRange, RangeOfFloats fadeTimeDelayRange)
            {
                if (mesh == null)
                {
                    return;
                }

                List<Vector4> lifeTimes = new List<Vector4>(mesh.vertexCount);
                float fadeDelay = 0.0f;

                for (int i = 0; i < mesh.vertexCount / 4; i++)
                {
                    float fadeTime = fadeTimeRange.Random();
                    float totalLifeTime = lifeTimeRange.Random();
                    float creationTime = Time.timeSinceLevelLoad + fadeDelay;
                    Vector4 lifeTime = new Vector4(creationTime, fadeTime, totalLifeTime);
                    fadeDelay += fadeTimeDelayRange.Random();
                    lifeTimes.Add(lifeTime);
                    lifeTimes.Add(lifeTime);
                    lifeTimes.Add(lifeTime);
                    lifeTimes.Add(lifeTime);
                }

                mesh.SetUVs(1, lifeTimes);
            }

            /// <summary>
            /// Hide clouds from a mesh
            /// </summary>
            /// <param name="mesh">Mesh</param>
            /// <param name="fadeTimeRange">Range of fade time for each cloud</param>
            /// <param name="fadeTimeDelayRange">Amount to add to the delay for fading for each subsequent cloud</param>
            public static void HideClouds(Mesh mesh, RangeOfFloats fadeTimeRange, RangeOfFloats fadeTimeDelayRange)
            {
                if (mesh == null)
                {
                    return;
                }

                List<Vector4> lifeTimes = new List<Vector4>(mesh.vertexCount);
                float fadeDelay = 0.0f;
                mesh.GetUVs(1, lifeTimes);

                for (int i = 0; i < lifeTimes.Count; )
                {
                    float fadeTime = fadeTimeRange.Random();
                    Vector4 lifeTime = lifeTimes[i];
                    lifeTime.y = fadeTimeRange.Random();
                    lifeTime.z = (Time.timeSinceLevelLoad - lifeTime.x) + fadeTime + fadeDelay;
                    fadeDelay += fadeTimeDelayRange.Random();
                    lifeTimes[i++] = lifeTimes[i++] = lifeTimes[i++] = lifeTimes[i++] = lifeTime;
                }

                mesh.SetUVs(1, lifeTimes);
            }

            public int Rows { get; private set; }
            public int Columns { get; private set; }

            public int NumberOfClouds { get; set; }
            public float BoundsRadius { get; set; }
            public Vector3 BoundsCenter { get; set; }
            public RangeOfFloats BoundsHeightRange { get; set; }
            public RangeOfFloats FadeTimeRange { get; set; }
            public RangeOfFloats FadeTimeDelayRange { get; set; }
            public RangeOfFloats LifeTimeRange { get; set; }
            public RangeOfFloats RadiusRange { get; set; }
            public RangeOfFloats VelocityRangeX { get; set; }
            public RangeOfFloats VelocityRangeZ { get; set; }
            public RangeOfFloats AngularVelocityRange { get; set; }
        }
        
        private Mesh mesh;

        [Header("3D Clouds")]
        [SingleLine("The range for how long each cloud takes (in seconds) to fade in and out")]
        public RangeOfFloats FadeTimeRange = new RangeOfFloats { Minimum = 1.0f, Maximum = 3.0f };

        [SingleLine("The range in time (seconds) to add to the fade time for each new cloud")]
        public RangeOfFloats FadeTimeDelayRange = new RangeOfFloats { Minimum = 0.001f, Maximum = 0.01f };

        [SingleLine("The range for how long each cloud lives (in seconds) before disappearing")]
        public RangeOfFloats LifeTimeRange = new RangeOfFloats { Minimum = 9999999.0f, Maximum = 9999999.0f };

        [SingleLine("The range in radius for the size of each cloud")]
        public RangeOfFloats RadiusRange = new RangeOfFloats { Minimum = 600.0f, Maximum = 1000.0f };

        [SingleLine("The range of possible x velocity values for each cloud")]
        public RangeOfFloats VelocityRangeX;
        
        [SingleLine("The range of possible z velocity values for each cloud")]
        public RangeOfFloats VelocityRangeZ;

        [SingleLine("The range (in radians per second) of angular velocity for each cloud")]
        public RangeOfFloats AngularVelocityRange = new RangeOfFloats { Minimum = 0.001f, Maximum = 0.005f };

        [Tooltip("The radius of the bounds for creation of clouds")]
        public float BoundsRadius = 4000.0f;

        [Tooltip("The center position to create the clouds at")]
        public Vector3 BoundsCenter = Vector3.zero;

        [SingleLine("The range of possible height values for created clouds. This is added to the hemisphere y value of the random point using the radius.")]
        public RangeOfFloats BoundsHeightRange = new RangeOfFloats { Minimum = 1000.0f, Maximum = 2000.0f };

        [Range(-100000.0f, 100000.0f)]
        [Tooltip("For purposes of billboarding the clouds, pretend that the camera y position is offset by this amount. Clouds more horizontal to the camera get billboarded more.")]
        public float BillboardOffset = -1.0f;

        [Range(1.0f, 100000.0f)]
        [Tooltip("Change how the clouds fade out as they get further away from the camera. Higher values decrease the fade.")]
        public float FadeOffset = 1300.0f;

        [Tooltip("The sun game object, null if none.")]
        public GameObject Sun;

        private MeshFilter meshFilter;

        private void OnDestroy()
        {
            if (mesh != null)
            {
                GameObject.Destroy(mesh);
            }
        }

        private void EnableFlare(bool enable)
        {
            if (Sun == null)
            {
                return;
            }
            LensFlare flare = Sun.GetComponent<LensFlare>();
            if (flare == null)
            {
                return;
            }
            Color startColor = (enable ? Color.black : Color.white);
            Color endColor = (enable ? Color.white : Color.black);
            TweenFactory.Tween("WeatherMakerLensFlare", startColor, endColor, 3.0f, TweenScaleFunctions.Linear, (ITween<Color> c) =>
            {
                flare.color = c.CurrentValue;
            }, null);
        }

        protected override void OnUpdateMaterial(Material m)
        {
            base.OnUpdateMaterial(m);

            m.SetFloat("_BillboardOffset", BillboardOffset);
            m.SetFloat("_FadeOffset", FadeOffset);
        }

        protected override void Start()
        {
            meshFilter = GetComponent<MeshFilter>();
            mesh = new Mesh();
            mesh.name = "WeatherMakerClouds";
            meshFilter.sharedMesh = mesh;
            base.Start();
        }

        protected override void Update()
        {
            base.Update();
        }

        /// <summary>
        /// Start the clouds appearing
        /// </summary>
        public override void CreateClouds()
        {
            if (mesh.vertexCount == 0)
            {
                CloudGenerator cloudGenerator = new CloudGenerator(mesh, NumberOfClouds, MaterialRows, MaterialColumns);
                cloudGenerator.AngularVelocityRange = AngularVelocityRange;
                cloudGenerator.BoundsRadius = BoundsRadius;
                cloudGenerator.BoundsCenter = BoundsCenter;
                cloudGenerator.BoundsHeightRange = BoundsHeightRange;
                cloudGenerator.FadeTimeRange = FadeTimeRange;
                cloudGenerator.FadeTimeDelayRange = FadeTimeDelayRange;
                cloudGenerator.LifeTimeRange = LifeTimeRange;
                cloudGenerator.RadiusRange = RadiusRange;
                cloudGenerator.VelocityRangeX = VelocityRangeX;
                cloudGenerator.VelocityRangeZ = VelocityRangeZ;
                cloudGenerator.CreateClouds();                
                cloudGenerator.Apply();
            }
            else
            {
                CloudGenerator.ShowClouds(mesh, LifeTimeRange, FadeTimeRange, FadeTimeDelayRange);
            }
            EnableFlare(false);
        }

        /// <summary>
        /// Start the clouds disappearing
        /// </summary>
        public override void RemoveClouds()
        {
            CloudGenerator.HideClouds(mesh, FadeTimeRange, FadeTimeDelayRange);
            EnableFlare(true);
        }

        /// <summary>
        /// Reset the mesh so that a new set of clouds that look different can be created
        /// </summary>
        public override void Reset()
        {
            mesh.Clear();
        }
    }
}