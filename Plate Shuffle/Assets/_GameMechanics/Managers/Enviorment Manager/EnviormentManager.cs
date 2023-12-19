using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class EnviormentManager : MonoBehaviour
{
    #region Structs -----------

    [System.Serializable]
    private struct EnviormentMaterialData
    {
        public Material groundMaterial;
        public Material plateAreaMaterial;
    }
    #endregion

    [FoldoutGroup("Enviorment Renderers")]
    [SerializeField] MeshRenderer groundRender;
    [FoldoutGroup("Enviorment Renderers")]
    [SerializeField] MeshRenderer plateAreaBaseRender;

    [FoldoutGroup("Enviorment Materials")]
    [SerializeField] EnviormentMaterialData[] enviormentMaterialList;

    [BoxGroup("Status")]
    [ReadOnly][SerializeField] int currentEnviorment;


    [Button(ButtonSizes.Small)]
    public void NextEnviorment()
    {
        SetEnviorment(GetIndex(previous: false));
    }

    [Button(ButtonSizes.Small)]
    public void PreviousEnviorment()
    {
        SetEnviorment(GetIndex(previous:true));
    }

    private void SetEnviorment(int index)
    {
        groundRender.material = enviormentMaterialList[index].groundMaterial;
        plateAreaBaseRender.material = enviormentMaterialList[index].plateAreaMaterial;
    }

    private int GetIndex(bool previous = false)
    {
        if (previous)
        {
            currentEnviorment--;
            if(currentEnviorment <= 0) { currentEnviorment = 0; }
            CurrentEnviorment = currentEnviorment;

            return currentEnviorment;
        }
        else
        {
            currentEnviorment++;
            if (currentEnviorment > enviormentMaterialList.Length-1) { currentEnviorment = enviormentMaterialList.Length - 1; }
            CurrentEnviorment = currentEnviorment;

            return currentEnviorment;
        }
    }

    private int CurrentEnviorment
    {
        get 
        {
            return PlayerPrefs.GetInt("currentEnvIndex", 0);
        }
        set
        {
            PlayerPrefs.SetInt("currentEnvIndex", value);
        }
    }
}


