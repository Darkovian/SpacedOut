//
// Weather Maker for Unity
// (c) 2016 Digital Ruby, LLC
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 

using UnityEngine;
using System.Collections;
using System;

namespace DigitalRuby.WeatherMaker
{
    public class WeatherMakerThunderAndLightningScript2D : WeatherMakerThunderAndLightningScript
    {
        private void CalculateVisibleBounds(out Vector3 visibleMin, out Vector3 visibleMax, out float width, out float height)
        {
            visibleMin = Camera.ViewportToWorldPoint(Vector3.zero);
            visibleMax = Camera.ViewportToWorldPoint(Vector3.one);
            width = (visibleMax.x - visibleMin.x);
            height = (visibleMax.y - visibleMin.y);
        }

        protected override Vector3 CalculateStartPosition(ref Vector3 anchorPosition, Camera visibleInCamera, bool intense)
        {
            Vector3 visibleMin, visibleMax;
            float width, height;
            CalculateVisibleBounds(out visibleMin, out visibleMax, out width, out height);
            Vector3 start = new Vector3(visibleMin.x + (width * UnityEngine.Random.Range(0.2f, 0.8f)), visibleMin.y + (height * StartYBase.Random()), 0.0f);
            start.x += (width * StartXVariance.Random());
            start.y += (height * StartYVariance.Random());

            return start;
        }

        protected override Vector3 CalculateEndPosition(ref Vector3 anchorPosition, ref Vector3 start, bool intense)
        {
            Vector3 dir = (start + UnityEngine.Random.insideUnitSphere).normalized;
            dir.y = -Mathf.Abs(dir.y);
            dir.z = 0.0f;

            RaycastHit2D h;
            if (UnityEngine.Random.Range(0.0f, 1.0f) >= GroundLightningChance ||
                ((h = Physics2D.Raycast(start, dir)).collider == null))
            {
                Vector3 visibleMin, visibleMax;
                float width, height;
                CalculateVisibleBounds(out visibleMin, out visibleMax, out width, out height);
                float maxDimen = Mathf.Max(width, height);
                float variance = Mathf.Max(maxDimen * 0.25f, maxDimen * UnityEngine.Random.Range(EndYVariance.Minimum, EndYVariance.Maximum));
                Vector3 end = start + (dir * variance);
                end.x += (width * EndXVariance.Random());
                end.y += (height * EndYVariance.Random());
                end.z = 0.0f;

                return end;
            }

            return h.point;
        }

        protected override void Start()
        {
            base.Start();
        }

        protected override void Update()
        {
            base.Update();
        }
    }
}