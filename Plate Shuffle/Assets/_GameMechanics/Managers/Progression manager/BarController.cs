using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BarController : MonoBehaviour
{
    [Header("New Plate Number Text")]
    [SerializeField] TextMeshProUGUI numberText;
 
    [Header("New Plate Visual Image Container")]
    [SerializeField] Image plateVisual;
    [Header("FIll Image")]
    [SerializeField] Image progressBar;

    public void UpdateValue(float value,System.Action action)
    {
        progressBar.DOFillAmount(value, 0.3f).OnComplete(() =>
        {
            action?.Invoke();
        });
    }

    public void SetPlateVisual(Sprite palteVis)
    {
        plateVisual.sprite = palteVis;
    }

    public void SetPlateNumber(int num)
    {
        numberText.text = num.ToString();
    }
}
