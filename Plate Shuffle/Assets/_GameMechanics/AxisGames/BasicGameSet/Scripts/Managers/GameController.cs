using System;
using UnityEngine;

namespace AxisGames
{
	namespace BasicGameSet
	{
		public enum GameState
		{
			Home,
			Gameplay,
			Complete,
			Fail
		}

		public class GameController : MonoBehaviour
		{

			[SerializeField] GameState gameState = GameState.Home;
			public static Action<GameState> changeGameState;

			public static event Action onHome, onGameplay, onLevelComplete, onLevelFail;

			protected void Awake()
			{
				//Application.targetFrameRate = 60;
				//Vibration.Init();
				
				//InitializeGame(); // enable if not using refrence Manager.... (Reason is this iitialization is done there because that script is executed first in order..)

				changeGameState += ChangeGameState;
			}
			private void Start()
			{
				changeGameState?.Invoke(gameState);
			}

			void InitializeGame()
			{
				if (!GameManager.Instance.Initialized)
				{
					SaveData.Instance = new SaveData();
					GSF_SaveLoad.LoadProgress();
					GameManager.Instance.Initialized = true;
				}
			}

			void ChangeGameState(GameState state)
			{
				gameState = state;
				switch (gameState)
				{
					case GameState.Home:
						onHome?.Invoke();
						break;

					case GameState.Gameplay:
						onGameplay?.Invoke();
						break;

					case GameState.Complete:
						onLevelComplete?.Invoke();
						break;

					case GameState.Fail:
						onLevelFail?.Invoke();
						break;
				}
			}
			void OnDestroy()
			{
				onLevelComplete = null;
				changeGameState = null;
				onLevelFail = null;
				onGameplay = null;
				onHome = null;
			}
        }
	}
}