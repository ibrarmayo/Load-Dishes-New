using DG.Tweening;
using UnityEngine;

public abstract class TutorialProfile : MonoBehaviour
{
    [SerializeField] protected Canvas tutorialCanvas;
    [Space]
    [SerializeField] protected GameObject focusBar;
    [SerializeField] protected GameObject tutorialPanel;
    [Space]
    [SerializeField] protected Transform tutorialHand;
    [SerializeField] protected DOTweenAnimation handAnimation;
    [Space]
    [SerializeField] protected UnitBase[] unitsToControll;
    [SerializeField] protected GameObject[] tutorialUiList;

    public abstract void RunTutorial();
    
    protected Vector2 SetUiPos(Transform hand,Transform target)
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(tutorialCanvas.transform as RectTransform, Input.mousePosition, tutorialCanvas.worldCamera, out pos);
        transform.position = tutorialCanvas.transform.TransformPoint(pos);

        return pos;
    } 

    protected void MoveHandToPoint(Transform targetPoint)
    {
        handAnimation.DORewind();
        tutorialHand.DOMove(targetPoint.position, 0.4f).OnComplete(() => { handAnimation.DORestart(); });
    }

    protected void SetCanvasVisiblity(bool active)
    {
        tutorialCanvas.gameObject.SetActive(active);
    }

    public virtual bool RequirmentMeet()
    {
        return false;
    }
}
