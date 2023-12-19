using System.Collections;
using UnityEngine;

[System.Serializable]
public class SaveData {

	public static SaveData Instance;

	public int Level = 0;
	public int Coins = 0;
	public bool LockDataInitialized = false;
	public bool FirstTimeDeal = false;
	public bool InitalProgression = false;
	public int HighestPlateCoolor = 0;
	public int WashPlatesProgress = 0;
	public bool TutorialRunning = true;
	public int CurrentTutorialNumber = 0;



	public string hashOfSaveData;
	
	
	public bool RemoveAds = false;

	//Constructor to save actual GameData
	public SaveData () { }

	//Constructor to check any tampering with the SaveData
	public SaveData (bool ads, int levels, int coins,bool lockDataInitialized,bool firstTimeDeal,bool initialProgression,int highestPlateColor,int washPlatesProgress,bool tutorialRunning,int currentTutorial) {
		RemoveAds = ads;
		Level = levels;
		Coins = coins;
		LockDataInitialized = lockDataInitialized;
		FirstTimeDeal = firstTimeDeal;
		InitalProgression = initialProgression;
		HighestPlateCoolor = highestPlateColor;
		WashPlatesProgress = washPlatesProgress;
		TutorialRunning = tutorialRunning;
		CurrentTutorialNumber = currentTutorial;
 
	}

}