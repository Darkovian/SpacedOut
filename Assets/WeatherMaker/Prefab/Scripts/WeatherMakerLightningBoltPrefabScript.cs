//
// Weather Maker for Unity
// (c) 2016 Digital Ruby, LLC
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace DigitalRuby.WeatherMaker
{
    public abstract class WeatherMakerLightningBoltPrefabScriptBase : WeatherMakerLightningBoltScript
    {

#if DEBUG

        private static bool showedManualWarning;

#endif

        private static readonly System.Random random = new System.Random();
        private readonly List<LightningBoltParameters> batchParameters = new List<LightningBoltParameters>();

        [Header("Lightning Spawn Properties")]
        [SingleLineClamp("How long to wait before creating another round of lightning bolts in seconds", 0.001, double.MaxValue)]
        public RangeOfFloats IntervalRange = new RangeOfFloats { Minimum = 0.05f, Maximum = 0.1f };

        [SingleLineClamp("How many lightning bolts to emit for each interval", 0.0, 100.0)]
        public RangeOfIntegers CountRange = new RangeOfIntegers { Minimum = 1, Maximum = 1 };

        [Tooltip("Reduces the probability that additional bolts from CountRange will actually happen (0 - 1).")]
		[Range(0.0f, 1.0f)]
        public float CountProbabilityModifier = 1.0f;

        [SingleLineClamp("Delay in seconds (range) before each additional lightning bolt in count range is emitted", 0.0f, 30.0f)]
        public RangeOfFloats DelayRange = new RangeOfFloats { Minimum = 0.0f, Maximum = 0.0f };

        [SingleLineClamp("For each bolt emitted, how long should it stay in seconds", 0.01, 10.0)]
        public RangeOfFloats DurationRange = new RangeOfFloats { Minimum = 0.06f, Maximum = 0.12f };

        [Header("Lightning Appearance Properties")]
        [SingleLineClamp("The trunk width range in unity units (x = min, y = max)", 0.0001, 100.0)]
        public RangeOfFloats TrunkWidthRange = new RangeOfFloats { Minimum = 0.1f, Maximum = 0.2f };

        [Tooltip("How long (in seconds) this game object should live before destroying itself. Leave as 0 for infinite.")]
        [Range(0.0f, 1000.0f)]
        public float LifeTime = 0.0f;

        [Tooltip("Generations (1 - 8, higher makes more detailed but more expensive lightning)")]
        [Range(1, 8)]
        public int Generations = 6;

        [Tooltip("The chaos factor determines how far the lightning can spread out, higher numbers spread out more. 0 - 1.")]
        [Range(0.0f, 1.0f)]
        public float ChaosFactor = 0.075f;

        [Tooltip("The intensity of the glow, 0 - 1")]
        [Range(0.0f, 1.0f)]
        public float GlowIntensity = 0.1f;

        [Tooltip("The width multiplier for the glow, 0 - 64")]
        [Range(0.0f, 64.0f)]
        public float GlowWidthMultiplier = 4.0f;

        [Tooltip("How forked should the lightning be? (0 - 1, 0 for none, 1 for lots of forks)")]
        [Range(0.0f, 1.0f)]
        public float Forkedness = 0.25f;

        [Range(0.0f, 10.0f)]
        [Tooltip("Minimum distance multiplier for forks")]
        public float ForkLengthMultiplier = 0.6f;

        [Range(0.0f, 10.0f)]
        [Tooltip("Fork distance multiplier variance. Random range of 0 to n that is added to Fork Length Multiplier.")]
        public float ForkLengthVariance = 0.2f;

        [Tooltip("What percent of time the lightning should fade in and out. For example, 0.15 fades in 15% of the time and fades out 15% of the time, with full visibility 70% of the time.")]
        [Range(0.0f, 0.5f)]
        public float FadePercent = 0.15f;

        [Tooltip("0 - 1, how slowly the lightning should grow. 0 for instant, 1 for slow.")]
        [Range(0.0f, 1.0f)]
        public float GrowthMultiplier;

        [Tooltip("How much smaller the lightning should get as it goes towards the end of the bolt. For example, 0.5 will make the end 50% the width of the start.")]
        [Range(0.0f, 10.0f)]
        public float EndWidthMultiplier = 0.5f;

        [Tooltip("Forks have their EndWidthMultiplier multiplied by this value")]
        [Range(0.0f, 10.0f)]
        public float ForkEndWidthMultiplier = 1.0f;

        [Header("Lightning Light Properties")]
        [Tooltip("Light parameters")]
        public LightningLightParameters LightParameters;

        [Tooltip("Maximum number of lights that can be created per batch of lightning")]
        [Range(0, 64)]
        public int MaximumLightsPerBatch = 8;

        [Header("Lightning Trigger Type")]
        [Tooltip("Manual or automatic mode. Manual requires that you call the Trigger method in script. Automatic uses the interval to create lightning continuously.")]
        public bool ManualMode;

        private float nextArc;
        private float lifeTimeRemaining;

        private void CreateInterval(float offset)
        {
            nextArc = offset + ((float)random.NextDouble() * (IntervalRange.Maximum - IntervalRange.Minimum)) + IntervalRange.Minimum;
        }

        private void CallLightning()
        {
            CallLightning(null, null);
        }

        private void CallLightning(Vector3? start, Vector3? end)
        {
            int count = random.Next(CountRange.Minimum, CountRange.Maximum + 1);
            for (int i = 0; i < count; i++)
            {
                LightningBoltParameters p = CreateParameters();
                if (CountProbabilityModifier == 1.0f || i == 0 || (float)p.Random.NextDouble() <= CountProbabilityModifier)
                {
                    CreateLightningBolt(p);
					if (start != null)
                    {
                        p.Start = start.Value;
                    }
                    if (end != null)
                    {
                        p.End = end.Value;
                    }
                }
            }
            CreateLightningBoltsNow();
        }

        protected void CreateLightningBoltsNow()
        {
            int tmp = LightningBolt.MaximumLightsPerBatch;
            LightningBolt.MaximumLightsPerBatch = MaximumLightsPerBatch;
            CreateLightningBolts(batchParameters);
            LightningBolt.MaximumLightsPerBatch = tmp;
            batchParameters.Clear();
        }

        protected void PopulateParameters(LightningBoltParameters p)
        {
            float duration = ((float)random.NextDouble() * (DurationRange.Maximum - DurationRange.Minimum)) + DurationRange.Maximum;
            float trunkWidth = ((float)random.NextDouble() * (TrunkWidthRange.Maximum - TrunkWidthRange.Minimum)) + TrunkWidthRange.Maximum;

            p.Generations = Generations;
            p.LifeTime = duration;
            p.ChaosFactor = ChaosFactor;
            p.TrunkWidth = trunkWidth;
            p.GlowIntensity = GlowIntensity;
            p.GlowWidthMultiplier = GlowWidthMultiplier;
            p.Forkedness = Forkedness;
            p.ForkLengthMultiplier = ForkLengthMultiplier;
            p.ForkLengthVariance = ForkLengthVariance;
            p.FadePercent = FadePercent;
            p.GrowthMultiplier = GrowthMultiplier;
            p.EndWidthMultiplier = EndWidthMultiplier;
            p.ForkEndWidthMultiplier = ForkEndWidthMultiplier;
            p.Random = random;
            p.DelayRange = DelayRange;
            p.LightParameters = LightParameters;
        }

        protected virtual LightningBoltParameters OnCreateParameters()
        {
            return new LightningBoltParameters();
        }

        protected LightningBoltParameters CreateParameters()
        {
            LightningBoltParameters p = OnCreateParameters();
            p.quality = QualitySetting;
            PopulateParameters(p);

            return p;
        }

        protected override void Start()
        {
            base.Start();
            CreateInterval(0.0f);
            lifeTimeRemaining = (LifeTime <= 0.0f ? float.MaxValue : LifeTime);
        }

        protected override void Update()
        {
            base.Update();

            if ((lifeTimeRemaining -= Time.deltaTime) < 0.0f)
            {
                GameObject.Destroy(gameObject);
            }
            else if ((nextArc -= Time.deltaTime) < 0)
            {
                CreateInterval(nextArc);
                if (ManualMode)
                {

#if DEBUG

                    if (!showedManualWarning)
                    {
                        showedManualWarning = true;
                        Debug.LogWarning("Lightning bolt script is in manual mode. Trigger method must be called.");
                    }

#endif

                }
                else
                {
                    CallLightning();
                }
            }
        }

#if UNITY_EDITOR

        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            UnityEditor.Handles.color = Color.white;
        }

#endif

        /// <summary>
        /// Derived classes can override and can call this base class method last to add the lightning bolt parameters to the list of batched lightning bolts
        /// </summary>
        /// <param name="p">Lightning bolt creation parameters</param>
        public override void CreateLightningBolt(LightningBoltParameters p)
        {
            batchParameters.Add(p);
            // do not call the base method, we batch up and use CreateLightningBolts
        }

        /// <summary>
        /// Manually trigger lightning
        /// </summary>
        public void Trigger()
        {
            CallLightning();
        }

        /// <summary>
        /// Manually trigger lightning
        /// </summary>
        /// <param name="start">Start position</param>
        /// <param name="end">End position</param>
        public void Trigger(Vector3? start, Vector3? end)
        {
            CallLightning(start, end);
        }
    }

    public class WeatherMakerLightningBoltPrefabScript : WeatherMakerLightningBoltPrefabScriptBase
    {
        [Header("Start/end")]
        [Tooltip("The source game object, can be null")]
        public GameObject Source;

        [Tooltip("The destination game object, can be null")]
        public GameObject Destination;

        [Tooltip("X, Y and Z for variance from the start point. Use positive values.")]
        public Vector3 StartVariance;

        [Tooltip("X, Y and Z for variance from the end point. Use positive values.")]
        public Vector3 EndVariance;

#if UNITY_EDITOR

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            if (Source != null)
            {
                Gizmos.DrawIcon(Source.transform.position, "LightningPathStart.png");
            }
            if (Destination != null)
            {
                Gizmos.DrawIcon(Destination.transform.position, "LightningPathNext.png");
            }
            if (Source != null && Destination != null)
            {
                Gizmos.DrawLine(Source.transform.position, Destination.transform.position);
                Vector3 direction = (Destination.transform.position - Source.transform.position);
                Vector3 center = (Source.transform.position + Destination.transform.position) * 0.5f;
                float arrowSize = Mathf.Min(2.0f, direction.magnitude);
                UnityEditor.Handles.ArrowCap(0, center, Quaternion.LookRotation(direction), arrowSize);
            }
        }

#endif

        public override void CreateLightningBolt(LightningBoltParameters parameters)
        {
            parameters.Start = (Source == null ? parameters.Start : Source.transform.position);
            parameters.End = (Destination == null ? parameters.End : Destination.transform.position);
            parameters.StartVariance = StartVariance;
            parameters.EndVariance = EndVariance;

            base.CreateLightningBolt(parameters);
        }
    }
}
