using System.Collections.Generic;
using UnityEngine;
using AxisGames.ObjectPooler;
using AxisGames.Singletons;

namespace AxisGames.ParticleSystem
{
    public class ParticleManager : SingletonLocal<ParticleManager>
    {
        [Header("----- Pooler Data Sheets -----")]
        [SerializeField] ParticleDataHolder[] popupPoolers; // Data Refrence
        [Space(5)]

        [Header("----- Refrences -----")]
        [SerializeField] GameObject container; // World Object to Hold Pooled Objects... ( Must be Enable All the Time ) 


        private Dictionary<ParticleType, ParticleDataHolder> popupDictionary = new Dictionary<ParticleType, ParticleDataHolder>();  // Holds All Pooled Data with Type Key into Dictionary

        protected override void Awake()
        {
            base.Awake();
            if (CanInitialize()) InitializePoolers(); // Checks For Null if Non,, then Start Pooler 
        }

        #region Particle Pooling Methods ---

        public void PlayParticle(ParticleType particleType, Vector3 position) // Plays gien Type Particle on the Position...
        {
            Particle popedParticle = GetParticle(particleType);
            if (popedParticle)
            {

                popedParticle.transform.position = position;
                popedParticle.Play();
                popedParticle.StartTimer();


            }
        }

        private Particle GetParticle(ParticleType particleType)
        {
            if (popupDictionary.ContainsKey(particleType))
            {
                return popupDictionary[particleType].pool.GetNew();
            }
            else
            {
                FireLogMessage("Provided Particle Type Does not Exist");
                return null;
            }
        }

        #endregion

        #region Pooler and Data Initialization Checks ---

        private bool CanInitialize() // Null Checks -----
        {
            if (popupPoolers.Length <= 0) { FireLogMessage("Pooler Data Not Assigned"); return false; }
            else if (container == null) { FireLogMessage("Container Not Assigned"); return false; }
            else { return true; }
        }
        private void InitializePoolers() // Loads the Pool Data Into Dictionary
        {
            for (int i = 0; i < popupPoolers.Length; i++)
            {
                popupPoolers[i].poolContainer = container.transform;
                popupPoolers[i].pool = new ObjectPooler<Particle>();
                popupPoolers[i].pool.Initialize(popupPoolers[i].poolSize, popupPoolers[i].particlePrefab, popupPoolers[i].poolContainer);

                popupDictionary.Add(popupPoolers[i].particleType, popupPoolers[i]);
            }
        }
        private void FireLogMessage(string message)
        {
            Debug.LogError(message + " !! Safe Return !! ");
        }

        #endregion

    }
}