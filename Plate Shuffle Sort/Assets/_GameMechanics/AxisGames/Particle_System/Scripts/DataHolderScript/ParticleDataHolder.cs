using AxisGames.ObjectPooler;
using UnityEngine;

namespace AxisGames.ParticleSystem
{
    public enum ParticleType
    {
        StackUnlock,
        Merge,
        Confitie,
        Hit
    }


    [CreateAssetMenu(menuName = "AxisGames/Particle System /Particle DataSheet")]
    public class ParticleDataHolder : ScriptableObject
    {
        public string poolerName;
        public ParticleType particleType;
        public Particle particlePrefab;
        public int poolSize;
        public Transform poolContainer;
        public ObjectPooler<Particle> pool;
    }
}
