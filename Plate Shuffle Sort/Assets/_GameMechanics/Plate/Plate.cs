using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plate : MonoBehaviour
{
	[Header("Renderer--")]
	[SerializeField] MeshRenderer renderer;
	[Space]
	[Header("Settings--")]
	[SerializeField] PlateColor color;
	[SerializeField] float time = 0.5f;
	[SerializeField] int jumpPower = 1;

	public PlateColor PlateColor { get => color; }

	public void SetTutorialLayer(int layer)
	{
		renderer.gameObject.layer = layer;
	}

	public void MoveTowardsDestination(Vector3 destination, Action<Plate> action)
	{
		transform.DOJump(destination, jumpPower, 1, time).OnComplete(() => { action?.Invoke(this); });
	}

	public void MoveAndRotateTowardsDestination(TransformData transformData, Action<Plate> addPlateAction)
	{
		transform.DOJump(transformData.Pos, jumpPower, 1, time).OnComplete(() => { addPlateAction?.Invoke(this); });
		transform.DORotate(transformData.Rot, time);
	}

	public void SetMaterial(Material mat)
    {
		renderer.material = mat;
    }
}
