using AxisGames.Singletons;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class DataManager : SingletonLocal<DataManager>
{
	[Header("Meta")] public string persistentName;
	public List<DataHolder> stackDataToStore, washStackDataToStore = new();

	public void SaveStackData(DataHolder dataToSave)
	{
		var bf = new BinaryFormatter();
		var file = File.Create(Application.persistentDataPath + $"/{persistentName}_{dataToSave.name}.txt");
		var json = JsonUtility.ToJson(dataToSave, true);
		bf.Serialize(file, json);
		file.Close();
	}

	public void LoadStackData(DataHolder dataToLoad)
	{
		if (File.Exists(Application.persistentDataPath + $"/{persistentName}_{dataToLoad.name}.txt"))
		{
			var bf = new BinaryFormatter();
			var file = File.Open(Application.persistentDataPath + $"/{persistentName}_{dataToLoad.name}.txt",
				FileMode.Open);
			JsonUtility.FromJsonOverwrite((string)bf.Deserialize(file), dataToLoad);
			file.Close();
		}
		else
		{
			print("file doesn't exist");
		}
	}

	private void ResetAllData()
	{
		for (var i = 0; i < stackDataToStore.Count; i++)
		{
			var bf = new BinaryFormatter();
			var file = File.Create(Application.persistentDataPath + $"/{persistentName}_{stackDataToStore[i].name}.txt");
			var json = JsonUtility.ToJson(stackDataToStore[i], true);
			bf.Serialize(file, json);
			file.Close();
		}

		for (var i = 0; i < washStackDataToStore.Count; i++)
		{
			var bf = new BinaryFormatter();
			var file = File.Create(Application.persistentDataPath + $"/{persistentName}_{washStackDataToStore[i].name}.txt");
			var json = JsonUtility.ToJson(washStackDataToStore[i], true);
			bf.Serialize(file, json);
			file.Close();
		}
	}

	[Button(ButtonSizes.Medium)]
	public void ClearAllData()
	{
		PlayerPrefs.DeleteAll();
		PlayerPrefs.Save();

		foreach (var stack in stackDataToStore)
		{
			stack.ClearData();
		}

		foreach (var stack in washStackDataToStore)
		{
			stack.ClearData();
		}
		ResetAllData();
	}
}