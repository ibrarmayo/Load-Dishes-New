using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace AxisGames
{
    namespace BasicGameSet
    {
        #region Structure For Extra Level Information
        [Serializable]
        public struct ExtraInfo
        {
            public Color fogColor;
            public Material color;
            public Camera camera;
        }
        #endregion

        #region Structure For Resource Loading
        [Serializable]
        public struct Path
        {
            public string URL;
            public LevelInfo[] levels;
        }

        #endregion

        #region Structure For Scene Data Loading
        [Serializable]
        public struct LevelInfo
        {
            public Transform levelObject;
            public ExtraInfo ExtraInfo;
        }

        #endregion

        public class LevelManager : MonoBehaviour
        {
            #region Fields ---------------------------

            [ReadOnly]
            [SerializeField] LevelInfo currentLevel;

            [BoxGroup("Custom Level Testing",centerLabel:true)]
            [ToggleLeft]
            [SerializeField] bool isTesting = false;

            [GUIColor(0, 1, 0)]
            [ShowIfGroup("isTesting")]
            [BoxGroup("isTesting/Enter the Level Number!",centerLabel:true)]
            [SerializeField] int customLevelNumber = 0;

            [Space()]
            [BoxGroup("---- Levels ----", centerLabel: true)]
            
            [DisableIf("loadFromResources")]
            [TabGroup("---- Levels ----/t1", "Scene Levels")]
            [SerializeField] bool loadFromScene;
            [EnableIf("loadFromScene")]
            [TabGroup("---- Levels ----/t1", "Scene Levels")]
            [SerializeField] LevelInfo[] levels;
            
            [DisableIf("loadFromScene")]
            [TabGroup("---- Levels ----/t1", "Resource Levels")]
            [SerializeField] bool loadFromResources;
            [EnableIf("loadFromResources")]
            [TabGroup("---- Levels ----/t1", "Resource Levels")]
            [SerializeField] int resourseLevelCount;
            [EnableIf("loadFromResources")]
            [TabGroup("---- Levels ----/t1", "Resource Levels")]
            [SerializeField] Path path;

            // Private Variables
            int levelNo;

            #endregion

            public void Awake()
            {
                GameController.onLevelComplete += OnLevelComplete;
                ActiveLevel();
            }

            void OnLevelComplete()
            {
                if (!isTesting)
                {
                    var levelNo = SaveData.Instance.Level;
                    levelNo++;
                    SaveData.Instance.Level = levelNo;
                    GSF_SaveLoad.SaveProgress();
                }
            }

            void ActiveLevel()
            {
                levelNo = GetLevelNumber();

                ValidateLevel(ref levelNo);

                CheckSceneLoadMethod();
            }

            #region Level Loading Metods--------
            private int GetLevelNumber()
            {
                if (!isTesting)
                {
                    return SaveData.Instance.Level;
                }
                else 
                { /// added some checks for Invalid Index input
                    if (loadFromScene)
                    {
                        if (customLevelNumber > levels.Length) { Debug.LogError("Invalid Index !.. State Reset "); return 0; }
                    }
                    else if(loadFromResources)
                    {
                        if (customLevelNumber > path.levels.Length) { Debug.LogError("Invalid Index !.. State Reset "); return 0; }
                    }
                    
                    return customLevelNumber; 
                }
            }
            private void ValidateLevel(ref int levelNo)
            {
                if (loadFromResources)
                {
                    if(resourseLevelCount <= 0) { Debug.LogError("Please define levels COunt!!"); }
                    CheckNumberisValid(ref levelNo, resourseLevelCount);
                }
                else
                {
                    CheckNumberisValid(ref levelNo, levels.Length);
                }
            }

            private void CheckNumberisValid(ref int levelNo,int length)
            {
                if (levelNo > length - 1)
                {
                    if (!isGameRepeating) { isGameRepeating = true; Debug.Log("Repeating Levels"); }
                    levelNo %= length;
                }
            }

            private void CheckSceneLoadMethod()
            {
                if (loadFromScene)
                {
                    LoadFromCurrentScene(levels[levelNo]);
                }
                else if (loadFromResources)
                {
                    if(!path.URL.Equals(string.Empty))
                    {
                        LoadFromResource(levelNo);
                    }
                    else { Debug.LogError("Path not Defined Or Wrong !!"); }
                }
                else
                {
                    if(levels.Length > 0)
                    {
                        LoadFromCurrentScene(levels[levelNo]);
                    }
                    else { Debug.LogError("Please Chose one Loading Scheme !!!"); }
                }
            }

            private void LoadFromCurrentScene(LevelInfo level)
            {
                currentLevel = level;
                currentLevel.levelObject.gameObject.SetActive(true);
            }

            private void LoadFromResource(int levelNo)
            {
                currentLevel = path.levels[levelNo];

                GameObject Level = Instantiate(Resources.Load(path.URL)) as GameObject;

                RenderSettings.fogColor = path.levels[levelNo].ExtraInfo.fogColor;
            }

            #endregion

            #region Prefsss---------------
            private bool isGameRepeating
            {
                get
                {
                    return (PlayerPrefs.GetInt("isrepeating", 0) == 1) ? true : false;
                }
                set
                {
                    PlayerPrefs.SetInt("isrepeating", (value ? 1 : 0));
                }
            }
            #endregion
        }
    }
}