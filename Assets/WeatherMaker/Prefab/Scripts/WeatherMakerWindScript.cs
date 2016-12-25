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
    public class WeatherMakerWindScript : MonoBehaviour
    {
        [Tooltip("Optional camera the wind is being shown in.")]
        public Camera Camera;

        [Tooltip("Wind looping audio source")]
        public AudioSource LoopSourceWind;

        [Tooltip("Weather wind zone")]
        public WindZone WindZone;

        [Tooltip("X = minimum wind speed. Y = maximum wind speed. Z = sound multiplier. Wind speed is divided by Z to get sound multiplier value. Set Z to lower than Y to increase wind sound volume, or higher to decrease wind sound volume.")]
        public Vector3 WindSpeedRange = new Vector3(50.0f, 500.0f, 500.0f);

        [SingleLine("How often the wind speed and direction changes (minimum and maximum change interval in seconds)")]
        public RangeOfFloats WindChangeInterval = new RangeOfFloats { Minimum = 5.0f, Maximum = 30.0f };

        [Tooltip("Whether the wind can blow up. Default is false which means only horizontal wind.")]
        public bool AllowBlowUp = false;

        [Tooltip("Whether wind should be enabled.")]
        public bool EnableWind = true;

        /// <summary>
        /// Wind audio source
        /// </summary>
        public LoopingAudioSource AudioSourceWind { get; private set; }

        private float nextWindTime;

        private void UpdateWind()
        {
            if (EnableWind && WindZone != null && WindSpeedRange.y > 1.0f)
            {
                WindZone.gameObject.SetActive(true);
                if (Camera != null)
                {
                    WindZone.transform.position = Camera.transform.position;
                    if (!Camera.orthographic)
                    {
                        WindZone.transform.Translate(0.0f, WindZone.radius, 0.0f);
                    }
                }
                if (nextWindTime < Time.time)
                {
                    WindZone.windMain = UnityEngine.Random.Range(WindSpeedRange.x, WindSpeedRange.y);
                    WindZone.windTurbulence = UnityEngine.Random.Range(WindSpeedRange.x, WindSpeedRange.y);
                    if (Camera != null && Camera.orthographic)
                    {
                        int val = UnityEngine.Random.Range(0, 2);
                        WindZone.transform.rotation = Quaternion.Euler(0.0f, (val == 0 ? 90.0f : -90.0f), 0.0f);
                    }
                    else
                    {
                        float xAxis = (AllowBlowUp ? UnityEngine.Random.Range(-30.0f, 30.0f) : 0.0f);
                        WindZone.transform.rotation = Quaternion.Euler(xAxis, UnityEngine.Random.Range(0.0f, 360.0f), 0.0f);
                    }
                    nextWindTime = Time.time + WindChangeInterval.Random();
                    AudioSourceWind.Play((WindZone.windMain / WindSpeedRange.z));
                }
            }
            else
            {
                AudioSourceWind.Stop();
            }

            AudioSourceWind.Update();
        }

        private void Start()
        {
            AudioSourceWind = new LoopingAudioSource(LoopSourceWind);
        }

        private void Update()
        {
            UpdateWind();
        }
    }
}