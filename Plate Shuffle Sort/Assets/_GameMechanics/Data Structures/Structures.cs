using UnityEngine;

public class Structures
{
   
}

[System.Serializable]
public struct TransformData
{
    public Vector3 Pos;
    public Vector3 Rot;
}

[System.Serializable]
public struct UnitLockStateData
{
    public bool locked;
    public int amount;
}

[System.Serializable]
public struct UnitLockDependency
{
    public UnitBase parentLock;
    [Space]
    public UnitBase childLock1;
    public UnitBase childLock2;
}
