using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
	[SerializeField] SpawnDataDisctionary dataDisctionary;


	public Plate SpawnPlate(PlateColor color, Transform parent)
	{
		Plate plate = Instantiate(dataDisctionary.GetPlate(color), parent);
		return plate;
	}
}
