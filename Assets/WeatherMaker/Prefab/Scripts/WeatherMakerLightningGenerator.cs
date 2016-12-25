//
// Weather Maker for Unity
// (c) 2016 Digital Ruby, LLC
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Lightning bolt parameters that contain a preset list of points
    /// </summary>
    public class LightningBoltPathParameters : LightningBoltParameters
    {
        /// <summary>
        /// Points for the trunk to follow
        /// </summary>
        public List<Vector3> Points { get; set; }

        /// <summary>
        /// The amount of smoothing applied. For example, if there were 4 original points and smoothing / spline created 32 points, this value would be 8
        /// </summary>
        public int SmoothingFactor;
    }

    public class LightningGenerator
    {
        private static readonly List<LightningBoltSegmentGroup> groupCache = new List<LightningBoltSegmentGroup>();

        protected LightningBolt CurrentBolt { get; private set; }

        private void GetPerpendicularVector(ref Vector3 directionNormalized, out Vector3 side)
        {
            if (directionNormalized == Vector3.zero)
            {
                side = Vector3.right;
            }
            else
            {
                // use cross product to find any perpendicular vector around directionNormalized:
                // 0 = x * px + y * py + z * pz
                // => pz = -(x * px + y * py) / z
                // for computational stability use the component farthest from 0 to divide by
                float x = directionNormalized.x;
                float y = directionNormalized.y;
                float z = directionNormalized.z;
                float px, py, pz;
                float ax = Mathf.Abs(x), ay = Mathf.Abs(y), az = Mathf.Abs(z);
                if (ax >= ay && ay >= az)
                {
                    // x is the max, so we can pick (py, pz) arbitrarily at (1, 1):
                    py = 1.0f;
                    pz = 1.0f;
                    px = -(y * py + z * pz) / x;
                }
                else if (ay >= az)
                {
                    // y is the max, so we can pick (px, pz) arbitrarily at (1, 1):
                    px = 1.0f;
                    pz = 1.0f;
                    py = -(x * px + z * pz) / y;
                }
                else
                {
                    // z is the max, so we can pick (px, py) arbitrarily at (1, 1):
                    px = 1.0f;
                    py = 1.0f;
                    pz = -(x * px + y * py) / z;
                }
                side = new Vector3(px, py, pz).normalized;
            }
        }

        protected virtual void OnGenerateLightningBolt(Vector3 start, Vector3 end, LightningBoltParameters p)
        {
            GenerateLightningBoltStandard(start, end, p.Generations, p.Generations, 0.0f, p);
        }

        public bool ShouldCreateFork(LightningBoltParameters p, int generation, int totalGenerations)
        {
            return (generation > p.generationWhereForksStop && generation >= totalGenerations - p.forkednessCalculated && (float)p.Random.NextDouble() < p.Forkedness);
        }

        public void CreateFork(LightningBoltParameters p, int generation, int totalGenerations, Vector3 start, Vector3 midPoint)
        {
            if (ShouldCreateFork(p, generation, totalGenerations))
            {
                Vector3 branchVector = (midPoint - start) * p.ForkMultiplier();
                Vector3 splitEnd = midPoint + branchVector;
                GenerateLightningBoltStandard(midPoint, splitEnd, generation, totalGenerations, 0.0f, p);
            }
        }

        public void GenerateLightningBoltStandard(Vector3 start, Vector3 end, int generation, int totalGenerations, float offsetAmount, LightningBoltParameters p)
        {
            if (generation < 1)
            {
                return;
            }

            LightningBoltSegmentGroup group = CreateGroup();
            group.Segments.Add(new LightningBoltSegment { Start = start, End = end });

            // every generation, get the percentage we have gone down and square it, this makes lines thinner
            float widthMultiplier = (float)generation / (float)totalGenerations;
            widthMultiplier *= widthMultiplier;

            Vector3 randomVector;
            group.LineWidth = p.TrunkWidth * widthMultiplier;
            group.Generation = generation;
            group.Color = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, (byte)(255.0f * widthMultiplier));
            group.EndWidthMultiplier = p.EndWidthMultiplier * p.ForkEndWidthMultiplier;
            if (offsetAmount <= 0.0f)
            {
                offsetAmount = (end - start).magnitude * p.ChaosFactor;
            }

            while (generation-- > 0)
            {
                int previousStartIndex = group.StartIndex;
                group.StartIndex = group.Segments.Count;
                for (int i = previousStartIndex; i < group.StartIndex; i++)
                {
                    start = group.Segments[i].Start;
                    end = group.Segments[i].End;

                    // determine a new direction for the split
                    Vector3 midPoint = (start + end) * 0.5f;

                    // adjust the mid point to be the new location
                    RandomVector(ref start, ref end, offsetAmount, p.Random, out randomVector);
                    midPoint += randomVector;

                    // add two new segments
                    group.Segments.Add(new LightningBoltSegment { Start = start, End = midPoint });
                    group.Segments.Add(new LightningBoltSegment { Start = midPoint, End = end });

                    CreateFork(p, generation, totalGenerations, start, midPoint);
                }

                // halve the distance the lightning can deviate for each generation down
                offsetAmount *= 0.5f;
            }
        }

        public LightningBoltSegmentGroup CreateGroup()
        {
            LightningBoltSegmentGroup g;
            if (groupCache.Count == 0)
            {
                g = new LightningBoltSegmentGroup();
            }
            else
            {
                int index = groupCache.Count - 1;
                g = groupCache[index];
                groupCache.RemoveAt(index);
            }
            CurrentBolt.AddGroup(g);
            return g;
        }

        public static void ReturnGroupToCache(LightningBoltSegmentGroup group)
        {
            groupCache.Add(group);
        }

        public Vector3 RandomDirection3D(System.Random r)
        {
            float z = (2.0f * (float)r.NextDouble()) - 1.0f; // z is in the range [-1,1]
            Vector3 planar = RandomDirection2D(r) * Mathf.Sqrt(1.0f - (z * z));
            planar.z = z;

            return planar;
        }

        public Vector3 RandomDirection2D(System.Random r)
        {
            float azimuth = (float)r.NextDouble() * 2.0f * Mathf.PI;
            return new Vector3(Mathf.Cos(azimuth), Mathf.Sin(azimuth), 0.0f);
        }

        public Vector3 RandomDirection2DXZ(System.Random r)
        {
            float azimuth = (float)r.NextDouble() * 2.0f * Mathf.PI;
            return new Vector3(Mathf.Cos(azimuth), 0.0f, Mathf.Sin(azimuth));
        }

        public void RandomVector(ref Vector3 start, ref Vector3 end, float offsetAmount, System.Random random, out Vector3 result)
        {
            if (CurrentBolt.Script.calculatedCameraMode == CameraMode.Perspective)
            {
                Vector3 directionNormalized = (end - start).normalized;
                Vector3 side;
                GetPerpendicularVector(ref directionNormalized, out side);

                // generate random distance
                float distance = (((float)random.NextDouble() + 0.1f) * offsetAmount);

                // get random rotation angle to rotate around the current direction
                float rotationAngle = ((float)random.NextDouble() * 360.0f);

                // rotate around the direction and then offset by the perpendicular vector
                result = Quaternion.AngleAxis(rotationAngle, directionNormalized) * side * distance;
            }
            else if (CurrentBolt.Script.calculatedCameraMode == CameraMode.OrthographicXY)
            {
                // XY plane
                end.z = start.z;
                Vector3 directionNormalized = (end - start).normalized;
                Vector3 side = new Vector3(-directionNormalized.y, directionNormalized.x, 0.0f);
                float distance = ((float)random.NextDouble() * offsetAmount * 2.0f) - offsetAmount;
                result = side * distance;
            }
            else
            {
                // XZ plane
                end.y = start.y;
                Vector3 directionNormalized = (end - start).normalized;
                Vector3 side = new Vector3(-directionNormalized.z, 0.0f, directionNormalized.x);
                float distance = ((float)random.NextDouble() * offsetAmount * 2.0f) - offsetAmount;
                result = side * distance;
            }
        }

        public void GenerateLightningBolt(LightningBoltParameters p, LightningBolt bolt)
        {
            CurrentBolt = bolt;

            Vector3 start = p.ApplyVariance(p.Start, p.StartVariance);
            Vector3 end = p.ApplyVariance(p.End, p.EndVariance);

            OnGenerateLightningBolt(start, end, p);

            CurrentBolt = null;
        }

        public static readonly LightningGenerator GeneratorInstance = new LightningGenerator();
    }

    /// <summary>
    /// Generates lightning that follows a path
    /// </summary>
    public class LightningGeneratorPath : LightningGenerator
    {
        public static readonly LightningGeneratorPath PathInstance = new LightningGeneratorPath();

        public void GenerateLightningBoltPath(Vector3 start, Vector3 end, LightningBoltPathParameters p)
        {
            if (p.Points.Count < 2)
            {
                Debug.LogError("Lightning path should have at least two points");
                return;
            }

            int generation = p.Generations;
            int totalGenerations = generation;
            float offsetAmount, d;
            int smoothingFactor = p.SmoothingFactor - 1;
            Vector3 distance, randomVector;
            LightningBoltSegmentGroup group = p.Generator.CreateGroup();
            group.LineWidth = p.TrunkWidth;
            group.Generation = generation--;
            group.EndWidthMultiplier = p.EndWidthMultiplier;
            group.Color = Color.white;

            p.Start = p.Points[0] + start;
            p.End = p.Points[p.Points.Count - 1] + end;
            end = p.Start;

            for (int i = 1; i < p.Points.Count; i++)
            {
                start = end;
                end = p.Points[i];
                distance = (end - start);
                d = distance.magnitude;
                if (p.ChaosFactor > 0.0f)
                {
                    if (CurrentBolt.Script.calculatedCameraMode == CameraMode.Perspective)
                    {
                        end += (d * p.ChaosFactor * RandomDirection3D(p.Random));
                    }
                    else if (CurrentBolt.Script.calculatedCameraMode == CameraMode.OrthographicXY)
                    {
                        end += (d * p.ChaosFactor * RandomDirection2D(p.Random));
                    }
                    else
                    {
                        end += (d * p.ChaosFactor * RandomDirection2DXZ(p.Random));
                    }
                    distance = (end - start);
                }
                group.Segments.Add(new LightningBoltSegment { Start = start, End = end });

                offsetAmount = d * p.ChaosFactor;
                RandomVector(ref start, ref end, offsetAmount, p.Random, out randomVector);

                if (ShouldCreateFork(p, generation, totalGenerations))
                {
                    Vector3 branchVector = distance * p.ForkMultiplier() * smoothingFactor * 0.5f;
                    Vector3 forkEnd = end + branchVector + randomVector;
                    GenerateLightningBoltStandard(start, forkEnd, generation, totalGenerations, 0.0f, p);
                }

                if (--smoothingFactor == 0)
                {
                    smoothingFactor = p.SmoothingFactor - 1;
                }
            }
        }

        protected override void OnGenerateLightningBolt(Vector3 start, Vector3 end, LightningBoltParameters p)
        {
            GenerateLightningBoltPath(start, end, p as LightningBoltPathParameters);
        }
    }
}
