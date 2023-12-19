using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomSettings : MonoBehaviour
{
	[SerializeField] private List<Transform> targets, list01, list02;

	[Button(ButtonSizes.Small)]
	public void SetPositions()
	{
		for (int i = 0; i < targets.Count; i++)
		{
			list01[i].transform.position = targets[i].position;
			list02[i].transform.position = targets[i].position;
		}
	}
}
