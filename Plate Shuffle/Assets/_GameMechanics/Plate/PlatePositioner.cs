using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatePositioner : MonoBehaviour
{
    [SerializeField] private List<Transform> plates = new List<Transform>();
    [SerializeField] private float currYPos,yPosToAdd;
    [SerializeField] private bool rename;
    int tempNum;

    [Button(ButtonSizes.Small)]
    public void SetPosition()
    {
        currYPos = 0;
        tempNum = 0;

        foreach (Transform t in plates)
        {
            t.localPosition = new Vector3(t.localPosition.x, currYPos, t.localPosition.z);
            if(rename) { tempNum++; t.name = $"Point {tempNum}"; }
            currYPos += yPosToAdd;
        }
    }
}
