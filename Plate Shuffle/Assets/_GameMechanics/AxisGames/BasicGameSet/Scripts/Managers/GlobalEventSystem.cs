using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GlobalEventSystem : MonoBehaviour
{   
    public event Action onStartProgression;

    private void OnDestroy()
    {
        onStartProgression  = null;
    }

    internal void StartPregression()
    {
        onStartProgression?.Invoke();
    }
}
