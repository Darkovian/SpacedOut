//
// Weather Maker for Unity
// (c) 2016 Digital Ruby, LLC
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace DigitalRuby.WeatherMaker
{
    public class WeatherMakerFallingParticleScript : MonoBehaviour
    {
        /// <summary>
        /// Set this to false if you are on low end mobile devices or have a day-time scene. Not all graphics cards will support this.
        /// </summary>
        public static bool EnablePerPixelLightingForMist = true;

        /// <summary>
        /// Camera, where the particles should hover over
        /// </summary>
        [HideInInspector]
        [System.NonSerialized]
        public Camera Camera;

        [Tooltip("Light particle looping audio source")]
        public AudioSource LoopSourceLight;

        [Tooltip("Medium particle looping audio source")]
        public AudioSource LoopSourceMedium;

        [Tooltip("Heavy particle looping audio source")]
        public AudioSource LoopSourceHeavy;

        [Tooltip("Intensity threshold for medium looping sound")]
        public float SoundMediumIntensityThreshold = 0.33f;

        [Tooltip("Intensity threshold for heavy loop sound")]
        public float SoundHeavyIntensityThreshold = 0.67f;

        [Tooltip("Overall intensity of the system (0-1)")]
        [Range(0.0f, 1.0f)]
        public float Intensity;

        [Tooltip("Intensity multiplier for fewer or extra particles")]
        [Range(0.1f, 10.0f)]
        public float IntensityMultiplier = 1.0f;

        [Tooltip("Intensity multiplier for fewer or extra mist particles")]
        [Range(0.1f, 10.0f)]
        public float MistIntensityMultiplier = 1.0f;

        [Tooltip("Base number of particles to emit per second. This is multiplied by intensity and intensity multiplier.")]
        [Range(100, 10000)]
        public int BaseEmissionRate = 1000;

        [Tooltip("Base number of mist paticles to emit per second. This is multiplied by intensity and intensity multiplier.")]
        [Range(5, 500)]
        public int BaseMistEmissionRate = 50;

        [Tooltip("Particle system")]
        public ParticleSystem ParticleSystem;

        [Tooltip("Particle system to use for mist")]
        public ParticleSystem MistParticleSystem;

        [Tooltip("Particles system for when particles hit something")]
        public ParticleSystem ExplosionParticleSystem;

        [Tooltip("The threshold that Intensity must pass for mist to appear (0 - 1). Set to 1 for no mist. Set this before changing Intensity.")]
        [Range(0.0f, 1.0f)]
        public float MistThreshold = 0.5f;

        public LoopingAudioSource AudioSourceLight { get; private set; }
        public LoopingAudioSource AudioSourceMedium { get; private set; }
        public LoopingAudioSource AudioSourceHeavy { get; private set; }
        public LoopingAudioSource AudioSourceCurrent { get; private set; }

        protected Material Material { get; private set; }
        protected Material ExplosionMaterial { get; private set; }
        protected Material MistMaterial { get; private set; }

        [NonSerialized]
        private float lastIntensityValue = -1.0f;

        [NonSerialized]
        private float lastIntensityMultiplierValue = -1.0f;

        [NonSerialized]
        private float lastMistIntensityMultiplierValue = -1.0f;

        private void CheckForIntensityChange()
        {
            if (lastIntensityValue == Intensity && lastIntensityMultiplierValue == IntensityMultiplier && lastMistIntensityMultiplierValue == MistIntensityMultiplier)
            {
                return;
            }

            lastIntensityValue = Intensity;
            lastIntensityMultiplierValue = IntensityMultiplier;
            lastMistIntensityMultiplierValue = MistIntensityMultiplier;

            if (Intensity < 0.001f)
            {
                if (AudioSourceCurrent != null)
                {
                    AudioSourceCurrent.Stop();
                    AudioSourceCurrent = null;
                }
                if (ParticleSystem != null)
                {
                    ParticleSystem.Stop();
                }
                if (MistParticleSystem != null)
                {
                    MistParticleSystem.Stop();
                }
            }
            else
            {
                LoopingAudioSource newSource;
                if (Intensity >= SoundHeavyIntensityThreshold)
                {
                    newSource = AudioSourceHeavy;
                }
                else if (Intensity >= SoundMediumIntensityThreshold)
                {
                    newSource = AudioSourceMedium;
                }
                else
                {
                    newSource = AudioSourceLight;
                }
                if (AudioSourceCurrent != newSource)
                {
                    if (AudioSourceCurrent != null)
                    {
                        AudioSourceCurrent.Stop();
                    }
                    AudioSourceCurrent = newSource;
                    AudioSourceCurrent.Play(1.0f);
                }
                if (ParticleSystem != null)
                {
                    var e = ParticleSystem.emission;
                    ParticleSystem.MinMaxCurve rate = ParticleSystem.emission.rate;
                    rate.mode = ParticleSystemCurveMode.Constant;
                    rate.constantMin = rate.constantMax = BaseEmissionRate * Intensity * IntensityMultiplier;
                    ParticleSystem.maxParticles = (int)Mathf.Max(ParticleSystem.maxParticles, rate.constantMax * ParticleSystem.startLifetime);
                    e.rate = rate;
                    if (!ParticleSystem.isPlaying)
                    {
                        ParticleSystem.Play();
                    }
                    //Debug.Log("Rate: " + ParticleSystem.emission.rate.constantMax);
                }
                if (MistParticleSystem != null)
                {
                    if (MistThreshold >= Intensity)
                    {
                        MistParticleSystem.Stop();
                    }
                    else
                    {
                        var e = MistParticleSystem.emission;
                        ParticleSystem.MinMaxCurve rate = MistParticleSystem.emission.rate;
                        rate.mode = ParticleSystemCurveMode.Constant;
                        rate.constantMin = rate.constantMax = BaseMistEmissionRate * Intensity * MistIntensityMultiplier;
                        MistParticleSystem.maxParticles = (int)Mathf.Max(MistParticleSystem.maxParticles, rate.constantMax * ParticleSystem.startLifetime);
                        e.rate = rate;
                        if (!MistParticleSystem.isPlaying)
                        {
                            MistParticleSystem.Play();
                        }
                    }
                }
            }
        }

        private Material InitParticleSystem(ParticleSystem p, bool perPixelParticles)
        {
            if (p == null)
            {
                return null;
            }

            Renderer renderer = p.GetComponent<Renderer>();
            Material m = new Material(renderer.material);
            if (perPixelParticles && SystemInfo.graphicsShaderLevel >= 30)
            {
                m.EnableKeyword("PER_PIXEL_LIGHTING");
            }
            else
            {
                m.DisableKeyword("PER_PIXEL_LIGHTING");
            }
            renderer.material = m;

            return m;
        }

        private void CheckForParticleSystem()
        {

#if DEBUG

            if (ParticleSystem == null)
            {
                Debug.LogError("Particle system is null");
                return;
            }

#endif

        }

        protected virtual void OnCollisionEnabledChanged() { }

        protected virtual void Start()
        {
            CheckForParticleSystem();

            AudioSourceLight = new LoopingAudioSource(LoopSourceLight);
            AudioSourceMedium = new LoopingAudioSource(LoopSourceMedium);
            AudioSourceHeavy = new LoopingAudioSource(LoopSourceHeavy);

            InitialStartSpeed = ParticleSystem.startSpeed;
            InitialStartSize = ParticleSystem.startSize;
            if (MistParticleSystem != null)
            {
                InitialStartSpeedMist = MistParticleSystem.startSpeed;
                InitialStartSizeMist = MistParticleSystem.startSize;
            }
            Material = InitParticleSystem(ParticleSystem, false);
            MistMaterial = InitParticleSystem(MistParticleSystem, EnablePerPixelLightingForMist);
            ExplosionMaterial = InitParticleSystem(ExplosionParticleSystem, false);
        }

        protected virtual void Update()
        {
            CheckForIntensityChange();
            AudioSourceLight.Update();
            AudioSourceMedium.Update();
            AudioSourceHeavy.Update();
        }

        protected virtual void FixedUpdate()
        {
        }

        protected float InitialStartSpeed { get; private set; }
        protected float InitialStartSize { get; private set; }
        protected float InitialStartSpeedMist { get; private set; }
        protected float InitialStartSizeMist { get; private set; }

        private bool collisionEnabled = true;
        public bool CollisionEnabled
        {
            get { return collisionEnabled; }
            set
            {
                if (value != collisionEnabled)
                {
                    collisionEnabled = value;

                    if (ParticleSystem != null)
                    {
                        var c = ParticleSystem.collision;
                        var s = ParticleSystem.subEmitters;
                        c.enabled = collisionEnabled;
                        s.enabled = collisionEnabled;
                    }

                    if (MistParticleSystem != null)
                    {
                        var c = MistParticleSystem.collision;
                        var s = MistParticleSystem.subEmitters;
                        c.enabled = collisionEnabled;
                        s.enabled = collisionEnabled;
                    }

                    if (ExplosionParticleSystem != null)
                    {
                        var c = ExplosionParticleSystem.collision;
                        var s = ExplosionParticleSystem.subEmitters;
                        c.enabled = collisionEnabled;
                        s.enabled = collisionEnabled;
                    }

                    OnCollisionEnabledChanged();
                }
            }
        }
    }
}