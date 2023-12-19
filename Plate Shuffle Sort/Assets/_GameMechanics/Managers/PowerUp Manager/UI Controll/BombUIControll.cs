using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using TMPro;

public class BombUIControll : MonoBehaviour
{
    [Header("Button components Refrence")]
    [SerializeField] private Button bombButton;
    [Space]
    [SerializeField] private Image bombFiller;
    [SerializeField] private Button fillerButton;
    [Space]
    [SerializeField] TextMeshProUGUI priceText;
    
    [Space]
    [SerializeField] private float delayDur;
    [ReadOnly]  [SerializeField] private bool isTweening;
    int price = 10;

    private void OnEnable()
    {
        CoinsManager.OnCoinsUpdated += CoinManager_OnCoinsUpdate;
        CoinManager_OnCoinsUpdate(CoinsManager.SavedCoins);
    }

    private void CoinManager_OnCoinsUpdate(int Coins)
    {
        //if (isTweening) return;
        if(Coins >= price)
        {
            EnableButton(true);
            fillerButton.interactable = true;
        }
        else
        {
            EnableButton(false);
            fillerButton.interactable = false;
        }
    }
    public void SetPrice(int price)
    {
        this.price = price;
        priceText.text = price.ToString();
        CoinManager_OnCoinsUpdate(CoinsManager.SavedCoins);
    }

    public void MakeButtonWaitForClick()
    {
        if (isTweening) return;
        isTweening = true;
        EnableButton(false);
        bombFiller.fillAmount = 0;
        fillerButton.interactable = true;
        bombFiller.DOFillAmount(1f, delayDur).OnComplete(()=> {
            isTweening = false;
            CoinManager_OnCoinsUpdate(CoinsManager.SavedCoins);
            bombFiller.fillAmount = 1f;
        });
    }


    private void EnableButton(bool active)
    {
        bombButton.interactable = active;
    }
}
