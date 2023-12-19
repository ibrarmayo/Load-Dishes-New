#if SW_STAGE_STAGE1_OR_ABOVE
using System;
using UnityEngine;
#if UNITY_IOS
using UnityEngine.iOS;
#endif

namespace SupersonicWisdomSDK
{
    public class SwStage1TesterManager : MonoBehaviour
    {
        #region --- Members ---

        public bool isIOS14;
        public RuntimePlatform currentOS;

        public SwStage1TesterState currentSwStage1TesterState;
        public string sysVersion;
        public string OperationSystem;

        #endregion


        #region --- Properties ---

        public static SwStage1TesterManager Instance { get; private set; }

        #endregion


        #region --- Mono Override ---

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            OperationSystem = SystemInfo.operatingSystem;
            currentOS = Application.platform;

            if (currentOS == RuntimePlatform.IPhonePlayer)
            {
#if UNITY_IOS
                sysVersion = Device.systemVersion.Split(new[] { '.' }, StringSplitOptions.None)[0];

                if (int.Parse(sysVersion) >= 14)
                {
                    Debug.Log("Running iOS 14 and above!");
                    isIOS14 = true;
                }
#endif
            }
        }

        #endregion


        #region --- Public Methods ---

        public void SaveStateToPrefs(SwStage1TesterState mSwStage1TesterState)
        {
            PlayerPrefs.SetString("Age", mSwStage1TesterState.Age.ToString());
            PlayerPrefs.SetString("TodaySessionsCount", mSwStage1TesterState.TodaySessionsCount.ToString());
            PlayerPrefs.SetString("TotalSessionsCount", mSwStage1TesterState.TotalSessionsCount.ToString());
            PlayerPrefs.SetString("Level", mSwStage1TesterState.Level.ToString());
        }

        public SwStage1TesterState GetStateFromPrefs()
        {
            var cState = new SwStage1TesterState();

            if (long.TryParse(PlayerPrefs.GetString("Age"), out var resultAge))
            {
                cState.Age = resultAge;
            }

            if (long.TryParse(PlayerPrefs.GetString("TodaySessionsCount"), out var resultTodaySC))
            {
                cState.TodaySessionsCount = resultTodaySC;
            }

            if (long.TryParse(PlayerPrefs.GetString("TotalSessionsCount"), out var resultTSC))
            {
                cState.TotalSessionsCount = resultTSC;
            }

            if (long.TryParse(PlayerPrefs.GetString("Level"), out var resultLevel))
            {
                cState.Level = resultLevel;
            }

            return cState;
        }

        public void CreateState(SwStage1TesterState mSwStage1TesterState)
        {
            currentSwStage1TesterState = mSwStage1TesterState;
            SaveStateToPrefs(mSwStage1TesterState);
            Debug.Log("GM: " + currentSwStage1TesterState);
        }

        #endregion


        #region --- Inner Classes ---

    }

    #endregion
}
#endif