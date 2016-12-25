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
    public class WeatherMakerThunderAndLightningScript3D : WeatherMakerThunderAndLightningScript
    {
        [Header("3D settings")]
        [SingleLine("Range of distances away from the camera that normal lightning can be")]
        public RangeOfFloats NormalDistance = new RangeOfFloats { Minimum = 3000.0f, Maximum = 5000.0f };

        [SingleLine("Range of distances away from the camera that intense lightning can be")]
        public RangeOfFloats IntenseDistance = new RangeOfFloats { Minimum = 500.0f, Maximum = 1000.0f };

        protected override Vector3 CalculateStartPosition(ref Vector3 anchorPosition, Camera visibleInCamera, bool intense)
        {
            Vector3 start = anchorPosition;
            Vector3 randomDir;

            if (visibleInCamera == null)
            {
                randomDir = UnityEngine.Random.onUnitSphere;
            }
            else
            {
                Vector3 randomScreenPoint = new Vector3
                (
                    UnityEngine.Random.Range((float)Screen.width * 0.2f, (float)Screen.width * 0.8f),
                    UnityEngine.Random.Range((float)Screen.height * 0.2f, (float)Screen.height * 0.8f),
                    visibleInCamera.farClipPlane * 0.5f
                );
                randomDir = visibleInCamera.ScreenToWorldPoint(randomScreenPoint).normalized;
            }
            randomDir.y = Mathf.Abs(randomDir.y);
            randomDir *= (intense ? IntenseDistance.Random() : NormalDistance.Random());
            randomDir.y = StartYBase.Random();
            start += randomDir;
            start.x += StartXVariance.Random();
            start.y += StartYVariance.Random();
            start.z += StartZVariance.Random();

            // if the start is too close to the anchor point, push it back
            float minDistance = (intense ? IntenseDistance.Minimum : NormalDistance.Minimum);
            if (Vector3.Distance(start, anchorPosition) < minDistance)
            {
                Vector3 startDir = (start - anchorPosition).normalized;
                start = anchorPosition + (startDir * minDistance);
            }

            return start;
        }

        protected override Vector3 CalculateEndPosition(ref Vector3 anchorPosition, ref Vector3 start, bool intense)
        {
            Vector3 end;

            // determine if we should strike the ground
            if (UnityEngine.Random.Range(0.0f, 1.0f) > GroundLightningChance)
            {
                end.y = start.y + EndYVariance.Random();
            }
            else
            {
                end.y = -100.0f;
            }

            end.x = start.x + EndXVariance.Random();
            end.z = start.z + EndZVariance.Random();

            // see if the bolt hit anything on it's way to the ground - if so, change the end point
            RaycastHit hit;
            if (Physics.Raycast(start, (start - end).normalized, out hit, float.MaxValue))
            {
                end = hit.point;
            }

            // if the end is too close to the anchor point, push it back
            float minDistance = (intense ? IntenseDistance.Minimum : NormalDistance.Minimum);
            if (Vector3.Distance(anchorPosition, end) < minDistance)
            {
                Vector3 endDir = (end - anchorPosition).normalized;
                end = anchorPosition + (endDir * minDistance);
            }

            return end;
        }

        protected override void Start()
        {
            base.Start();

#if DEBUG

            if (Camera.farClipPlane < 10000.0f && !Camera.orthographic)
            {
                Debug.LogWarning("Far clip plane should be 10000+ for best lightning effects");
            }

#endif

        }

        protected override void Update()
        {
            base.Update();
        }
    }
}