using AxisGames.ParticleSystem;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
	//[Header("Renderer--")]
	//[SerializeField] MeshRenderer renderer;
	//[Space]
	[Header("Settings--")]
	[SerializeField] float distanceBeforeImpact = 0.5f;
	[SerializeField] float time = 0.5f;
	[SerializeField] int jumpPower = 1;

	bool reached;

	public void MoveTowardsDestination(Vector3 destination, Action action)
	{
		transform.DOJump(destination, jumpPower, 1, time)
						   .OnUpdate(() =>
						   {
							   if (Vector3.Distance(destination, transform.position) <= distanceBeforeImpact && !reached)
							   {
								   reached = true;
                                   action?.Invoke();
								   ParticleManager.Instance.PlayParticle(ParticleType.Hit, destination);
                                   //Debug.Log("Colse to the Unit");
							   }
						   }
						   ).OnComplete(() =>
                           {
							   Destroy(gameObject);
                           });
    }
}
