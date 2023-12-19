using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SortTutorial : TutorialProfile
{
    [SerializeField] BoxCollider dealButtonColider;
    [Space]
    [SerializeField] PlatesSpawnData[] spawndata;
    [Space]
    [SerializeField] int currentTutorial = 0;
    [SerializeField] int currentUnit = 0;

    [System.Serializable]
    private struct PlatesSpawnData
    {
        public PlateColor color;
        public int platesToSpawn;
        public StackUnit unit;
    }

    public override void RunTutorial()
    {
        tutorialCanvas.gameObject.SetActive(true);
        dealButtonColider.enabled = false;


        EnableUI(true);
        SetText(currentTutorial);

        SetUnit(currentUnit);
        unitsToControll[currentUnit].SetDeleget(()=> { StartCoroutine(SecondUnit()); } );
        LoadPlates();

        tutorialHand.gameObject.SetActive(true);
        MoveHandToPoint(unitsToControll[currentUnit].transform);
    }

    IEnumerator SecondUnit()
    {
        EnableUI(false);
        SetText(currentTutorial,true);
        yield return new WaitForSeconds(0.5f);

        currentUnit++;
        currentTutorial++;
        EnableUI(true);
        SetText(currentTutorial);

        SetUnit(currentUnit);
        unitsToControll[currentUnit].SetDeleget(() => { StartCoroutine(SelectAgainUnit()); });

        MoveHandToPoint(unitsToControll[currentUnit].transform);
    }

    IEnumerator SelectAgainUnit()
    {
        EnableUI(false);
        SetText(currentTutorial, true);
        yield return new WaitForSeconds(0.5f);
        
        currentTutorial++;
        EnableUI(true);
        SetText(currentTutorial);

        unitsToControll[currentUnit].SetDeleget(MoveToNextTutorial);

        tutorialHand.gameObject.SetActive(true);
        MoveHandToPoint(unitsToControll[currentUnit].transform);
    }

    private void MoveToNextTutorial()
    {
        //Debug.Log("Compleeted");
        EnableUI(false);
        SetUnit(currentUnit,true);
        SetText(currentTutorial, true);
        TutorialManager.Instance.MoveToNextTutorial();
    }

    private void EnableUI(bool active)
    {
        focusBar.SetActive(active);
        tutorialHand.gameObject.SetActive(active);
        tutorialPanel.gameObject.SetActive(active);
    }
    private void SetText(int toUnlock,bool disableAll = false)
    {
        //Debug.Log("Text UI Method");
        for (int i = 0; i < tutorialUiList.Length; i++)
        {
            if(i == toUnlock && !disableAll)
            {
                //Debug.Log("Text UI On");
                tutorialUiList[i].SetActive(true);
            }
            else
            {
                tutorialUiList[i].SetActive(false);
            }
        }
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
    private void LoadPlates()
    {
        for (int i = 0; i < spawndata.Length; i++)
        {
            spawndata[i].unit.SpawnPlates(spawndata[i].platesToSpawn, spawndata[i].color,8);
        }
    }

    
}
