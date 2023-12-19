using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class PowerupManager : MonoBehaviour
{
    [Header("UI Ref")]
    [SerializeField] BombUIControll bombUIControll;
    [Header("Bomb Powerup")]
    [SerializeField] Bomb bombPrefab;
    [Space]
    [SerializeField] Transform startPos;
    [SerializeField] int requiredAmount = 10;

    StackUnit unit;

    private void Awake()
    {
        SetPrice();
    }

    private void SetPrice()
    {
        bombUIControll.SetPrice(requiredAmount);
    }

    public void UseBomb()
    {
        if (CoinsManager.Instance.CanDoTransaction(requiredAmount))
        {
            unit = RefrenceManager.Instance.PlatesDistrubutor.GetUnit();
            if (unit == null) { Debug.Log("No stack with Full plates found !!"); return; }

            CoinsManager.Instance.DeductCoins(requiredAmount);
            unit.LockHandler.Locked = true;
            Bomb bomb = MakeBomb(startPos);

            bomb.MoveTowardsDestination(unit.transform.position, OnBombReach);
            SoundManager.Instance.PlayOneShot(SoundManager.Instance.stackSelect, 0.5f);
        }
    }

    private void OnBombReach()
    {
        SoundManager.Instance.PlayOneShot(SoundManager.Instance.platesDestroy, 1f);
        StartCoroutine(unit.DestroyPlates());
        unit = null;
    }


    private Bomb MakeBomb(Transform startPos)
    {
        return Instantiate(bombPrefab, startPos);
    }
}
