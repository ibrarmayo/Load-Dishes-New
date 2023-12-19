using System.Collections;
using UnityEngine;

public class GameManager {

	private static GameManager instance;

	private GameManager () { }

	public static GameManager Instance {
		get {
			if (instance == null) {
				instance = new GameManager ();
			}
			return instance;
		}
	}

	public bool Initialized = false;
	public bool isSaveDataTesting = false;

#if UNITY_EDITOR
	public bool EditorSession = true;
#endif

}