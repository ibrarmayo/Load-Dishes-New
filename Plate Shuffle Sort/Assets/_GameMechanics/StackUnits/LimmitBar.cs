using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LimmitBar : MonoBehaviour
{
    [SerializeField] GameObject FillerObject;
    [SerializeField] Image fillBar; 

    public void SetValue(int  max,int current)
    {
        FillerObject.SetActive(true);

        float amt = (float) current / max;
        fillBar.DOKill();
        fillBar.DOFillAmount(amt, 0.2f);
    }

    public void ResetValue()
    {
        fillBar.DOKill();
        fillBar.DOFillAmount(0, 0.2f).OnComplete(() => { FillerObject.SetActive(false); });
    }
}
