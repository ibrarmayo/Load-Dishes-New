using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StackData", menuName = "ImtiazScriptables/StackDataObject")]
public class StackDataHolder : DataHolder
{
	public List<PlateColor> stackedObjectsType = new List<PlateColor>();

	public override void ClearData()
	{
		stackedObjectsType.Clear();
	}
}
