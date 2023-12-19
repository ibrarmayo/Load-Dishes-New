using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AxisGames.Singletons;

public class TutorialManager : SingletonLocal<TutorialManager>
{
    [SerializeField] int currentTutorial;
    [Space]
    [SerializeField] TutorialProfile[] profiles;

    protected override void Awake()
    {
        if (SaveData.Instance.TutorialRunning)
        {
            base.Awake();
            currentTutorial = SaveData.Instance.CurrentTutorialNumber;
            RunTutorial();
        }
    }

    public void RunTutorial()
    {
        StartCoroutine(StartTutorial(0.01f));
    }

    IEnumerator StartTutorial(float startTime)
    {
        yield return new WaitForSeconds(startTime);
        profiles[currentTutorial].RunTutorial();
    }

    public void MoveToNextTutorial()
    {
        currentTutorial++;
        
        if(currentTutorial > profiles.Length - 1)
        {
            SaveData.Instance.TutorialRunning = false;
            GSF_SaveLoad.SaveProgress();
            Debug.LogWarning("Tutorials Completed");
            return;
        }

        SaveData.Instance.CurrentTutorialNumber = currentTutorial;
        GSF_SaveLoad.SaveProgress();
        StartCoroutine(StartTutorial(0.01f));
    }

    public bool CheckRequirment()
    {
        return profiles[currentTutorial].RequirmentMeet();
    }
}
