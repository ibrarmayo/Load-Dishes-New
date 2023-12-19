using UnityEngine;
using AxisGames.BasicGameSet;
using AxisGames.Singletons;

public class RefrenceManager : SingletonLocal<RefrenceManager>
{

    // Refrences --------------------

    [Header("------- Main Camera -------")]
    [SerializeField] Camera _mainCam;

    [Header("------- Level Manager -------")]
    [SerializeField] LevelManager _levelManager;
    [Space]
    [Header("------- UI Manager -------")]
    [SerializeField] UIManager _uiManager;
    [Space]
    [Header("------- Global Event System -------")]
    [SerializeField] GlobalEventSystem _globalEventSystem;
    [Space]
    [Header("------- SpawnManager -------")]
    [SerializeField] SpawnManager _spawnManager;
    [Space]
    [Header("------- Lock Manager -------")]
    [SerializeField] LockManager _lockManager;
    [Space]
    [Header("------- Plate Distributor -------")]
    [SerializeField] PlatesDistrubutor _platesDistrubutor;
    [Space]
    [Header("------- Progress Manager -------")]
    [SerializeField] ProgressManager _progressManager;

    [Header("------- Material -------")]
    public Material CleanPlateMat;
    // Properties --------------------

    public Camera                Camera              { get => _mainCam; } 
    public LevelManager          LevelManager        { get => _levelManager; }
    public UIManager             UiManager           { get => _uiManager; }
    public GlobalEventSystem     GlobalEvents        { get => _globalEventSystem; }
    public SpawnManager          SpawnManager        { get => _spawnManager; }
    public LockManager           LockManager         {  get => _lockManager; }
    public PlatesDistrubutor     PlatesDistrubutor   { get => _platesDistrubutor; }
    public ProgressManager       ProgressManager     { get => _progressManager; }

    protected override void Awake()
    {
        base.Awake();

        InitializeGame();
#if (!UNITY_EDITOR)
        Application.targetFrameRate = 60;
#endif
        Vibration.Init();
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

}
