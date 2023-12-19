#if SW_STAGE_STAGE1_OR_ABOVE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SupersonicWisdomSDK
{
    public class SwStage1TesterButtonsManager : MonoBehaviour
    {
        #region --- Members ---

        public Button ToggleBlockingLoaderButton;
        public Color myRed;
        public Color myGreen;
        public InputField Keywords;
        public LinkedList<string> mLog = new LinkedList<string>();
        public long level;
        public ScrollRect ScrollView;
        public Text StateText;
        public Text SDKStatusText;
        public Text WisdomVersion;
        public Text AppVersion;
        public Text OSText;
        public TextMeshProUGUI DebugLogText;
        public TextMeshProUGUI SmallDebugLogText;
        public TextMeshProUGUI Results;
        public Toggle SearchLogCase;

        #endregion


        #region --- Mono Override ---

        protected virtual void Start()
        {
            Keywords.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
            StateText.text = SwStage1TesterManager.Instance != null ? SwStage1TesterManager.Instance.currentSwStage1TesterState.ToString() : "";
            ColorUtility.TryParseHtmlString("#E77A6C", out myRed);
            ColorUtility.TryParseHtmlString("#79E76C", out myGreen);

            level = SwStage1TesterManager.Instance != null ? SwStage1TesterManager.Instance.currentSwStage1TesterState.Level : 0;
            Debug.Log("CurrentOS: " + SystemInfo.operatingSystem);
            Debug.Log("WisdomSDK Version: " + SwConstants.SdkVersion);
            AppVersion.text = Application.version;
            WisdomVersion.text = SwConstants.SdkVersion;
            OSText.text = SwStage1TesterManager.Instance != null ? SwStage1TesterManager.Instance.OperationSystem : SystemInfo.operatingSystem;
            SearchLogCase.onValueChanged.AddListener(delegate { ToggleValueChanged(SearchLogCase); });
        }

        protected virtual void Awake()
        {
            UpdateLogHandler();

            if (!SupersonicWisdom.Api.IsReady())
            {
                SupersonicWisdom.Api.Initialize(new Dictionary<string, object>
                {
                    [SwInitKeys.IsLegacyNoAds] = false,
                });
            }
        }

        #endregion


        #region --- Public Methods ---

        public void UpdateLogHandler()
        {
            Application.logMessageReceived -= HandleLog;

            if (PlayerPrefs.GetInt("LogOn", 0) == 1)
            {
                Application.logMessageReceived += HandleLog;
            }
        }

        public void ClearLog()
        {
            DebugLogText.text = "";
            SmallDebugLogText.text = "";
        }

        public void onConsentEventClicked()
        {
            // SupersonicWisdom.TrackConsentEvent(SwConsentEvent.Open);
            // Debug.Log("TrackConsentEvent: SwConsentEvent.Open sent!");
        }

        public void onGameAdEventClicked()
        {
            Debug.Log("TrackGameAdEvent: SwAdEvent.Open,SwAdType.Interstitial,WisdomTestApp sent!");
        }

        public void onGameProgressEventClicked()
        {
            Debug.Log("TrackGameProgressEvent: SwProgressEvent.GameStart,0 sent!");
        }

        public void onLevelCompletedClicked()
        {
#if SW_STAGE_STAGE3_OR_ABOVE
            level++;
            SupersonicWisdom.Api.NotifyLevelCompleted(level, () => { Debug.Log("Finished onLevelCompleted " + level); });
#endif
        }

        public void onLevelFailedClicked()
        {
#if SW_STAGE_STAGE3_OR_ABOVE
            SupersonicWisdom.Api.NotifyLevelFailed(level, () => { Debug.Log("Finished onLevelFailed " + level); });
#endif
        }

        public void onClearPrefsClicked()
        {
            StartCoroutine(ClearData());
        }

        public void ScrollToBottomClicked()
        {
            ScrollView.normalizedPosition = new Vector2(0, 0);
        }

        public void ScrollToTopClicked()
        {
            ScrollView.normalizedPosition = new Vector2(0, 1);
        }

        public void ToggleValueChanged(Toggle change)
        {
            ValueChangeCheck();
        }

        public List<string> SearchLog(LinkedList<string> log, string keyword)
        {
            var result = new List<string>();

            if (keyword != "")
            {
                if (SearchLogCase.isOn)
                {
                    foreach (var item in log)
                    {
                        if (item.Contains(keyword))
                        {
                            var newItem = item.Replace(keyword, $"<color=\"yellow\"><b>{keyword}</b></color>");
                            result.Add(newItem);
                        }
                    }
                }
                else
                {
                    foreach (var item in log)
                    {
                        if (item.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            var mItem = $"<color=\"yellow\"><b>{keyword}</b></color>";
                            var newResult = Regex.Replace(item, keyword, mItem, RegexOptions.IgnoreCase);
                            result.Add(newResult);
                        }
                    }
                }

                if (result.Count < 1)
                {
                    result.Add("No Logs");
                }
            }

            return result;
        }

        public IEnumerator RunAfterDelay(int delayInSeconds, Action callback)
        {
            yield return new WaitForSeconds(delayInSeconds);
            callback.Invoke();
        }

        #endregion


        #region --- Private Methods ---

        protected virtual IEnumerator ClearData()
        {
            Debug.Log("Clear Data Requested");

            Caching.ClearCache();
            Debug.Log("Cache has been cleared");
            PlayerPrefs.DeleteAll();
            Debug.Log("PlayerPrefs cleared.");
            Debug.Log("The app will quit automatically in 3 seconds...");

            yield return new WaitForSeconds(1);
            Debug.Log("The app will quit automatically in 2 seconds...");

            yield return new WaitForSeconds(1);
            Debug.Log("The app will quit automatically in 1 seconds...");

            yield return new WaitForSeconds(1);
            Application.Quit(0);
        }

        private void UpdateUILog(string line)
        {
            DebugLogText.text = line + DebugLogText.text;
            SmallDebugLogText.text = line + SmallDebugLogText.text;
        }

        private void ValueChangeCheck()
        {
            Results.text = string.Join("", SearchLog(mLog, Keywords.text));
        }

        #endregion


        #region --- Event Handler ---

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            string line;

            if (type == LogType.Exception)
            {
                line = DateTime.Now + "\nException: " + logString + "\nat:\n" + stackTrace + "\n\n\n";
            }
            else
            {
                line = DateTime.Now + "\n" + logString + "\n\n";
            }

            mLog.AddFirst(line);
            UpdateUILog(line);
        }

        #endregion
    }
}
#endif