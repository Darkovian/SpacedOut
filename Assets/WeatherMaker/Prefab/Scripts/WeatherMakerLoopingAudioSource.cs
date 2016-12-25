//
// Weather Maker for Unity
// (c) 2016 Digital Ruby, LLC
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 

using System;

using UnityEngine;

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Provides an easy wrapper to looping audio sources with nice transitions for volume when starting and stopping
    /// </summary>
    public class LoopingAudioSource
    {
        /// <summary>
        /// The audio source that is looping
        /// </summary>
        public AudioSource AudioSource { get; private set; }

        /// <summary>
        /// The target volume
        /// </summary>
        public float TargetVolume { get; set; }

        /// <summary>
        /// The original target volume - useful if the global sound volume changes you can still have the original target volume to multiply by.
        /// </summary>
        public float OriginalTargetVolume { get; private set; }

        /// <summary>
        /// Is this sound stopping?
        /// </summary>
        public bool Stopping { get; private set; }

        private float initialTargetVolume;

        public float VolumeModifier
        {
            get { return volumeModifier; }
            set
            {
                if (value != volumeModifier)
                {
                    volumeModifier = Mathf.Clamp(value, 0.0f, 1.0f);
                    UpdateVolumeModifier();
                }
            }
        }

        private float startVolume;
        private float startMultiplier;
        private float stopMultiplier;
        private float currentMultiplier;
        private float timestamp;
        private float volumeModifier;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="audioSource">Audio source, will be looped automatically</param>
        public LoopingAudioSource(AudioSource audioSource) : this(audioSource, 2.0f, 2.0f)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="audioSource">Audio source, will be looped automatically</param>
        /// <param name="startMultiplier">Start multiplier - seconds to reach peak sound</param>
        /// <param name="stopMultiplier">Stop multiplier - seconds to fade sound back to 0 volume when stopped</param>
        public LoopingAudioSource(AudioSource audioSource, float startMultiplier, float stopMultiplier)
        {
            AudioSource = audioSource;
            if (audioSource != null)
            {
                AudioSource.loop = true;
                AudioSource.volume = 0.0f;
                AudioSource.Stop();
            }

            this.startMultiplier = currentMultiplier = startMultiplier;
            this.stopMultiplier = stopMultiplier;
        }

        /// <summary>
        /// Play this looping audio source
        /// </summary>
        public void Play()
        {
            Play(1.0f);
        }

        /// <summary>
        /// Play this looping audio source
        /// </summary>
        /// <param name="targetVolume">Max volume</param>
        public void Play(float targetVolume)
        {
            if (AudioSource != null)
            {
                AudioSource.volume = startVolume = (AudioSource.isPlaying ? AudioSource.volume : 0.0f);
                AudioSource.loop = true;
                currentMultiplier = startMultiplier;
                OriginalTargetVolume = targetVolume;
                TargetVolume = initialTargetVolume = targetVolume * VolumeModifier;
                Stopping = false;
                timestamp = 0.0f;
                if (!AudioSource.isPlaying)
                {
                    AudioSource.Play();
                }
            }
        }

        /// <summary>
        /// Stop this looping audio source. The sound will fade out smoothly.
        /// </summary>
        public void Stop()
        {
            if (AudioSource != null && AudioSource.isPlaying && !Stopping)
            {
                startVolume = AudioSource.volume;
                TargetVolume = 0.0f;
                currentMultiplier = stopMultiplier;
                Stopping = true;
                timestamp = 0.0f;
            }
        }

        /// <summary>
        /// Update this looping audio source
        /// </summary>
        /// <returns>True if finished playing, false otherwise</returns>
        public bool Update()
        {
            if (AudioSource != null && AudioSource.isPlaying)
            {
                // check if we need to stop because the volume has reached 0
                if ((AudioSource.volume = Mathf.Lerp(startVolume, TargetVolume, (timestamp += Time.deltaTime) / currentMultiplier)) == 0.0f && Stopping)
                {
                    AudioSource.Stop();
                    Stopping = false;

                    // done playing
                    return true;
                }
                else
                {
                    // not done playing yet
                    return false;
                }
            }

            // done playing
            return true;
        }

        private void UpdateVolumeModifier()
        {
            TargetVolume = initialTargetVolume * VolumeModifier;
        }
    }
}
