using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UnitBase : MonoBehaviour
{
	#region Fields -----------
	[BoxGroup("Components")]
	[SerializeField] public StackDataHolder dataHolder;
	[BoxGroup("Components")]
	[SerializeField] protected LockHandler lockHandler;
    [BoxGroup("Components")]
    [SerializeField] protected StackBuilder stackBuilder;
    [BoxGroup("Components")]
	[SerializeField] protected Transform stackPoint;
    [BoxGroup("Components")]
    [SerializeField] GameObject stackFullWarning;
    [BoxGroup("Settings")]
	[SerializeField] protected float plateOffset;
	//[BoxGroup("Settings")]
	//[SerializeField] protected int totalStackLimit = 100;
    //[BoxGroup("Settings")]
    //[ReadOnly] [SerializeField] protected int currentStackLimmit = 0;

    [Space]
    [BoxGroup("Status")]
    public bool Opned = true;
	[BoxGroup("Status")]
	[ReadOnly] [SerializeField] protected bool stackFull = false;
    [BoxGroup("Status")]
	[ReadOnly]
	[SerializeField] protected PlateColor topPlateColor = PlateColor.Empty;
	[BoxGroup("Status")]
	[ReadOnly]
	[SerializeField] protected List<Plate> stackedPlates;
    [BoxGroup("Status")]
    [ReadOnly]
	[SerializeField] protected Transform tempStackPoint;

	protected System.Action onUnitselect;
	public LockHandler LockHandler { get => lockHandler; }
	#endregion
	public PlateColor TopPlate { get => topPlateColor; set => topPlateColor = value; }
	
	

	protected virtual void Awake()
	{
		tempStackPoint = stackPoint;
		//currentStackLimmit = stackBuilder.GetLimit();;
	}
	public abstract bool CanInteract(PlateColor plateColor);
	public abstract void SelectUnit();
	public abstract void ClearUnit();
	public abstract void AddPlate(Plate plate);
	public abstract List<Plate> GetPlates(PlateColor plateColor, int stackLimit);
	public abstract int GetLimit();
	public abstract void SaveDataInternally(Plate plate, bool add);
	public abstract void LoadDataInternally();
	public abstract void SetDeleget(System.Action action);

	public virtual TransformData GetPosRot()
	{
		if (stackedPlates.Count != 0)
		{
			tempStackPoint = stackedPlates[stackedPlates.Count - 1].transform;
		}
		TransformData data = new TransformData();
		data.Pos = new Vector3(tempStackPoint.position.x, tempStackPoint.position.y + plateOffset, tempStackPoint.position.z);
		data.Rot = tempStackPoint.eulerAngles;
		return data;
	}
	public virtual bool PlatesAvailable()
    {
		if(stackedPlates.Count > 0) { return true; }
        else { return false; }
    }

	public virtual int PlatesCount()
    {
		return stackedPlates.Count;
    }

	protected void IncreaseStackCount()
	{
        if (stackBuilder.CurrentStacklimit >= stackBuilder.TotalLimit)
        {
            if (stackFullWarning && !stackFullWarning.activeSelf) { stackFullWarning.SetActive(true); }
            stackFull = true;
        }
	}
    protected void DecreaseStackCount()
    {
        stackBuilder.ReleasePoint();
        if (stackBuilder.CurrentStacklimit >= 0)
        {
            if (stackFullWarning && stackFullWarning.activeSelf) { stackFullWarning.SetActive(false); }
            stackFull = false;
        }
    }
}
