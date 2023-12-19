using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitLockData", menuName = "ImtiazScriptables/UnitLockDataObject")]
public class UnitLockDataHolder : DataHolder
{
    public UnitLockStateData[] StackUnitLockStates;
    public UnitLockStateData[] WashUnitLockStates;

    public override void ClearData()
    {
        Array.Clear(StackUnitLockStates, 0, StackUnitLockStates.Length);
        Array.Clear(WashUnitLockStates, 0, WashUnitLockStates.Length);
        
    }
}
