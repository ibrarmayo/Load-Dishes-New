using UnityEngine;
using UnityEngine.SceneManagement;

namespace AxisGames
{
	namespace BasicGameSet
	{
		public class SceneLoad : MonoBehaviour
		{

			[SerializeField] string sceneName;

			public void LoadScene()
			{
				if (string.IsNullOrEmpty(sceneName))
				{
					return;
				}

				SceneManager.LoadSceneAsync(sceneName);
			}

			public void ReloadScene()
			{
				if (string.IsNullOrEmpty(sceneName))
				{
					sceneName = SceneManager.GetActiveScene().name;
				}

				SceneManager.LoadSceneAsync(sceneName);
			}

        }
	}
}