using Sirenix.OdinInspector;
using System;
using UnityEngine;

public class ProgressManager : MonoBehaviour
{
    #region Fields------
    [FoldoutGroup("Component Refs")]
    [SerializeField] PlatesUiController platesUiController;
    [FoldoutGroup("Component Refs")]
    [SerializeField] BarController barController;
    [FoldoutGroup("Component Refs")]
    [SerializeField] UnlockPopup unlockPopup;
    [FoldoutGroup("Component Refs")]
    [SerializeField] GameObject newPlatePanel;

    //[FoldoutGroup("Component Refs")]
    //[SerializeField] GameObject platesRenderCamera;

    //[FoldoutGroup("3D Plate Objects")]
    //[SerializeField] GameObject[] plateObjects;

    [BoxGroup("Progression Settings")]
    [SerializeField] int startingPlateNumber = 3;
    [BoxGroup("Progression Settings")]
    [SerializeField] int limmitperNewPlate = 30;

    [Space]
    [BoxGroup("Plates Progression Stats")]
    [ReadOnly]
    [SerializeField] int washedPlatesCount = 0;
    [BoxGroup("Plates Progression Stats")]
    [ReadOnly]
    [SerializeField] int highestPlateNumber;
    [BoxGroup("Plates Progression Stats")]
    [ReadOnly]
    [SerializeField] int totalPlateNumber;

    public int MaxUnlockedPlate => highestPlateNumber;

    #endregion

    private void Awake()
    {
        InitializeStats();
        UpdateBar();
    }

    #region Data Initialization ----
    private void InitializeStats()
    {
        if (!SaveData.Instance.InitalProgression)
        {
            SetInitial();
        }
        else
        {
            LoadData();
        }
    }
    private void SetInitial()
    {
        totalPlateNumber = Enum.GetNames(typeof(PlateColor)).Length;
        highestPlateNumber = startingPlateNumber;

        SaveData.Instance.InitalProgression = true;
        SaveData.Instance.HighestPlateCoolor = highestPlateNumber;

        GSF_SaveLoad.SaveProgress();
    }
    private void LoadData()
    {
        totalPlateNumber = Enum.GetNames(typeof(PlateColor)).Length;
        highestPlateNumber = SaveData.Instance.HighestPlateCoolor;
        washedPlatesCount = SaveData.Instance.WashPlatesProgress;
    }

    #endregion

    public void UpdateProgress()
    {
        washedPlatesCount += 10;
        
        UpdateBar();
    }
    private void UpdateBar()
    {
        if (highestPlateNumber < platesUiController.TotalPlates())
        {
            float amt = (float)washedPlatesCount / limmitperNewPlate;
            barController.UpdateValue(amt, CheckForLimmit);
            barController.SetPlateNumber(highestPlateNumber - 2);
            barController.SetPlateVisual(platesUiController.GetSprite(highestPlateNumber));
        }
        else
        {
            float amt = (float)washedPlatesCount / limmitperNewPlate;
            barController.SetPlateVisual(platesUiController.GetSprite(platesUiController.TotalPlates()-1));
            barController.UpdateValue(amt, () => { });
        }
    }
    private void CheckForLimmit()
    {
        if (washedPlatesCount >= limmitperNewPlate && highestPlateNumber < platesUiController.TotalPlates())
        {
            unlockPopup.SetPlateVisualAndOpen(platesUiController.GetSprite(highestPlateNumber));
            //EnableRender();
            SoundManager.Instance.PlayOneShot(SoundManager.Instance.skinUnlock, 0.5f);
            highestPlateNumber++;
            washedPlatesCount = 0;
            UpdateBar();
        }
        SaveProgress();
    }

    public void EnableRender()
    {
        //Enable3DPlate(highestPlateNumber);
        SetComponentsState(true);
    }

    public void DisableRender()
    {
        SetComponentsState(false);
    }

    private void SetComponentsState(bool state)
    {
        newPlatePanel.SetActive(state);
        //platesRenderCamera.SetActive(state);
    }

    //private void Enable3DPlate(int plateToShow)
    //{
    //    for (int i = 0; i < plateObjects.Length; i++) 
    //    {
    //        if (i == plateToShow)
    //        {
    //            plateObjects[i].SetActive(true);
    //        }
    //        else
    //        {
    //            plateObjects[i].SetActive(false);
    //        }
    //    }
    //}

    private void SaveProgress()
    {
        SaveData.Instance.HighestPlateCoolor = highestPlateNumber;
        SaveData.Instance.WashPlatesProgress = washedPlatesCount;
        GSF_SaveLoad.SaveProgress();
    }
}
