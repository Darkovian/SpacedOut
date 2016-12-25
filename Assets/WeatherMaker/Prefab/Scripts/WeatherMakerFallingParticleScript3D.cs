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
    public class WeatherMakerFallingParticleScript3D : WeatherMakerFallingParticleScript
    {
        [Header("3D Settings")]
        [Tooltip("The height above the camera that the particles will start falling from")]
        public float Height = 25.0f;

        [Tooltip("How far the particle system is ahead of the player")]
        public float ForwardOffset = -7.0f;

        [Tooltip("The top y value of the mist particles")]
        public float MistHeight = 3.0f;

        private void CreateMeshEmitter(ParticleSystem p)
        {
            if (p == null || p.shape.shapeType != ParticleSystemShapeType.Mesh)
            {
                return;
            }

            Mesh emitter = new Mesh();
            emitter.vertices = new Vector3[]
            {
                new Vector3(-5.0f, 0.0f, 0.0f),
                new Vector3(5.0f, 0.0f, 0.0f),
                new Vector3(-70.0f, 0.0f, 50.0f),
                new Vector3(70.0f, 0.0f, 50.0f)
            };
            emitter.triangles = new int[] { 0, 1, 2, 2, 1, 3 };
            var s = p.shape;
            s.mesh = emitter;
        }

        private void TransformParticleSystem(ParticleSystem p, float forward, float height, float rotationYModifier)
        {
            if (p == null)
            {
                return;
            }

            Vector3 pos = Camera.transform.position;
            Vector3 anchorForward = Camera.transform.forward;
            pos.x += anchorForward.x * forward;
            pos.y += height;
            pos.z += anchorForward.z * forward;
            p.transform.position = pos;
            Vector3 angles = p.transform.rotation.eulerAngles;
            p.transform.rotation = Quaternion.Euler(angles.x, Camera.transform.rotation.eulerAngles.y * rotationYModifier, angles.z);
        }

        protected virtual void UpdatePositions()
        {
            // keep particles and mist above the player
            TransformParticleSystem(ParticleSystem, ForwardOffset, Height, 1.0f);
            TransformParticleSystem(MistParticleSystem, 0.0f, MistHeight, 0.0f);
        }

        protected override void Start()
        {
            base.Start();

            CreateMeshEmitter(ParticleSystem);
            CreateMeshEmitter(MistParticleSystem);
        }

        protected override void Update()
        {
            base.Update();

            UpdatePositions();
        }
    }
}