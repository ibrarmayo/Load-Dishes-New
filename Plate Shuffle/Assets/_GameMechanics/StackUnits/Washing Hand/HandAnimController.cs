using UnityEngine;

public class HandAnimController : MonoBehaviour
{
    [SerializeField] Animator animator;

    private readonly int scrub = Animator.StringToHash("Scrub");
    private readonly int idle = Animator.StringToHash("Idle");

    public void SetWashing()
    {
        animator.SetTrigger(scrub);
    }

    public void SetIdle()
    {
        animator.SetTrigger(idle);
    }
}
