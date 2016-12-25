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
    public abstract class WeatherMakerThunderAndLightningScript : MonoBehaviour
    {
        [Header("Script and Camera")]
        [Tooltip("Lightning bolt script")]
        public WeatherMakerLightningBoltPrefabScript LightningBoltScript;

        [Tooltip("Lightning is created around this camera and this camera is also used if lightning is forced visible. Defaults to the main camera.")]
        public Camera Camera;

        [Header("Timing")]
        [SingleLine("Random interval between strikes.")]
        public RangeOfFloats LightningIntervalTimeRange = new RangeOfFloats { Minimum = 10.0f, Maximum = 25.0f };

        [Header("Intensity")]
        [Tooltip("Probability (0-1) of an intense lightning bolt that hits really close. Intense lightning has increased brightness and louder thunder compared to normal lightning, and the thunder sounds plays a lot sooner.")]
        [Range(0.0f, 1.0f)]
        public float LightningIntenseProbability = 0.2f;

        [Header("Audio")]
        [Tooltip("Sounds to play for normal thunder. One will be chosen at random for each lightning strike. Depending on intensity, some normal lightning may not play a thunder sound.")]
        public AudioClip[] ThunderSoundsNormal;

        [Tooltip("Sounds to play for intense thunder. One will be chosen at random for each lightning strike.")]
        public AudioClip[] ThunderSoundsIntense;

        [Header("Positioning")]
        [SingleLine("Starting y value for the lightning strikes. Can be absolute or percentage of visible scene height depending on 2D or 3D mode.")]
        public RangeOfFloats StartYBase = new RangeOfFloats { Minimum = 500.0f, Maximum = 600.0f };

        [SingleLine("The variance of the end point in the x direction. Can be absolute or percentage depending on 2D or 3D mode.")]
        public RangeOfFloats StartXVariance = new RangeOfFloats { Minimum = -500.0f, Maximum = 500.0f };

        [SingleLine("The variance of the end point in the y direction. Does not get applied if the lightning hit the ground. Can be absolute or percentage depending on 2D or 3D mode.")]
        public RangeOfFloats StartYVariance = new RangeOfFloats { Minimum = -500.0f, Maximum = 0.0f };

        [SingleLine("The variance of the end point in the z direction. Can be absolute or percentage depending on 2D or 3D mode.")]
        public RangeOfFloats StartZVariance = new RangeOfFloats { Minimum = -500.0f, Maximum = 500.0f };

        [SingleLine("The variance of the end point in the x direction. Can be absolute or percentage depending on 2D or 3D mode.")]
        public RangeOfFloats EndXVariance = new RangeOfFloats { Minimum = -500.0f, Maximum = 500.0f };

        [SingleLine("The variance of the end point in the y direction. Does not get applied if the lightning hit the ground. Can be absolute or percentage depending on 2D or 3D mode.")]
        public RangeOfFloats EndYVariance = new RangeOfFloats { Minimum = -500.0f, Maximum = 0.0f };

        [SingleLine("The variance of the end point in the z direction. Can be absolute or percentage depending on 2D or 3D mode.")]
        public RangeOfFloats EndZVariance = new RangeOfFloats { Minimum = -500.0f, Maximum = 500.0f };

        [Tooltip("Probability that lightning strikes will be forced to be visible in the camera view. Even if this fails, there is still " +
            " a change that the lightning will be visible. Ignored for some modes such as 2D.")]
        [Range(0.0f, 1.0f)]
        public float LightningForcedVisibilityProbability = 0.5f;

        [Tooltip("Change that non-cloud lightning will hit the ground")]
        [Range(0.0f, 1.0f)]
        public float GroundLightningChance = 0.3f;

        [Tooltip("The chance lightning will simply be in the clouds with no visible bolt")]
        [Range(0.0f, 1.0f)]
        public float CloudLightningChance = 0.5f;

        [Tooltip("Will deminish the brightness of lightning during the day")]
        public Light Sun;

        [Tooltip("Volue modifier for thunder")]
        [System.NonSerialized]
        public float VolumeModifier = 1.0f;

        protected float NextLightningTime { get; private set; }
        protected bool LightningInProgress { get; private set; }
        protected AudioClip LastThunderSound { get; private set; }
        protected AudioSource AudioSourceThunder { get; private set; }

        private void CalculateNextLightningTime()
        {
            NextLightningTime = Time.time + LightningIntervalTimeRange.Random();
            LightningInProgress = false;
        }

        private void CheckForLightning()
        {
            // time for another strike?
            float v = (Time.time - NextLightningTime);
            if (v >= 0.0f)
            {
                if (v < 1.0f)
                {
                    StartCoroutine(ProcessLightning(null, null));
                }
                else
                {
                    CalculateNextLightningTime();
                }
            }
        }

        public IEnumerator ProcessLightning(Vector3? start, Vector3? end, bool? _intense = null, bool? _forceVisible = null)
        {
            bool intense = _intense ?? (UnityEngine.Random.Range(0.0f, 1.0f) <= LightningIntenseProbability);
            bool forceVisible = _forceVisible ?? (UnityEngine.Random.Range(0.0f, 1.0f) <= LightningForcedVisibilityProbability);
            float sleepTime;
            AudioClip[] sounds;
            float intensity;
            LightningInProgress = true;

            if (intense)
            {
                float percent = UnityEngine.Random.Range(0.0f, 1.0f);
                intensity = Mathf.Lerp(2.0f, 8.0f, percent);
                sleepTime = 5.0f / intensity;
                sounds = ThunderSoundsIntense;
            }
            else
            {
                float percent = UnityEngine.Random.Range(0.0f, 1.0f);
                intensity = Mathf.Lerp(0.0f, 2.0f, percent);
                sleepTime = 30.0f / intensity;
                sounds = ThunderSoundsNormal;
            }

            // perform the strike
            Strike(start, end, intense, intensity, Camera, forceVisible);

            // calculate the next lightning strike
            CalculateNextLightningTime();

            // thunder will play depending on intensity of lightning
            bool playThunder = (intensity >= 1.0f);

            //Debug.Log("Lightning intensity: " + intensity.ToString("0.00") + ", thunder delay: " +
            //          (playThunder ? sleepTime.ToString("0.00") : "No Thunder"));

            if (playThunder && sounds != null && sounds.Length != 0)
            {
                // wait for a bit then play a thunder sound
                yield return new WaitForSeconds(sleepTime);

                AudioClip clip = null;
                do
                {
                    // pick a random thunder sound that wasn't the same as the last sound, unless there is only one sound, then we have no choice
                    clip = sounds[UnityEngine.Random.Range(0, sounds.Length - 1)];
                }
                while (sounds.Length > 1 && clip == LastThunderSound);

                // set the last sound and play it
                LastThunderSound = clip;
                AudioSourceThunder.PlayOneShot(clip, intensity * 0.5f * VolumeModifier);
            }
        }

        private void Strike(Vector3? _start, Vector3? _end, bool intense, float intensity, Camera camera, bool forceVisible)
        {
            Vector3 anchorPosition = camera.transform.position;
            Vector3 start = _start ?? CalculateStartPosition(ref anchorPosition, (forceVisible ? camera : null), intense);
            Vector3 end = _end ?? CalculateEndPosition(ref anchorPosition, ref start, intense);

            // save the generations and trunk width in case of cloud only lightning which will modify these properties
            int generations = LightningBoltScript.Generations;
            float intensityModifier;
            if (Sun == null)
            {
                intensityModifier = 0.35f;
            }
            else if (Camera.orthographic)
            {
                float sunX = (Sun.transform.eulerAngles.x + 180.0f);
                sunX = (sunX >= 360.0f ? sunX - 360.0f : sunX);
                sunX = Mathf.Abs((sunX * 0.5f) - 90.0f);
                intensityModifier = Mathf.Lerp(0.1f, 0.75f, sunX * 0.016f);
            }
            else
            {
                float sunX = (Sun.transform.eulerAngles.x + 90.0f);
                sunX = (sunX >= 360.0f ? sunX - 360.0f : sunX);
                sunX = Mathf.Abs((sunX * 0.5f) - 90.0f);
                intensityModifier = Mathf.Lerp(0.1f, 0.75f, sunX * 0.006f);
            }
            RangeOfFloats trunkWidth = LightningBoltScript.TrunkWidthRange;
            if (UnityEngine.Random.value < CloudLightningChance)
            {
                // cloud only lightning
                LightningBoltScript.TrunkWidthRange = new RangeOfFloats();
                LightningBoltScript.Generations = 1;
            }
            LightningBoltScript.LightParameters.LightIntensity = intensity * intensityModifier;
            LightningBoltScript.Trigger(start, end);

            // restore properties in case they were modified
            LightningBoltScript.TrunkWidthRange = trunkWidth;
            LightningBoltScript.Generations = generations;
        }

        private void UpdateLighting()
        {
            if (LightningInProgress)
            {
                return;
            }

            CheckForLightning();
        }

        protected virtual void Start()
        {
            Camera = (Camera == null ? (Camera.main == null ? Camera.current : Camera.main) : Camera);
            AudioSourceThunder = gameObject.AddComponent<AudioSource>();
            CalculateNextLightningTime();
        }

        protected abstract Vector3 CalculateStartPosition(ref Vector3 anchorPosition, Camera visibleInCamera, bool intense);
        protected abstract Vector3 CalculateEndPosition(ref Vector3 anchorPosition, ref Vector3 end, bool intense);
        
        protected virtual void Update()
        {
            if (EnableLightning)
            {
                UpdateLighting();
            }
        }

        public void CallNormalLightning()
        {
            CallNormalLightning(null, null);
        }

        public void CallNormalLightning(Vector3? start, Vector3? end)
        {
            StartCoroutine(ProcessLightning(start, end, false, true));
        }

        public void CallIntenseLightning()
        {
            CallIntenseLightning(null, null);
        }

        public void CallIntenseLightning(Vector3? start, Vector3? end)
        {
            StartCoroutine(ProcessLightning(start, end, true, true));
        }

        public bool EnableLightning { get; set; }
    }
}