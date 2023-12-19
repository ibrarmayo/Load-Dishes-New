using System;
using _GameMechanics.AxisGames.Core.WorldPositionInput;
using AxisGames.ParticleSystem;
using UnityEngine;

namespace _GameMechanics.AxisGames.Particle_System.Example.Scripts
{

    public class ParticleShooter : MonoBehaviour
    {
        [SerializeField] ParticleManager particleManager;
        [SerializeField] ParticleType particleType;
        private MouseWorldInput _mouseWorldInput;

        private void Start()
        {
            _mouseWorldInput = FindObjectOfType<MouseWorldInput>();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                particleManager.PlayParticle(particleType, _mouseWorldInput.GetPosition());
            }
        }
    }
}