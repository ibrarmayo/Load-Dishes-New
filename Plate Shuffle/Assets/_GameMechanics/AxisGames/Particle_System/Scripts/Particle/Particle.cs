using AxisGames.ObjectPooler;
using System;
using UnityEngine;


namespace AxisGames.ParticleSystem
{
    public class Particle : MonoBehaviour, IPooled<Particle>
    {
        [SerializeField] float disposeTime = 1f;
        public int poolID { get; set; }
        public ObjectPooler<Particle> pool { get; set; }

        [SerializeField] UnityEngine.ParticleSystem particle;

        public void StartTimer()
        {
            Invoke(nameof(FreePool), disposeTime);
        }

        public void FreePool()
        {
            pool.Free(this);
        }

        internal void Play()
        {
            particle.Play();
        }
    }
}