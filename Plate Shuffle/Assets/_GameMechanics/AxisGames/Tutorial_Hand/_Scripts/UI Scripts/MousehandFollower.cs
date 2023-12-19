using UnityEngine;
using DG.Tweening;
using AxisGames.BasicGameSet;

public class MousehandFollower : MonoBehaviour
{
    [Header("----- Main Canvas Refrence -----")]
    [SerializeField] Canvas myCanvas;

    [Space]
    [SerializeField] GameObject handobj;
    [SerializeField] DOTweenAnimation handAnimation;
    [SerializeField] bool hideHand = true;

    [Space]
    [Header("----- Hand Click Behaviour -----")]
    [SerializeField] bool onClick     = false;
    [SerializeField] bool followMouse = false;

    Vector2 pos;

    private void Awake()
    {
        HideHand();

        if (followMouse)
        {
            ShowHand();
        }


    }

    void Update()
    {
        if (onClick) { ClickBehavior(); }
        if (followMouse) { FollowMouseBehaviour(); }
    }

    private void ClickBehavior()
    {
        
            if (Input.GetMouseButtonDown(0))
            {
                handobj.SetActive(true);
                
                RectTransformUtility.ScreenPointToLocalPointInRectangle(myCanvas.transform as RectTransform, Input.mousePosition, myCanvas.worldCamera, out pos);
                transform.position = myCanvas.transform.TransformPoint(pos);
                handAnimation.DORestart();
            }
    }

    private void FollowMouseBehaviour()
    {
        if (Input.GetMouseButtonDown(0)) { TweenHand(true);  }
        if (Input.GetMouseButtonUp(0))   { TweenHand(false); }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(myCanvas.transform as RectTransform, Input.mousePosition, myCanvas.worldCamera, out pos);
        transform.position = myCanvas.transform.TransformPoint(pos);
    }

    public void ShowHand()
    {
        if (hideHand)
        {
            handAnimation.gameObject.SetActive(true);
        }
    }

    public void HideHand()
    {
        if (hideHand)
        {
            handAnimation.gameObject.SetActive(false);
        }
    }

    private void TweenHand(bool push = false)
    {
        if (push) { transform.DOScale(new Vector3(0.9f, 0.9f, 0.9f),0.3f); }
        else { transform.DOScale(Vector3.one,0.3f); }
    }

}
