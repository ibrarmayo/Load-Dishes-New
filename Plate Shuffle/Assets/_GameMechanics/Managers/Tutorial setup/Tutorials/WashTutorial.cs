using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WashTutorial : TutorialProfile
{
    public override void RunTutorial()
    {
        unitsToControll[0].Opned = true;
        unitsToControll[0].SetDeleget(() => { StartCoroutine(NextTutorial()); });
        unitsToControll[0].GetComponent<LayerChanger>().ChangeLayerForTutorial(8);

        EnableUI(true);
        tutorialUiList[1].SetActive(false);
        MoveHandToPoint(unitsToControll[0].transform);

    }

    IEnumerator NextTutorial()
    {
        EnableUI(false);
        //Debug.Log("Compleeted");
        yield return new WaitForSeconds(4f);
        TutorialManager.Instance.MoveToNextTutorial();
        unitsToControll[0].GetComponent<LayerChanger>().ChangeLayerForTutorial(6);
    }

    private void EnableUI(bool active)
    {
        focusBar.SetActive(active);
        tutorialHand.gameObject.SetActive(active);
        tutorialUiList[0].SetActive(active);
        tutorialPanel.gameObject.SetActive(active);
    }


}
