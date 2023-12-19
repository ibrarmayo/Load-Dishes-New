using UnityEngine;
using UnityEngine.UI;

public class UnlockPopup : MonoBehaviour
{
    [Header("New Plate Visual Image")]
    [SerializeField] Image plateVisual;


    public void SetPlateVisualAndOpen(Sprite palteVis)
    {
        plateVisual.sprite = palteVis;
        gameObject.SetActive(true);
    }
}
