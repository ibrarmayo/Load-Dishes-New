using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;

public class WashHandController : MonoBehaviour
{
    [Header("Hand Components")]
    [SerializeField] Transform washhand;
    [SerializeField] HandAnimController handAnimator;

    [Space]
    [Header("Wash Particle")]
    [SerializeField] ParticleSystem washParticle;

    [Space]
    [Header("Hand Move Poses")]
    [SerializeField] Transform startPos;
    [SerializeField] Transform washTarget;

    public void MoveHandToWash(Action onhandReached)
    {
        SetwahHand(true);
        washhand.DOMove(washTarget.position, 0.4f).OnComplete(() => 
        {
            handAnimator.SetWashing();
            washParticle.Play();  
            onhandReached?.Invoke(); 
        });
    } 

    public void Resethand()
    {
        handAnimator.SetIdle();
        washParticle.Stop();

        washhand.DOMove(startPos.position, 0.3f).OnComplete(() =>
        {
            SetwahHand(false);
        });
    }

    private void SetwahHand(bool active)
    {
        washhand.gameObject.SetActive(active);
    }

}
