using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WashUnit : UnitBase
{
    [BoxGroup("Components")]
    [SerializeField] private WashHandController washHandController;
    [BoxGroup("Components")]
    [SerializeField] private LimmitBar limmitBar;
    [BoxGroup("Components")]
    [SerializeField] private GameObject selectVisual;
    [BoxGroup("Components")]
    [SerializeField] private Transform mergePoint;
    private bool cleaning = false;

    private void Start()
    {
        LoadDataInternally();
        CheckStackFull();
    }
    public override bool CanInteract(PlateColor plateColor)
    {
        if (cleaning) return false;

        if (plateColor == TopPlate || TopPlate == PlateColor.Empty)
        {
            InvokeDeleget();
            return true;
        }
        else
        {
            return false;
        }
    }
    public override void AddPlate(Plate plate)
    {
        //plate.transform.DOMove(GetPosRot().Pos, 0.1f);
        stackedPlates.Add(plate);
        topPlateColor = plate.PlateColor;

        limmitBar.SetValue(stackBuilder.TotalLimit, stackBuilder.CurrentStacklimit);
        SaveDataInternally(plate, true);

        CheckStackFull();
    }

    private void CheckStackFull()
    {
        if (stackBuilder.GetLimit() == 0 && !cleaning)
        {
            cleaning = true;
            Opned = false;
            StartCoroutine(StartWashing());
        }
    }

    IEnumerator StartWashing()
    {
        yield return new WaitForSeconds(0.5f);
        washHandController.MoveHandToWash(() => { StartCoroutine(StartScrubing()); }); // move hand to the wash unit. then start the scrub animation.

    }
    IEnumerator StartScrubing()
    {
        SoundManager.Instance.PlayOneShot(SoundManager.Instance.washing, 0.5f);
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < stackedPlates.Count; i++)
        {
            stackedPlates[i].SetMaterial(RefrenceManager.Instance.CleanPlateMat);
            yield return new WaitForSeconds(0.05f);
        }
        washHandController.Resethand();
        yield return new WaitForSeconds(0.5f);

        //for (int i = 0; i < stackedPlates.Count; i++)
        //{
        //	stackedPlates[i].transform.DOMove(mergePoint.position, 0.3f);
        //	stackedPlates[i].transform.DORotate(mergePoint.eulerAngles, 0.3f);
        //	yield return new WaitForSeconds(0.05f);
        //}

        //for (int i = 0; i < stackedPlates.Count; i++)
        //{
        //	SaveDataInternally(stackedPlates[i], false);
        //	Destroy(stackedPlates[i].gameObject);
        //	stackedPlates.RemoveAt(i);
        //	i--;
        //	yield return new WaitForSeconds(0.02f);
        //}

        for (int i = 0; i < stackedPlates.Count; i++)
        {
            stackedPlates[i].transform.DOScale(Vector3.zero, 0.2f).OnComplete(() =>
            {
                Destroy(stackedPlates[i].gameObject);
                stackedPlates.RemoveAt(i);
                i--;
            });
            SaveDataInternally(stackedPlates[i], false);
            yield return new WaitForSeconds(0.015f);
        }

        CoinsManager.Instance.AddCoins(10, CoinsToGive, transform);
        UpdateEarningCounter();
        SoundManager.Instance.PlayOneShot(SoundManager.Instance.cashPick, 0.65f);
        Vibration.VibrateNope();

        stackedPlates.Clear();
        TopPlate = PlateColor.Empty;
        limmitBar.ResetValue();
        stackBuilder.ClearAllPoints();
        tempStackPoint = stackPoint;
        RefrenceManager.Instance.ProgressManager.UpdateProgress();

        cleaning = false;
        Opned = true;
    }

    public override List<Plate> GetPlates(PlateColor plateColor, int stackLimit)
    {
        List<Plate> tempList = new List<Plate>();
        for (int i = stackedPlates.Count - 1; i >= 0; i--)
        {
            if (tempList.Count - 1 >= stackLimit - 1) { topPlateColor = stackedPlates[i].PlateColor; break; }
            if (stackedPlates[i].PlateColor != plateColor) { topPlateColor = stackedPlates[i].PlateColor; break; }

            tempList.Add(stackedPlates[i]);
            SaveDataInternally(stackedPlates[i], false);
            stackBuilder.ReleasePoint();
            stackedPlates.RemoveAt(i);
        }
        if (stackedPlates.Count == 0) 
        { 
            tempStackPoint = stackPoint; 
            topPlateColor = PlateColor.Empty;
            stackBuilder.ClearAllPoints();
        }
        
        limmitBar.SetValue(stackBuilder.TotalLimit, stackBuilder.CurrentStacklimit);
        cleaning = false;
        return tempList;

    }
    public override TransformData GetPosRot()
    {
        Transform point = stackBuilder.GetPoint();
        TransformData data = new TransformData();
        data.Pos = point.position;
        data.Rot = point.eulerAngles;
        return data;
    }
    public override void SelectUnit()
    {
        selectVisual.SetActive(true);
        InvokeDeleget();
    }
    public override void ClearUnit()
    {
        selectVisual.SetActive(false);
        // if (cureentCount >= holdCount) { StartCoroutine(ClearTray()); }
    }
    public override int GetLimit()
    {
        return stackBuilder.GetLimit();
    }
    public override void SaveDataInternally(Plate plate, bool add)
    {
        if (add)
        {
            dataHolder.stackedObjectsType.Add(plate.PlateColor);
        }
        else
        {
            dataHolder.stackedObjectsType.Remove(plate.PlateColor);
        }

        DataManager.Instance.SaveStackData(dataHolder);
    }
    public override void LoadDataInternally()
    {
        DataManager.Instance.LoadStackData(dataHolder);

        foreach (var item in dataHolder.stackedObjectsType)
        {
            Plate plate = RefrenceManager.Instance.SpawnManager.SpawnPlate(item, RefrenceManager.Instance.PlatesDistrubutor.transform);
            //plate.transform.DOMove(GetPosRot().Pos, 0.1f);
            TransformData data = GetPosRot();
            plate.transform.position = data.Pos;
            plate.transform.eulerAngles = data.Rot;
            stackedPlates.Add(plate);
            topPlateColor = plate.PlateColor;
            limmitBar.SetValue(stackBuilder.TotalLimit, stackBuilder.CurrentStacklimit);
        }
    }

    public override void SetDeleget(Action action)
    {
        onUnitselect = action;
    }
    private void InvokeDeleget()
    {
        onUnitselect?.Invoke();
        onUnitselect = null;
    }

    private void UpdateEarningCounter()
    {
        EarningCounter++;
        if(EarningCounter >= 15)
        {
            EarningCounter = 0;
            CoinsToGive += 1;
        }
    }
    private int CoinsToGive
    {
        get
        {
            return PlayerPrefs.GetInt("cleanEarning", 1);
        }
        set
        {
            PlayerPrefs.SetInt("cleanEarning",value);
        }
    }
    private int EarningCounter
    {
        get
        {
            return PlayerPrefs.GetInt("earningCounter", 1);
        }
        set
        {
            PlayerPrefs.SetInt("earningCounter", value);
        }
    }
}
