using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class PointsAlligner : MonoBehaviour
{
    [BoxGroup("Start Point")]
    [SerializeField] Transform startPoint;

    [Space]
    [BoxGroup("Following Points")]
    [SerializeField] Transform[] followingPoints;

    [Space]
    [BoxGroup("Settings")]
    [SerializeField] float distance;

    [Button]
    private void AllignPoints()
    {
        Transform prevPoint = startPoint;
        for (int i = 0; i < followingPoints.Length; i++)
        {
            Vector3 newPos = new Vector3(prevPoint.position.x, prevPoint.position.y, prevPoint.position.z - distance);
            followingPoints[i].position = newPos;
            followingPoints[i].rotation = prevPoint.rotation;

            prevPoint = followingPoints[i];
        }
    }
}
