using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class GSF_SaveLoad {

	public static void SaveProgress()
	{
		SaveData.Instance.hashOfSaveData = HashGenerator(SaveObjectJSON());
		string saveDataHashed = JsonUtility.ToJson(SaveData.Instance, true);

		if (GameManager.Instance.isSaveDataTesting)
		{
			File.WriteAllText(GetSavePath(), saveDataHashed);
		}
		else
		{

			PlayerPrefs.SetString("Json", saveDataHashed);
			PlayerPrefs.Save();
		}
	}

	public static SaveData SaveObjectCreator () {
        SaveData CheckSave = new SaveData(SaveData.Instance.RemoveAds, SaveData.Instance.Level,
                                           SaveData.Instance.Coins,
                                           SaveData.Instance.LockDataInitialized,
                                           SaveData.Instance.FirstTimeDeal,
                                           SaveData.Instance.InitalProgression,
                                           SaveData.Instance.HighestPlateCoolor,
                                           SaveData.Instance.WashPlatesProgress,
                                           SaveData.Instance.TutorialRunning,
                                           SaveData.Instance.CurrentTutorialNumber
                                           );
		return CheckSave;
	}

	public static string SaveObjectJSON () {
		string saveDataString = JsonUtility.ToJson (SaveObjectCreator (), true);
		return saveDataString;
	}

    public static void LoadProgress()
    {
        if (GameManager.Instance.isSaveDataTesting)
        {
            if (File.Exists(GetSavePath()))
            {
                string fileContent = File.ReadAllText(GetSavePath());
                JsonUtility.FromJsonOverwrite(fileContent, SaveData.Instance);

                LogMessage("Game Load Successful --> " + GetSavePath());
            }
            else
            {
                LogMessage("New Game Creation Successful --> " + GetSavePath());
                SaveProgress();
            }
        }
        else
        {
            if (File.Exists(GetSavePath()))
            {
                LogMessage("Old User Get data From Json");

                string fileContent = File.ReadAllText(GetSavePath());
                PlayerPrefs.SetString("Json", fileContent);
                PlayerPrefs.Save();
                JsonUtility.FromJsonOverwrite(fileContent, SaveData.Instance);
                DeleteProgress();
                
                LogMessage("Delete Json File For Old User and Save Data In player Pref");
                LogMessage("Game Load Successful --> " + GetSavePath());
            }
            else if (PlayerPrefs.HasKey("Json"))
            {
                string fileContent = PlayerPrefs.GetString("Json");
                JsonUtility.FromJsonOverwrite(fileContent, SaveData.Instance);
                
                LogMessage("Old User Get data From Player Prefs");
            }
            else
            {
                LogMessage("New Game Creation Successful --> " + GetSavePath());
                SaveProgress();
            }
        }
    }

    public static string HashGenerator (string saveContent) {
		SHA256Managed crypt = new SHA256Managed ();
		string hash = string.Empty;
		byte[] crypto = crypt.ComputeHash (Encoding.UTF8.GetBytes (saveContent), 0, Encoding.UTF8.GetByteCount (saveContent));
		foreach (byte bit in crypto) {
			hash += bit.ToString ("x2");
		}
		return hash;
	}

	public static void DeleteProgress () {
		if (File.Exists (GetSavePath ())) 
        {
			File.Delete (GetSavePath ());
		}
	}

	private static string GetSavePath () {
		return Path.Combine (Application.persistentDataPath, "SavedGame.json");
	}

    private static void LogMessage(string message)
    {
#if UNITY_EDITOR
        Debug.Log(message);
#endif
    }
}