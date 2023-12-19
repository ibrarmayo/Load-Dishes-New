using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealTutorial : TutorialProfile
{
    [Space]
    [SerializeField] DealButton dealbutton;
    [SerializeField] BoxCollider buttonCollider;

    public override void RunTutorial()
    {
        tutorialCanvas.gameObject.SetActive(true);
        EnableAllUnits();
        unitsToControll[3].Opned = false;
        unitsToControll[4].Opned = false;
        dealbutton.SetDeleget(() => { StartCoroutine(NextTutorial()); });
        buttonCollider.enabled = true;
        buttonCollider.gameObject.GetComponent<LayerChanger>().ChangeLayerForTutorial(8);
        tutorialUiList[1].SetActive(false);
        EnableUI(true);
        MoveHandToPoint(dealbutton.transform);
    }

    IEnumerator NextTutorial()
    {
        yield return new WaitForSeconds(0.01f);
        //Debug.Log("Compleeted");
        EnableUI(false);
        TutorialManager.Instance.MoveToNextTutorial();
        //yield return new WaitForSeconds(2f);
    }

    private void EnableUI(bool active)
    {
        focusBar.SetActive(active);
        tutorialHand.gameObject.SetActive(active);
        tutorialUiList[0].SetActive(active);
        tutorialPanel.gameObject.SetActive(active);
    }

    private void EnableAllUnits()
    {
        for (int i = 0; i < unitsToControll.Length; i++)
        {
            unitsToControll[i].Opned = true;
        }
    }
}
