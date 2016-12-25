//
// Weather Maker for Unity
// (c) 2016 Digital Ruby, LLC
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 

using UnityEngine;
using System.Collections.Generic;

namespace DigitalRuby.WeatherMaker
{
    [RequireComponent(typeof(ParticleSystem))]
    public class WeatherMakerParticleCollisionScript : MonoBehaviour
    {
        private readonly List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
        private ParticleSystem collisionParticleSystem;

        [Tooltip("Explosion particle system")]
        public ParticleSystem ExplosionParticleSystem;

        private void Start()
        {
            collisionParticleSystem = GetComponent<ParticleSystem>(); 
        }

        private void Update()
        {

        }
        
        private void OnParticleCollision(GameObject obj)
        {
            if (ExplosionParticleSystem != null)
            {
                int count = collisionParticleSystem.GetCollisionEvents(obj, collisionEvents);
                var em = ExplosionParticleSystem.emission;

                for (int i = 0; i < count; i++)
                {
                    ParticleCollisionEvent evt = collisionEvents[i];
                    Vector3 pos = evt.intersection;
                    ExplosionParticleSystem.transform.position = pos;
                    ExplosionParticleSystem.Emit(UnityEngine.Random.Range((int)em.rate.constantMin, (int)em.rate.constantMax + 1));
                }
            }
        }
    }
}