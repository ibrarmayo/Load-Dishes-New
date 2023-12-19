using UnityEngine;
using TMPro;
using Sirenix.OdinInspector;

public class LockHandler : MonoBehaviour
{
    [BoxGroup("Lock Ui Refs")]
    [SerializeField] GameObject lockObject;
    [BoxGroup("Lock Ui Refs")]
    [SerializeField] TextMeshProUGUI amountText;

    [BoxGroup("Base Visual")]
    [SerializeField] GameObject lockedBase;
    [BoxGroup("Base Visual")]
    [SerializeField] GameObject unLockedBase;
    [BoxGroup("Base Visual")]
    [SerializeField] GameObject disabledBase;

    [BoxGroup("Settings")]
    [SerializeField] bool locked = false;
    [BoxGroup("Settings")]
    [SerializeField] int unlockAmount = 10;

    System.Action onUnitselect;
    bool dependent = false;

    public bool Dependent => dependent;

    public bool CanUnlock()
    {
        if (CoinsManager.Instance.CanDoTransaction(unlockAmount))
        {
            InvokeDeleget();
            CoinsManager.Instance.DeductCoins(unlockAmount);
            return true;
        }
        else
        {
            Debug.Log("Cant do");
            return false;
        }
    }

    public void SetStatus(UnitLockStateData data)
    {
        locked = data.locked;
        unlockAmount = data.amount;

        if (locked)
        {
            amountText.text = $"${unlockAmount}";
        }
        else
        {
            SetBase(unlocked: true);
            lockObject.SetActive(false);
        }
    }

    public void Unlock()
    {
        amountText.text = "Unlocked";
        locked = false;
        SetBase(unlocked: true);
        Invoke(nameof(DisableText), 1f);
    }

    private void DisableText()
    {
        lockObject.SetActive(false);
    }

    public void SetDependent()
    {
        dependent = true;
    }

    private void SetBase(bool locked = false,bool unlocked = false, bool disabled = false)
    {
        if(lockedBase) lockedBase.SetActive(locked);
        if(unLockedBase) unLockedBase.SetActive(unlocked);
        if(disabledBase) disabledBase.SetActive(disabled);
    }

    public void SetLocked()
    {
        if (dependent) return;
        if (!locked) return;
        SetBase(disabled: true);
        lockObject.SetActive(false);        
    }
    public void SetUnlockable()
    {
        if (!locked) return;
        SetBase(locked: true);
        SetDependent();
        lockObject.SetActive(true);
    }

    public bool Locked { get => locked; set => locked = value; }

    public void SetDeleget(System.Action action)
    {
        onUnitselect = action;
    }
    private void InvokeDeleget()
    {
        onUnitselect?.Invoke();
        onUnitselect = null;
    }
}
