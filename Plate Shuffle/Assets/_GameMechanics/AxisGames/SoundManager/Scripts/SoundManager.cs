using UnityEngine;
using Sirenix.OdinInspector;
using AxisGames.BasicGameSet;
using AxisGames.Singletons;

public class SoundManager : SingletonLocal<SoundManager>
{

    [BoxGroup("--- Audio Sources ---", centerLabel: true)]
    [SerializeField] AudioSource bgSoundSource;
    [BoxGroup("--- Audio Sources ---")]
    [SerializeField] AudioSource buttonClickSource;
    [BoxGroup("--- Audio Sources ---")]
    [SerializeField] AudioSource effectSource;
    [BoxGroup("--- Audio Sources ---")]
    [SerializeField] AudioSource mainSoundSource;


    [BoxGroup("--- Audio Clips ---")]
    [TabGroup("--- Audio Clips ---/t1", "Main Event Souds")]
    public AudioClip stackSelect;
    [TabGroup("--- Audio Clips ---/t1", "Main Event Souds")]
    public AudioClip platesFlip;
    [TabGroup("--- Audio Clips ---/t1", "Main Event Souds")]
    public AudioClip stackUnlock;
    [TabGroup("--- Audio Clips ---/t1", "Main Event Souds")]
    public AudioClip dealSound;
    [TabGroup("--- Audio Clips ---/t1", "Main Event Souds")]
    public AudioClip cashPick;
    [TabGroup("--- Audio Clips ---/t1", "Main Event Souds")]
    public AudioClip washing;
    [TabGroup("--- Audio Clips ---/t1", "Main Event Souds")]
    public AudioClip skinUnlock;
    [TabGroup("--- Audio Clips ---/t1", "Main Event Souds")]
    public AudioClip platesDestroy;

    bool isGamePlayMode;
    bool backgroundState;

    protected override void Awake()
    {
        base.Awake();

        buttonClickSource.enabled = false;

        //GameController.onHome          += GameController_onHome;
        //GameController.onGameplay      += GameController_onGameplay;
        //GameController.onLevelFail     += GameController_onLevelFail;
        //GameController.onLevelComplete += GameController_onLevelComplete;

        isGamePlayMode = true;
        backgroundState = true;
        CheckBGState();
    }

    private void GameController_onHome()
    {
        isGamePlayMode = true;
        bgSoundSource.volume = 0.2f;
        CheckBGState();
    }

    private void GameController_onLevelComplete()
    {
        //bgSoundSource.enabled = false;
        bgSoundSource.volume = 0.2f;
    }

    private void GameController_onLevelFail()
    {
        bgSoundSource.enabled = false;
    }

    private void GameController_onGameplay()
    {
        isGamePlayMode = true;
        bgSoundSource.volume = 0.15f;
        //CheckBGState();

        //bgSoundSource.Play();
    }

    public void EnableAudio(bool active)
    {
        mainSoundSource.enabled   = active;
        backgroundState           = active;
        //bgSoundSource.enabled     = active;
        effectSource.enabled      = active;

        CheckBGState();

        //FunctionTimer.Create(() => { buttonClickSource.enabled = active; }, 1f);
    }

    private void CheckBGState()
    {
        if (isGamePlayMode) 
        {
            if(backgroundState == true)
            {
                if (bgSoundSource.isActiveAndEnabled) { bgSoundSource.Play(); }
                else
                {
                    bgSoundSource.enabled = true;
                    bgSoundSource.Play();
                }
            }
            else
            {
                if (bgSoundSource.isPlaying) { bgSoundSource.Pause(); }
            }
        }
    }

    public void PlayMainSounds(AudioClip clip, float volume)
    {
        mainSoundSource.PlayOneShot(clip, volume);
    }

    public void PlayOneShot(AudioClip clip, float volume)
    {
        effectSource.PlayOneShot(clip, volume);
    }

    public void PlayButtonSound()
    {
        buttonClickSource.Play();
    }

}
