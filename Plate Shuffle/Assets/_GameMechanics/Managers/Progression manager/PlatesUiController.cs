using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatesUiController : MonoBehaviour
{
    [SerializeField] Sprite[] plateSprites;

    public int TotalPlates()
    {
        return plateSprites.Length;
    }

    public Sprite GetSprite(int currentplate)
    {
        //int newPlateID = currentplate++;
        //if(newPlateID > plateSprites.Length - 1) { newPlateID = plateSprites.Length - 1; }

        return plateSprites[currentplate];
    }
}
