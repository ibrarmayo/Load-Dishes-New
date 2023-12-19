using RootMotion.FinalIK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaiterAnimator : MonoBehaviour
{
    private readonly int walk = Animator.StringToHash("Walk");
    private readonly int idle = Animator.StringToHash("Idle");


    [SerializeField] Animator animator;
    [SerializeField] FullBodyBipedIK ikControll;

    public void Move()
    {
        animator.SetTrigger(walk);
    }

    public void SetIdle()
    {
        animator.SetTrigger(idle);
    }

    public void SetIK(bool active)
    {
        ikControll.enabled = active;
    }

}
