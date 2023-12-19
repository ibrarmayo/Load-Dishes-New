using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
public class Concent : MonoBehaviour
{
    [Header("COncent Dialog Panel")]
    [SerializeField] GameObject dialog;

    [Space]
    [Header("Loading Panel Refrences")]
    [SerializeField] float startWaitTime = 5f;
    [SerializeField] TextMeshProUGUI LoadingPrecentText;
    [SerializeField] Image loadingBarFill;

    [Space]
    [Header("Policy Links")]
    [SerializeField] string PrivacyPolicyLink;
    [SerializeField] string TermConditionsLink;
    
    AsyncOperation _async;

    private void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Application.targetFrameRate = 60;
        
        InitializeGame();

        if (!IsFirstTimeInstall)
        {
            dialog.SetActive(true);
        }
        else
        {
            dialog.SetActive(false);
            StartCoroutine(loadScene());
        }

    }
    void InitializeGame()
    {
        if (!GameManager.Instance.Initialized)
        {
            SaveData.Instance = new SaveData();
            GSF_SaveLoad.LoadProgress();
            GameManager.Instance.Initialized = true;
        }
        LoadingPrecentText.text = "0%";
    }

    #region SceneLoad Methods ----------------------

    IEnumerator loadScene()
    {
        loadingBarFill.gameObject.SetActive(true);

        yield return new WaitForSecondsRealtime(startWaitTime);
        
        _async = SceneManager.LoadSceneAsync(1);

    }
    private void Update()
    {
        //if (_async!= null)
        //{
        //    loadingBarFill.fillAmount = _async.progress;
        //}

        LoadingPrecentText.text = (loadingBarFill.fillAmount* 100).ToString("##") + "%";
    }

    #endregion

    #region Concent Methods ----------------

    public void openPrivacyPolicy()
    {
        Application.OpenURL(PrivacyPolicyLink);
    }

    public void openTermPolicy()
    {
        Application.OpenURL(TermConditionsLink);
    }

    public void Accept()
    {
        IsFirstTimeInstall = true;

        dialog.SetActive(false);
        StartCoroutine(loadScene());
    }

    #endregion

    #region Prefs -------------------

    private bool IsFirstTimeInstall
    {
        get
        {
            return (PlayerPrefs.GetInt("isfirsttimeinstall", 0) == 1) ? true : false;
        }
        set
        {
            PlayerPrefs.SetInt("isfirsttimeinstall", (value ? 1 : 0));
        }
    }

    #endregion
}
