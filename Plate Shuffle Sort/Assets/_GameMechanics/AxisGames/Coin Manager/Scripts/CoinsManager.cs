using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using Sirenix.OdinInspector;
using AxisGames.BasicGameSet;
using AxisGames.Singletons;

public class CoinsManager : SingletonLocal<CoinsManager>
{
    public static System.Action<int> OnCoinsUpdated;

    [BoxGroup("Coin Status")]
    [ReadOnly]
    [SerializeField] int Coins;
    [BoxGroup("Script Refrences")]
    [SerializeField] SceneLoad sceneLoad;

    [BoxGroup("Coin UI Refrences")]
    [TabGroup("Coin UI Refrences/t1", "Coin Text")]
    //[SerializeField] Text coinText;
    [SerializeField] TextMeshProUGUI coinText;
    [TabGroup("Coin UI Refrences/t1", "Coin Text")]
    [SerializeField] DOTweenAnimation coinTextAnim;


    [TabGroup("Coin UI Refrences/t1", "Animated Coin Data")]
    [InfoBox("Set Up to Make Animated Coins in Game")]
    [SerializeField] bool UseAnimatedCoins;
    [TabGroup("Coin UI Refrences/t1", "Animated Coin Data")]
    [ShowIf("UseAnimatedCoins")]
    [SerializeField] GameObject animatedCoinPrefab;
    [TabGroup("Coin UI Refrences/t1", "Animated Coin Data")]
    [ShowIf("UseAnimatedCoins")]
    [SerializeField] Transform Container;
    [TabGroup("Coin UI Refrences/t1", "Animated Coin Data")]
    [ShowIf("UseAnimatedCoins")]
    [SerializeField] RectTransform target;
    [TabGroup("Coin UI Refrences/t1", "Animated Coin Data")]
    [ShowIf("UseAnimatedCoins")]
    [SerializeField] Transform StartPos;
    [TabGroup("Coin UI Refrences/t1", "Animated Coin Data")]
    [ShowIf("UseAnimatedCoins")]
    [SerializeField] RectTransform canvasRect;
    [TabGroup("Coin UI Refrences/t1", "Animated Coin Data")]
    [ShowIf("UseAnimatedCoins")]
    [Space(5)]
    [SerializeField] int maxCoins;
    Queue<GameObject> coinsQueue = new Queue<GameObject>();

    [TabGroup("Coin UI Refrences/t1", "Animated Coin Data")]
    [ShowIf("UseAnimatedCoins")]
    [SerializeField] [Range(0.5f, 0.9f)] float minAnimDuration;
    [TabGroup("Coin UI Refrences/t1", "Animated Coin Data")]
    [ShowIf("UseAnimatedCoins")]
    [SerializeField] [Range(0.9f, 2f)] float maxAnimDuration;

    [TabGroup("Coin UI Refrences/t1", "Animated Coin Data")]
    [ShowIf("UseAnimatedCoins")]
    [SerializeField] Ease easeType;
    [TabGroup("Coin UI Refrences/t1", "Animated Coin Data")]
    [ShowIf("UseAnimatedCoins")]
    [SerializeField] float spreadX = 0.3f;
    [TabGroup("Coin UI Refrences/t1", "Animated Coin Data")]
    [ShowIf("UseAnimatedCoins")]
    [SerializeField] float spreadY = 0.3f;


    [BoxGroup("Particles", centerLabel: true)]
    [SerializeField] GameObject confiti;
    [BoxGroup("Particles", centerLabel: true)]
    [SerializeField] ParticleSystem dimondParticle;

    /// <summary>
    /// Privae Variables
    /// </summary>
    int BonusPoint;
    int CollectedCoins = 1;
    int CollectedFood = 0;
    int Points = 0;
    Vector3 targetPosition;
    int numCompleted = 0; // this is use by bonus coins
    int vibrateCount = 0;

    protected override void Awake()
    {
        base.Awake();

        if (UseAnimatedCoins) PrepareCoins();
        CheckPreviousCoins();
    }

    void PrepareCoins()
    {
        if (animatedCoinPrefab == null || Container == null) { Debug.LogError("Coin Refrences not Assign !!"); return; }
        GameObject coin;
        for (int i = 0; i < maxCoins; i++)
        {
            coin = Instantiate(animatedCoinPrefab);
            coin.transform.SetParent(Container);
            coin.transform.position = Container.position;
            coin.transform.localScale = Vector3.one;
            coin.SetActive(false);
            coinsQueue.Enqueue(coin);
        }
    }

    public bool CanDoTransaction(int amount)
    {
        if (Coins >= amount)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void DeductCoins(int price)
    {
        Coins -= price;

        UpdateText(Coins);
        //coinText.text = Coins.ToString();

        if (!coinTextAnim.hasOnComplete) coinTextAnim.DORestart();
        if (dimondParticle) dimondParticle.Play();
        SaveCoins();
        OnCoinsUpdated?.Invoke(SavedCoins);

        coinText.color = Color.red;
        StartCoroutine(nameof(ResetTextColor));
    }

    IEnumerator ResetTextColor()
    {
        yield return new WaitForSeconds(0.15f);
        coinText.color = Color.white;
        StopCoroutine(nameof(ResetTextColor));
    }

    public void CollectCoin(int value, bool saveCurrentCoins = true)
    {
        Coins += value;
        if (saveCurrentCoins) CollectedCoins += value;
        //Points++;
        if (!coinTextAnim.hasOnComplete) coinTextAnim.DORestart();
        //coinText.text = Coins.ToString();
        UpdateText(Coins);
        SaveCoins();
        OnCoinsUpdated?.Invoke(SavedCoins);

        if (SaveData.Instance.TutorialRunning)
        {
            if (TutorialManager.instance.CheckRequirment())
            {
                //Debug.Log("FInal Tutorial");
                TutorialManager.instance?.RunTutorial();
            }
        }
    }

    public void TestAddCash(int amount)
    {
        Coins += amount;
        CollectedCoins += amount;
        if (!coinTextAnim.hasOnComplete) coinTextAnim.DORestart();
        UpdateText(Coins);
        SaveCoins();
        OnCoinsUpdated(SavedCoins);
    }

    public void CollectFood(int value)
    {
        CollectedFood += value;
    }

    public int GetCollectedCoins()
    {
        return CollectedCoins;
    }

    public int GetCollectedFood()
    {
        return CollectedFood;
    }

    private void UpdateText(int amount)
    {
        if (amount >= 1000)
        {
            float amu = (amount / 1000f);
            //Debug.Log($"Amount >> { amu }");
            coinText.text = amu.ToString("F") + "K";
        }
        else
        {
            coinText.text = amount.ToString();
        }

    }

    public void SaveCoins()
    {
        SavedCoins = Coins;
    }

    private void CheckPreviousCoins()
    {
        if (SavedCoins == 0 && InitialCash == 0)
        {
            InitialCash = 1;
            Coins = int.Parse(coinText.text);
            SavedCoins = Coins;
        }
        else
        {
            Coins = SavedCoins;
            //coinText.text = Coins.ToString();
            UpdateText(Coins);
        }
    }

    void Animate(int amount, Transform  startPosition, int toAdd)
    {
        targetPosition = target.position;
        for (int i = 0; i < amount; i++)
        {
            //check if there's coins in the pool
            if (coinsQueue.Count > 0)
            {
                //extract a coin from the pool
                GameObject coin = coinsQueue.Dequeue();

                Vector2 viewportPosition = Camera.main.WorldToViewportPoint(startPosition.position);
                Vector2 screenPosition = new Vector2(((viewportPosition.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f)), ((viewportPosition.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f)));
                var rectTransform = coin.GetComponent<RectTransform>();
                rectTransform.localPosition = screenPosition;
                //rectTransform.localRotation = new Quaternion(0, 0, 0, 0);

                //coin.transform.position = position;
                coin.SetActive(true);

                //animate coin to target position
                float duration = Random.Range(minAnimDuration, maxAnimDuration);

                coin.transform.DOLocalMove(screenPosition + new Vector2
                                                (Random.Range(-spreadX, spreadX),
                                                Random.Range(-spreadY, spreadY)), duration).OnComplete(() =>
             {

                 coin.transform.DOMove(targetPosition, duration)
                 .SetEase(easeType)
                 .OnComplete(() =>
                 {
                     //executes whenever coin reach target position
                     coin.SetActive(false);
                     coinsQueue.Enqueue(coin);
                     CollectCoin(toAdd,true);
                 });
             });
            }
        }
    }

    private void RestartScene()
    {
        if (confiti) confiti.SetActive(false);
        DOTween.KillAll();
        StopAllCoroutines();
        sceneLoad.ReloadScene();
    }
    public void AddCoins(int animateCount, int totalCash, Transform spawnPoint = null)
    {
        Transform startPoint = StartPos;

        if (spawnPoint) { startPoint = spawnPoint; }
        numCompleted = animateCount; // used to keep trake of animation completed to restart scene
        Animate(animateCount, startPoint, totalCash);
    }

    private void OnDestroy()
    {
        SaveCoins();
        OnCoinsUpdated = null;
    }

    public static int SavedCoins
    {
        get
        {
            return PlayerPrefs.GetInt("savedcoins");
        }
        set
        {
            PlayerPrefs.SetInt("savedcoins", value);
        }
    }

    public static int InitialCash
    {
        get
        {
            return PlayerPrefs.GetInt("InitialCash");
        }
        set
        {
            PlayerPrefs.SetInt("InitialCash", value);
        }
    }



}
