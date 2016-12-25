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
    public class WeatherMakerScript : MonoBehaviour
    {
        [Header("Precipitation Objects")]
        [Tooltip("Camera the weather should hover over. Defaults to main camera.")]
        public Camera Camera;

        [Tooltip("Clouds object")]
        public GameObject Clouds;

        [Tooltip("Wind object")]
        public GameObject Wind;

        [Tooltip("The sun")]
        public Light Sun;

        [Tooltip("Whether the precipitation collides with the world")]
        public bool CollisionEnabled = true;

        [Tooltip("Whether to enable wind")]
        public bool WindEnabled;

        [Tooltip("Configuration script. Should be deactivated for release builds.")]
        public WeatherMakerConfigurationScript ConfigurationScript;

        [Tooltip("Rain script")]
        public WeatherMakerFallingParticleScript RainScript;

        [Tooltip("Snow script")]
        public WeatherMakerFallingParticleScript SnowScript;

        [Tooltip("Hail script")]
        public WeatherMakerFallingParticleScript HailScript;

        [Tooltip("Sleet script")]
        public WeatherMakerFallingParticleScript SleetScript;

        [Tooltip("Lightning script")]
        public WeatherMakerThunderAndLightningScript LightningScript;

        [Tooltip("Cloud script")]
        public WeatherMakerCloudScript CloudScript;

        [Tooltip("Wind script")]
        public WeatherMakerWindScript WindScript;

        [Tooltip("Sky sphere script")]
        public WeatherMakerSkySphereScript SkySphereScript;

        [Tooltip("Day night script")]
        public WeatherMakerDayNightCycleScript DayNightScript;

        [Tooltip("Intensity of precipitation (0-1)")]
        [Range(0.0f, 1.0f)]
        public float PrecipitationIntensity;

        [Tooltip("How long in seconds to fully change from one precipitation type to another")]
        [Range(0.0f, 300.0f)]
        public float PrecipitationChangeDuration = 4.0f;

        [Tooltip("The threshold change in intensity that will cause a cross-fade between precipitation changes. Intensity changes smaller than this value happen quickly.")]
        [Range(0.0f, 0.2f)]
        public float PrecipitationChangeThreshold = 0.1f;

        [Range(0.0f, 1.0f)]
        [Tooltip("Change the volume of all weather maker sounds.")]
        public float VolumeModifier = 1.0f;

        [NonSerialized]
        private float lastPrecipitationIntensityChange = -1.0f;

        [NonSerialized]
        private float lastVolumeModifier = -1.0f;

        private void TweenScript(WeatherMakerFallingParticleScript script, float end)
        {
            if (Mathf.Abs(script.Intensity - end) < PrecipitationChangeThreshold)
            {
                script.Intensity = end;
            }
            else
            {
                TweenFactory.Tween("WeatherMakerPrecipitationChange_" + script.gameObject.GetInstanceID(), script.Intensity, end, PrecipitationChangeDuration, TweenScaleFunctions.Linear, (t) =>
                {
                    // Debug.LogFormat("Tween key: {0}, value: {1}, prog: {2}", t.Key, t.CurrentValue, t.CurrentProgress);
                    script.Intensity = t.CurrentValue;
                }, null);
            }
        }

        private void ChangePrecipitation(WeatherMakerFallingParticleScript newPrecipitation)
        {
            if (newPrecipitation != currentPrecipitation && currentPrecipitation != null)
            {
                TweenScript(currentPrecipitation, 0.0f);
                lastPrecipitationIntensityChange = -1.0f;
            }
            currentPrecipitation = newPrecipitation;
        }

        private void UpdateCollision()
        {
            RainScript.CollisionEnabled = CollisionEnabled;
            SnowScript.CollisionEnabled = CollisionEnabled;
            HailScript.CollisionEnabled = CollisionEnabled;
            SleetScript.CollisionEnabled = CollisionEnabled;
        }

        private void UpdateSoundsVolumes()
        {
            LightningScript.VolumeModifier = VolumeModifier;
            RainScript.AudioSourceLight.VolumeModifier = RainScript.AudioSourceMedium.VolumeModifier = RainScript.AudioSourceHeavy.VolumeModifier = VolumeModifier;
            SnowScript.AudioSourceLight.VolumeModifier = SnowScript.AudioSourceMedium.VolumeModifier = SnowScript.AudioSourceHeavy.VolumeModifier = VolumeModifier;
            HailScript.AudioSourceLight.VolumeModifier = HailScript.AudioSourceMedium.VolumeModifier = HailScript.AudioSourceHeavy.VolumeModifier = VolumeModifier;
            SleetScript.AudioSourceLight.VolumeModifier = SleetScript.AudioSourceMedium.VolumeModifier = SleetScript.AudioSourceHeavy.VolumeModifier = VolumeModifier;
            WindScript.AudioSourceWind.VolumeModifier = VolumeModifier;
        }

        private void UpdateWind()
        {
            if (Wind != null)
            {
                Wind.SetActive(WindEnabled);
            }
        }

        private void Awake()
        {
            Camera = (Camera == null ? (Camera.main == null ? Camera.current : Camera.main) : Camera);
            Debug.Assert(Camera != null, "Must assign a camera for weather maker to work properly. Tag a camera as main camera, or manually assign the camera property.");

            UpdateCollision();
            RainScript.Camera = Camera;
            SnowScript.Camera = Camera;
            HailScript.Camera = Camera;
            SleetScript.Camera = Camera;
            CloudScript.Camera = Camera;
            // DayNightScript.Camera = Camera;
            if (SkySphereScript != null)
            {
                SkySphereScript.Camera = Camera;
            }
            if (WindScript != null)
            {
                WindScript.Camera = Camera;
            }
        }

        private void Update()
        {
            if (currentPrecipitation != null && PrecipitationIntensity != lastPrecipitationIntensityChange)
            {
                lastPrecipitationIntensityChange = PrecipitationIntensity;
                TweenScript(currentPrecipitation, PrecipitationIntensity);
            }
            if (VolumeModifier != lastVolumeModifier)
            {
                lastVolumeModifier = VolumeModifier;
                UpdateSoundsVolumes();
            }
            UpdateCollision();
            UpdateWind();
        }

        public WeatherMakerFallingParticleScript CurrentPrecipitation
        {
            get { return currentPrecipitation; }
            set
            {
                if (value != currentPrecipitation)
                {
                    ChangePrecipitation(value);
                }
            }
        }

        private WeatherMakerFallingParticleScript currentPrecipitation;

        /// <summary>
        /// Gets the current time of day in seconds. 86400 seconds per day.
        /// </summary>
         // public float TimeOfDay { get { return DayNightScript.TimeOfDay; } }
    }
}