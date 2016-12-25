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
    public class WeatherMakerCloudScript2D : WeatherMakerCloudScript
    {
        private ParticleSystem cloudParticleSystem;
        private Renderer cloudParticleSystemRenderer;

        private void UpdateParticleSystem()
        {
            cloudParticleSystem.maxParticles = NumberOfClouds;
            var anim = cloudParticleSystem.textureSheetAnimation;
            anim.numTilesX = MaterialColumns;
            anim.numTilesY = MaterialRows;
            cloudParticleSystemRenderer.sharedMaterial.mainTexture = MaterialTexture;
            cloudParticleSystemRenderer.sharedMaterial.SetColor("_TintColor", TintColor);
            cloudParticleSystemRenderer.sharedMaterial.EnableKeyword("ORTHOGRAPHIC_MODE");
        }

        protected override void OnUpdateMaterial(Material m)
        {
            base.OnUpdateMaterial(m);
        }

        protected override void Start()
        {
            base.Start();

            cloudParticleSystem = GetComponentInChildren<ParticleSystem>();
            cloudParticleSystemRenderer = cloudParticleSystem.GetComponent<Renderer>();
            UpdateParticleSystem();
        }

        protected override void Update()
        {
            base.Update();

            UpdateParticleSystem();
        }

        public override void CreateClouds()
        {
            cloudParticleSystem.Play();
        }

        public override void RemoveClouds()
        {
            cloudParticleSystem.Stop();
        }

        public override void Reset()
        {
            cloudParticleSystem.Stop();
            cloudParticleSystem.Clear();
        }
    }
}