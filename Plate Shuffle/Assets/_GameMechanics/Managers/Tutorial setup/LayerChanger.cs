using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerChanger : MonoBehaviour
{
    public void ChangeLayerForTutorial(int layer)
    {
        SetLayerRecursively(this.gameObject, layer);
    }

    public void SetLayerRecursively(GameObject parent, int layerNumber)
    {
        foreach (Transform trans in parent.GetComponentsInChildren<Transform>(true))
        {
            trans.gameObject.layer = layerNumber;
        }
    }
}
