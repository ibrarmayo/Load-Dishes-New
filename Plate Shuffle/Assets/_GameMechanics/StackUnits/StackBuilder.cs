using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StackBuilder : MonoBehaviour
{
    [SerializeField] Transform[] stackPoints;

    [SerializeField] int currentPoint = 0;
    Transform temp;

    public int TotalLimit { get => stackPoints.Length; }
    public int CurrentStacklimit { get => currentPoint; }

    public int GetLimit()
    {
        return (stackPoints.Length-1) - (currentPoint-1);
    }
    public bool isStackFree()
    {
        if(currentPoint < stackPoints.Length) { return true; }
        else { return false; }
    }
    public Transform GetPoint()
    {
        temp = stackPoints[currentPoint];
        currentPoint +=1;
        return temp;
    }

    public void ReleasePoint()
    {
        currentPoint--;
    }

    public void ClearAllPoints()
    {
        currentPoint = 0;
    }
}
