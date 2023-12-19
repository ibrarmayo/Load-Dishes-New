using System;
using AxisGames.Singletons;
using UnityEngine;

namespace _GameMechanics.AxisGames.Core.WorldPositionInput
{
    public class MouseWorldInput : MonoBehaviour
    {
        [SerializeField] private new Camera camera;
        [SerializeField] private LayerMask targetLayerMask;

        private void Start()
        {
            camera = Camera.main;
        }

        public Vector3 GetPosition()
        {
            return GetRay().point;
        }

        public Transform GetHitObject()
        {
            return GetRay().transform;
        }

        private RaycastHit GetRay()
        {
            var ray = camera.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out RaycastHit rayCastHit, float.MaxValue, targetLayerMask);
            return rayCastHit;
        }
    }
}