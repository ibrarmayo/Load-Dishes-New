using AxisGames.BasicGameSet;
using AxisGames.ParticleSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockManager : MonoBehaviour
{
    public static System.Action<List<UnitBase>> OnUnitUnlocked;

    [Header("Unit Lock Holder--")]
    [SerializeField] UnitLockDataHolder unitLockDataHolder;

    [Space(5)]
    [Header("Unit Refrences--")]
    [SerializeField] List<UnitBase> stackUnits;
    [SerializeField] List<UnitBase> washUnits;

    [Header("Unit Lock Data--")]
    [Space(5)]
    [SerializeField] UnitLockStateData[] stackUnitLockStates;
    [SerializeField] UnitLockStateData[] washUnitLockStates;
    [Header("Unit Lock Dependency Data--")]
    [Space(5)]
    [SerializeField] UnitLockDependency[] unitLockDependencies;

    private void Awake()
    {
        InitializeData();

        SetUnitsData(stackUnits, stackUnitLockStates);
        SetUnitsData(washUnits, washUnitLockStates);

        SetDependencies();
    }
    private void Start()
    {
        SendUnlockEvent();
    }

    private void GameController_onHome()
    {
        SendUnlockEvent();
    }

    #region Saving / Loading -------

    private void InitializeData()
    {
        if (!SaveData.Instance.LockDataInitialized)
        {
            SaveData.Instance.LockDataInitialized = true;
            GSF_SaveLoad.SaveProgress();
            SaveUnitsData();
        }
        else
        {
            LoadUnitsData();
        }
    }

    private void LoadUnitsData()
    {
        DataManager.Instance.LoadStackData(unitLockDataHolder);

        stackUnitLockStates = unitLockDataHolder.StackUnitLockStates;
        washUnitLockStates = unitLockDataHolder.WashUnitLockStates;

        SendUnlockEvent();
    }

    private void SaveUnitsData()
    {
        unitLockDataHolder.StackUnitLockStates = stackUnitLockStates;
        unitLockDataHolder.WashUnitLockStates = washUnitLockStates;

        DataManager.Instance.SaveStackData(unitLockDataHolder);
    }

    #endregion

    public List<UnitBase> GetStackUnits()
    {
        List<UnitBase> units = new List<UnitBase>();
        
        for (int i = 0; i < stackUnits.Count; i++)
        {
            if (!stackUnits[i].LockHandler.Locked)
            {
                units.Add(stackUnits[i]);
            }
        }
        return units;
    }

    public void UnlockUnit(UnitBase unit)
    {
        if (stackUnits.Contains(unit))
        {
            UnlockItem(stackUnits, stackUnitLockStates, unit);
            SendUnlockEvent();
            SetDependencies();
        }
        else if (washUnits.Contains(unit))
        {
            UnlockItem(washUnits, washUnitLockStates, unit);
        }
        ParticleManager.Instance.PlayParticle(ParticleType.StackUnlock, unit.transform.position);
    }

    private void SendUnlockEvent()
    {
        List<UnitBase> units = new List<UnitBase>();

        for (int i = 0; i < stackUnits.Count; i++)
        {
            if (!stackUnits[i].LockHandler.Locked)
            {
                units.Add(stackUnits[i]);
            }
        }
        OnUnitUnlocked?.Invoke(units);
    }

    private void UnlockItem(List<UnitBase> units, UnitLockStateData[] lockStates,UnitBase unit)
    {
        int id = units.IndexOf(unit);
        lockStates[id].locked = false;
        unit.LockHandler.Unlock();
        SaveUnitsData();
    }

    private void SetUnitsData(List<UnitBase> units,UnitLockStateData[] lockStateData)
    {
        if (units.Count != lockStateData.Length) { Debug.LogError("Data DoesNot Match Plz Check provided data In Inspector for Units !!"); return; }

        for (int i = 0; i < units.Count; i++)
        {
            units[i].LockHandler.SetStatus(lockStateData[i]);
        }
    }

    private void SetDependencies()
    {
        for (int i = 0; i < unitLockDependencies.Length; i++)
        {
            if (unitLockDependencies[i].parentLock.LockHandler.Locked)
            {
                if(!unitLockDependencies[i].parentLock.LockHandler.Dependent) unitLockDependencies[i].parentLock.Opned = false;
                unitLockDependencies[i].parentLock.LockHandler.SetLocked();

                SetStatus(unitLockDependencies[i].childLock1, locked: true);
                SetStatus(unitLockDependencies[i].childLock2, locked: true);
            }
            else
            {
                SetStatus(unitLockDependencies[i].childLock1, locked: false);
                SetStatus(unitLockDependencies[i].childLock2, locked: false);
            }
        }
    }

    private void SetStatus(UnitBase unit,bool locked)
    {
        if (unit == null) return;
        if (locked)
        {
            unit.LockHandler.SetLocked();
            unit.Opned = false;
        }
        else
        {
            unit.LockHandler.SetUnlockable();
            unit.Opned = true;
        }
    }

    private void OnDestroy()
    {
        OnUnitUnlocked = null;
    }

}
