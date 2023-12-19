using UnityEngine;
using DG.Tweening;
using TMPro;

public class DealButton : MonoBehaviour
{
    [SerializeField] PlatesDistrubutor platesDistrubutor;
    [SerializeField] DOTweenAnimation animation;

    [Header("Button Render")]
    [SerializeField] MeshRenderer buttonRender;
    [Space]
    [SerializeField] Material enableMat;
    [SerializeField] Material disableMat;

    [Space]
    [Header("Button Push Animation Settings")]
    [SerializeField] Transform buttonObject;
    [SerializeField] TextMeshProUGUI buttonStateText;
    [Space]
    [SerializeField] Vector3 buttonDownPos;
    [SerializeField] Vector3 buttonUpPos;
    [Space]
    [SerializeField] float animationSpeed = 0.3f;
    bool isActive = true;

    System.Action onButtonPress;

    public void BuyDeal()
    {
        if (!isActive) return;
        isActive = false;
        SetButtonPos(buttonDownPos, () => 
        {

            DOVirtual.DelayedCall(0.5f, () => { SetButtonPos(buttonUpPos, null); });

        });
        SetButtonState(false);
        FireDeleget();

        platesDistrubutor.MakeDeal(WhenServingPlates, ReleaseButton);
        SoundManager.Instance.PlayOneShot(SoundManager.Instance.stackSelect, 1f);
    }

    private void WhenServingPlates()
    {
        SetButtonPos(buttonUpPos, null);
    }

    private void ReleaseButton()
    {
        //SetButtonPos(buttonUpPos, () => { isActive = true; SetButtonState(true);/*SetText("Deal");*/ });
        isActive = true;
        SetButtonState(true);
    }

    public void SetButtonPos(Vector3 tPos, System.Action action)
    {
        buttonObject.DOLocalMove(tPos, animationSpeed).OnComplete(() => { action?.Invoke(); });
    }

    private void SetText(string text)
    {
        if (buttonStateText) buttonStateText.text = text;
    }

    private void SetButtonState(bool active)
    {
        if (active)
        {
            buttonRender.material = enableMat;
        }
        else
        {
            buttonRender.material = disableMat;
        }
    }

    public void SetDeleget(System.Action onPress)
    {
        onButtonPress = onPress;
    }

    private void FireDeleget()
    {
        onButtonPress?.Invoke();
        onButtonPress = null;
    }
}
