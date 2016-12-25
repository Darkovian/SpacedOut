//
// Weather Maker for Unity
// (c) 2016 Digital Ruby, LLC
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 

using UnityEngine;

using System;
using System.Collections;

namespace DigitalRuby.WeatherMaker
{
    public class WeatherMakerFallingParticleScript2D : WeatherMakerFallingParticleScript
    {
        [Header("2D Settings")]
        [Tooltip("The camera to show the particles in. Defaults to main camera.")]
        public new Camera Camera;

        [Tooltip("The starting y offset for particles and mist. This will be offset as a percentage of visible height from the top of the visible world.")]
        public float HeightMultiplier = 0.15f;

        [Tooltip("The total width of the particles and mist as a percentage of visible width")]
        public float WidthMultiplier = 1.5f;

        [Tooltip("Collision mask for the particles")]
        public LayerMask CollisionMask = -1;

        [Tooltip("Lifetime to assign to particles that have collided. 0 for instant death. This can allow the particles to penetrate a little bit beyond the collision point.")]
        [Range(0.0f, 5.0f)]
        public float CollisionLifeTime = 0.02f;

        [Tooltip("When a particles life time is less than or equal to this value, it may emit an explosion")]
        [Range(0.0f, 1.0f)]
        public float ExplosionEmissionLifeTimeMaximum = 1.0f / 60.0f;

        [Tooltip("Multiply the velocity of any mist colliding by this amount")]
        [Range(0.0f, 1.0f)]
        public float MistCollisionVelocityMultiplier = 0.8f;

        [Tooltip("Multiply the life time of any mist colliding by this amount")]
        [Range(0.0f, 1.0f)]
        public float MistCollisionLifeTimeMultiplier = 0.95f;

        [NonSerialized]
        private float cameraMultiplier = 1.0f;

        [NonSerialized]
        private Bounds visibleBounds;

        [NonSerialized]
        private float yOffset;

        [NonSerialized]
        private float visibleWorldWidth;

        private readonly ParticleSystem.Particle[] particles = new ParticleSystem.Particle[2048];

        private void TransformParticleSystem(ParticleSystem p, float initialStartSpeed, float initialStartSize)
        {
            if (p == null)
            {
                return;
            }

            p.transform.position = new Vector3(base.Camera.transform.position.x, visibleBounds.max.y + yOffset, p.transform.position.z);
            p.transform.localScale = new Vector3(visibleWorldWidth * WidthMultiplier, 1.0f, 1.0f);
            p.startSpeed = initialStartSpeed * cameraMultiplier;
            p.startSize = initialStartSize * cameraMultiplier;
        }

        private void EmitExplosion(ref Vector3 pos)
        {
            var em = ExplosionParticleSystem.emission;
            var vel = ExplosionParticleSystem.velocityOverLifetime;
            var velX = vel.x;
            var velY = vel.y;
            ExplosionParticleSystem.transform.position = pos;
            ExplosionParticleSystem.startSpeed *= cameraMultiplier;
            ExplosionParticleSystem.startSize *= cameraMultiplier;
            velX.constantMin *= cameraMultiplier;
            velX.constantMax *= cameraMultiplier;
            velY.constantMin *= cameraMultiplier;
            velY.constantMax *= cameraMultiplier;
            ExplosionParticleSystem.Emit(UnityEngine.Random.Range((int)em.rate.constantMin, (int)em.rate.constantMax));
            velX.constantMin /= cameraMultiplier;
            velX.constantMax /= cameraMultiplier;
            velY.constantMin /= cameraMultiplier;
            velY.constantMax /= cameraMultiplier;
            ExplosionParticleSystem.startSpeed /= cameraMultiplier;
            ExplosionParticleSystem.startSize /= cameraMultiplier;
        }

        private void CheckForCollisionsParticles()
        {
            if (CollisionMask == 0)
            {
                return;
            }

            int count = 0;
            bool changes = false;
            Vector3 posOffset = (ParticleSystem.simulationSpace == ParticleSystemSimulationSpace.Local ? ParticleSystem.transform.position : Vector3.zero);
            count = ParticleSystem.GetParticles(particles);
            RaycastHit2D hit;

            for (int i = 0; i < count; i++)
            {
                if (particles[i].remainingLifetime > CollisionLifeTime)
                {
                    Vector3 pos = particles[i].position + posOffset;
                    hit = Physics2D.Raycast(pos, particles[i].velocity.normalized, particles[i].velocity.magnitude * Time.deltaTime, CollisionMask);
                    if (hit.collider != null)
                    {
                        if (CollisionLifeTime == 0.0f)
                        {
                            particles[i].remainingLifetime = 0.0f;
                        }
                        else
                        {
                            particles[i].remainingLifetime = Mathf.Min(particles[i].remainingLifetime, UnityEngine.Random.Range(0.0f, CollisionLifeTime));
                        }
                        particles[i].position = hit.point;
                        changes = true;
                    }
                }
            }

            if (changes)
            {
                if (ExplosionParticleSystem != null)
                {
                    if (count == 0)
                    {
                        count = ParticleSystem.GetParticles(particles);
                    }
                    for (int i = 0; i < count; i++)
                    {
                        if (particles[i].remainingLifetime <= ExplosionEmissionLifeTimeMaximum)
                        {
                            Vector3 pos = particles[i].position;
                            EmitExplosion(ref pos);
                        }
                    }
                }
                ParticleSystem.SetParticles(particles, count);
            }
        }

        private void CheckForCollisionsMistParticles()
        {
            if (MistParticleSystem == null || CollisionMask == 0)
            {
                return;
            }

            int count = MistParticleSystem.GetParticles(particles);
            bool changes = false;
            RaycastHit2D hit;

            for (int i = 0; i < count; i++)
            {
                Vector3 pos = particles[i].position;
                hit = Physics2D.Raycast(pos, particles[i].velocity.normalized, particles[i].velocity.magnitude * Time.deltaTime);
                if (hit.collider != null && ((1 << hit.collider.gameObject.layer) & CollisionMask) != 0)
                {
                    particles[i].velocity *= MistCollisionVelocityMultiplier;
                    particles[i].remainingLifetime *= MistCollisionLifeTimeMultiplier;
                    changes = true;
                }
            }

            if (changes)
            {
                MistParticleSystem.SetParticles(particles, count);
            }
        }

        protected override void OnCollisionEnabledChanged()
        {
            base.OnCollisionEnabledChanged();

            CollisionMask = (CollisionEnabled ? -1 : 0);
        }

        protected override void Start()
        {
            base.Start();

            Camera = (Camera == null ? (Camera.main == null ? Camera.current : Camera.main) : Camera);
        }

        protected override void Update()
        {
            base.Update();

            if (Material != null)
            {
                Material.EnableKeyword("ORTHOGRAPHIC_MODE");
            }
            if (MistMaterial != null)
            {
                MistMaterial.EnableKeyword("ORTHOGRAPHIC_MODE");
            }
            if (ExplosionMaterial != null)
            {
                ExplosionMaterial.EnableKeyword("ORTHOGRAPHIC_MODE");
            }

            cameraMultiplier = (Camera.orthographicSize * 0.25f);
            visibleBounds.min = Camera.main.ViewportToWorldPoint(Vector3.zero);
            visibleBounds.max = Camera.main.ViewportToWorldPoint(Vector3.one);
            visibleWorldWidth = visibleBounds.size.x;
            yOffset = (visibleBounds.max.y - visibleBounds.min.y) * HeightMultiplier;

            TransformParticleSystem(ParticleSystem, InitialStartSpeed, InitialStartSize);
            TransformParticleSystem(MistParticleSystem, InitialStartSpeedMist, InitialStartSizeMist);

            CheckForCollisionsParticles();
            CheckForCollisionsMistParticles();
        }
    }
}