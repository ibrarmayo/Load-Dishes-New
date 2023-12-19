using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitUnlockTutorial : TutorialProfile
{
    [Space]
    [SerializeField] int requiredAmount;
    [SerializeField] int unitToUnlock;

    public override void RunTutorial()
    {
        tutorialCanvas.gameObject.SetActive(true);
        tutorialUiList[0].SetActive(false);
        if (CoinsManager.SavedCoins >= requiredAmount)
        {
            Debug.Log("Unlock tutorial Sterted");
            EnableUI(true);
            unitsToControll[unitToUnlock].LockHandler.SetDeleget(() => { StartCoroutine(MoveToNext()); });
            
            SetUnit(unitToUnlock);
            unitsToControll[unitToUnlock].GetComponent<LayerChanger>().ChangeLayerForTutorial(8);
            MoveHandToPoint(unitsToControll[unitToUnlock].transform);

        }


    }

    public override bool RequirmentMeet()
    {
        return CoinsManager.SavedCoins >= requiredAmount;
    }

    IEnumerator MoveToNext()
    {
        EnableUI(false);
        SetUnit(0, true);
        tutorialCanvas.gameObject.SetActive(false);
        TutorialManager.Instance.MoveToNextTutorial();
        unitsToControll[unitToUnlock].GetComponent<LayerChanger>().ChangeLayerForTutorial(6);
        yield return new WaitForSeconds(2f);
        tutorialUiList[0].SetActive(true);
    }

    private void EnableUI(bool active)
    {
        focusBar.SetActive(active);
        tutorialHand.gameObject.SetActive(active);
        tutorialPanel.gameObject.SetActive(active);
    }

    private void SetUnit(int toUnlock, bool enableAll = false)
    {
        for (int i = 0; i < unitsToControll.Length; i++)
        {
            if (i == toUnlock || enableAll)
            {
                unitsToControll[i].Opned = true;
            }
            else
            {
                unitsToControll[i].Opned = false;
            }
        }
    }
}
