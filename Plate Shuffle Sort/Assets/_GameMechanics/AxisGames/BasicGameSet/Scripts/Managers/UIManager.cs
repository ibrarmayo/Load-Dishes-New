using System.Collections;
using UnityEngine;
using UnityEngine.UI;
//using SupersonicWisdomSDK;

namespace AxisGames
{
    namespace BasicGameSet
    {
        public class UIManager : MonoBehaviour
        {
            [Header("----- Main Panel UIs ----------")]
            [SerializeField] GameObject levelNumberPanel;
            [SerializeField] GameObject homePanel;
            [SerializeField] GameObject gamplayPanel;
            [SerializeField] GameObject completePanel;
            [SerializeField] GameObject levelFailPanel;
            [SerializeField] GameObject coinBarPanel;

            [Header("----- Text Fields --------------")]
            [SerializeField] Text levelNoText;

            // Private Variables
            int levelNo;

            protected void Awake()
            {
                GameController.onLevelComplete += OnLevelComplete;
                GameController.onGameplay += Gameplay;
                GameController.onLevelFail += LevelFail;
                GameController.onHome += Home;

                SetLevelText();
            }

            //Subscribed Events Definations
            #region Game Events ---

            void Home()
            {
               // ActiveMainPanel(home: true,levelPanel:true);
            }

            void Gameplay()
            {
                #region Analtics----------------------- 
                //AnalyticsMediator.instance?.LogGA_Event("LevelStarted", levelNo);
                //Debug.Log("Level Started : " + levelNo,gameObject);
                #endregion

                ActiveMainPanel(gameplay: true, coinPanel:true);
            }

            #region Complete Panle Logic ---

            void OnLevelComplete()
            {
                #region Analtics----------------------- 
                //AnalyticsMediator.instance?.LogGA_Event("LevelCompleted:", levelNo);
                //Debug.Log("Level Completed : " + levelNo);
                #endregion

                StartCoroutine(LoadCompletePanel(1f, 1f, showAd: false));
            }

            private IEnumerator LoadCompletePanel(float adTime, float panelTime, bool showAd)
            {
                //if (showAd)
                //{
                //    yield return new WaitForSeconds(1f);
                //    Mediation_Manager.instance?.Show_Interstital();
                //}
                yield return new WaitForSeconds(panelTime);
                ActiveMainPanel(complete: true, coinPanel: true);
                //SoundManager.Instance.PlayMainSounds(SoundManager.Instance.Win, 0.4f);
            }

            #endregion

            #region Fail Panel Logic ---
            void LevelFail()
            {
                #region Analtics----------------------- 
                //AnalyticsMediator.instance?.LogGA_Event("LevelFailed", levelNo);
                //Debug.Log("Level Completed : " + levelNo);
                #endregion

                StartCoroutine(LoadFailPanel(1f, 1f, showAd: false));
            }

            private IEnumerator LoadFailPanel(float adTime, float panelTime, bool showAd)
            {
                //if (showAd)
                //{
                //    yield return new WaitForSeconds(1f);
                //    Mediation_Manager.instance?.Show_Interstital();
                //}

                yield return new WaitForSeconds(panelTime);
                //SoundManager.Instance.PlayMainSounds(SoundManager.Instance.Fail, 0.4f);
                ActiveMainPanel(fail: true);
            }

            #endregion

            #endregion

            private void SetLevelText()
            {
                levelNo = SaveData.Instance.Level;
                levelNo += 1;
                levelNoText.text = $"Level {levelNo.ToString("00")}";
            }

            //Active Panels
            void ActiveMainPanel(bool levelPanel = false, bool gameplay = false, bool home = false, bool complete = false, bool fail = false, bool coinPanel = false)
            {
                levelNumberPanel.SetActive(levelPanel);
                gamplayPanel.SetActive(gameplay);
                homePanel.SetActive(home);
                completePanel.SetActive(complete);
                levelFailPanel.SetActive(fail);
                coinBarPanel?.SetActive(coinPanel);
            }

            // Buttons 
            public void TapToPlay()
            {
                GameController.changeGameState.Invoke(GameState.Gameplay);
            }

        }
    }
}