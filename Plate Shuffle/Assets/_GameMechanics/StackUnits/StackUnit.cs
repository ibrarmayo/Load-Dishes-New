using AxisGames.Singletons;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StackUnit : UnitBase
{
    [BoxGroup("Components")]
    [SerializeField] GameObject selectVisual;

    private void Start()
    {
        LoadDataInternally();
    }
    public IEnumerator DestroyPlates()
    {
        for (int i = stackedPlates.Count - 1; i >= 0; i--)
        {
            SaveDataInternally(stackedPlates[i], false);
            Destroy(stackedPlates[i].gameObject);
            //stackedPlates[i].gameObject.SetActive(false);
            stackedPlates.RemoveAt(i);
            DecreaseStackCount();
            yield return new WaitForSeconds(0.02f);
        }

        LockHandler.Locked = false;
        tempStackPoint = stackPoint;
        topPlateColor = PlateColor.Empty;
    }
    public override bool CanInteract(PlateColor plateColor)
    {
        if (plateColor == TopPlate)
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
        IncreaseStackCount();
        SaveDataInternally(plate, true);
    }
    public override void SelectUnit()
    {
        selectVisual.SetActive(true);
        InvokeDeleget();
    }
    public override void ClearUnit()
    {
        selectVisual.SetActive(false);
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
            DecreaseStackCount();
            stackedPlates.RemoveAt(i);
        }
        if (stackedPlates.Count == 0) { tempStackPoint = stackPoint; topPlateColor = PlateColor.Empty; }
        return tempList;
    }
    public override int GetLimit()
    {
        //return currentStackLimmit;
        return stackBuilder.GetLimit();
    }
    public override TransformData GetPosRot()
    {
        Transform point = stackBuilder.GetPoint();
        TransformData data = new TransformData();
        if (point)
        {
            data.Pos = point.position;
            data.Rot = point.eulerAngles;
        }
        return data;
    }
    public override void SaveDataInternally(Plate plate, bool add)
    {
        //Debug.Log("Saved Plated in Stack Unit");
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
            plate.transform.position = GetPosRot().Pos;
            stackedPlates.Add(plate);
            IncreaseStackCount();
            topPlateColor = plate.PlateColor;
        }
    }
    public void SpawnPlates(int count, PlateColor color, int layer = 0)
    {
        for (int i = 0; i < count; i++)
        {
            Plate plate = RefrenceManager.Instance.SpawnManager.SpawnPlate(color, RefrenceManager.Instance.PlatesDistrubutor.transform);
            plate.transform.position = GetPosRot().Pos;
            plate.SetTutorialLayer(layer);
            stackedPlates.Add(plate);
            IncreaseStackCount();
            topPlateColor = plate.PlateColor;
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
}
