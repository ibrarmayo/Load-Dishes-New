using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class PlatesDistrubutor : MonoBehaviour
{
    #region Fields -----------------------------------------

    [BoxGroup("Distributor Unit Components")]
    [SerializeField] private DistributorUnit distributorUnit;
    [BoxGroup("Distributor Unit Components")]
    [SerializeField] private Transform startPoint;
    [BoxGroup("Distributor Unit Components")]
    [SerializeField] private Transform endPoint;

    [BoxGroup("Stack Unit List")]
    [SerializeField] private List<UnitBase> stackUnits;

    [BoxGroup("Settings")]
    [SerializeField] private int platesperUnit;
    [BoxGroup("Settings")]
    [SerializeField] private float timePerPlate = 0.01f;
    [BoxGroup("Settings")]
    [SerializeField] private float timePerPlateDistribute = 0.1f;
    [BoxGroup("Settings")]
    [SerializeField] private float charMoveInSpeed;
    [BoxGroup("Settings")]
    [SerializeField] private float charMoveOutSpeed;

    private System.Action OnDealReady;
    private System.Action OnWaiterReset;
    private ProgressManager progressManager;
    private SpawnManager spawnManager;
    private Plate plate;
    
    private bool busy;
    private int totalPlates = 0;
    private int count = 0;
    private int limmit;

    #endregion

    private void Awake()
    {
        progressManager = RefrenceManager.Instance.ProgressManager;
        spawnManager = RefrenceManager.Instance.SpawnManager;

        LockManager.OnUnitUnlocked += LockManager_OnUnitUnlocked;
    }

    public StackUnit GetUnit()
    {
        StackUnit unit = stackUnits[0].GetComponent<StackUnit>();

        for (int i = 0; i < stackUnits.Count; i++)
        {
            if (stackUnits[i].PlatesCount() > unit.PlatesCount())
            {
                unit = stackUnits[i].GetComponent<StackUnit>();
            }
        }

        if (!unit.PlatesAvailable()) { unit = null; }
        return unit;
    }

    private void LockManager_OnUnitUnlocked(List<UnitBase> units)
    {
        stackUnits = units;
    }
    public void MakeDeal(System.Action onDealReady, System.Action onWaiterReset)
    {
        OnDealReady = onDealReady;
        OnWaiterReset = onWaiterReset;

        if (busy) return;
        //StartCoroutine(PreparePlateDeal(true)); // previous Method which uses a Character to serve plates.
        StartCoroutine(DirectDistributePlates()); // new method to directly serve plates on units.
    }

    #region Waiter Serving Method ---------------

    IEnumerator PreparePlateDeal(bool moveAfterReady = false)
    {
        SetBusy();

        int totalPlates = stackUnits.Count * platesperUnit;
        int count = 0;

        for (int i = 0; i < totalPlates; i++)
        {
            int numberPerColor = Random.Range(1, 4);
            PlateColor colorCode = (PlateColor)Random.Range(1, progressManager.MaxUnlockedPlate);

            for (int j = 0; j < numberPerColor; j++)
            {
                if (count >= totalPlates) { break; }
                count++;
                plate = spawnManager.SpawnPlate(colorCode, distributorUnit.StackPoint);
                distributorUnit.AddPlateQuickly(plate);
                yield return new WaitForSeconds(timePerPlate);
            }
            if (count >= totalPlates) { break; }
        }

        if (moveAfterReady) MoveTowardsCounter();
    }
    private void MoveTowardsCounter()
    {
        OnDealReady?.Invoke();
        distributorUnit.SetIK(true);
        distributorUnit.MoveUnit(endPoint, charMoveInSpeed, () => { StartCoroutine(StartDistribution()); });
    }
    IEnumerator StartDistribution()
    {
        SoundManager.Instance.PlayOneShot(SoundManager.Instance.dealSound, 0.5f);
        for (int i = 0; i < stackUnits.Count; i++)
        {
            for (int j = 0; j < platesperUnit; j++)
            {
                plate = distributorUnit.GetPlate();
                if (plate == null) { Debug.Log("Got Null Retrun"); break; }
                plate.transform.SetParent(transform);
                plate.MoveAndRotateTowardsDestination(stackUnits[i].GetPosRot(), stackUnits[i].AddPlate);
                yield return new WaitForSeconds(timePerPlateDistribute);
            }
        }
        distributorUnit.ClearStack();
        distributorUnit.SetIK(false);
        yield return new WaitForSeconds(0.1f);
        distributorUnit.MoveUnit(startPoint, charMoveOutSpeed, () => { SetFree(); OnWaiterReset?.Invoke(); });
    }

    #endregion

    IEnumerator DirectDistributePlates()
    {
        SetBusy();

        if (CheckifUnitsFree())
        {
            totalPlates = stackUnits.Count * platesperUnit;
            count = 0;

            for (int i = 0; i < stackUnits.Count; i++)
            {
                if (stackUnits[i].GetLimit() <= 0)
                {
                    totalPlates -= platesperUnit;
                }
                
            }

            // Make plates according to available units.
            for (int i = 0; i < totalPlates; i++) 
            {
                int numberPerColor = Random.Range(1, 4);
                PlateColor colorCode = (PlateColor)Random.Range(1, progressManager.MaxUnlockedPlate);

                for (int j = 0; j < numberPerColor; j++)
                {
                    if (count >= totalPlates) { break; }
                    count++;
                    plate = spawnManager.SpawnPlate(colorCode, distributorUnit.StackPoint);
                    distributorUnit.AddPlateQuickly(plate);
                }
                if (count >= totalPlates) { break; }
            }

            SoundManager.Instance.PlayOneShot(SoundManager.Instance.dealSound, 0.5f);

            // Distribute plates per unit.
            for (int i = 0; i < stackUnits.Count; i++) 
            {
                limmit = stackUnits[i].GetLimit();

                for (int j = 0; j < platesperUnit; j++)
                {
                    if (stackUnits[i].GetLimit() <= 0 || limmit <= 0) { break; }
                    
                    plate = distributorUnit.GetPlate();
                    if (plate == null) { break; }
                    
                    limmit--;
                    plate.transform.SetParent(transform);
                    plate.MoveAndRotateTowardsDestination(stackUnits[i].GetPosRot(), stackUnits[i].AddPlate);
                    
                    yield return new WaitForSeconds(timePerPlateDistribute);
                }
            }
            
            distributorUnit.ClearStack();
            yield return new WaitForSeconds(0.1f);
            SetFree();
            OnWaiterReset?.Invoke();
        }
        else
        {
            Debug.Log("No Units Free");
            yield return new WaitForSeconds(1f);
            SetFree();
            OnWaiterReset?.Invoke();
        }
    }

    private bool CheckifUnitsFree()
    {
        int unitsfree = stackUnits.Count;
        bool free = true;

        for (int i = 0;i < stackUnits.Count; i++)
        {
            if (stackUnits[i].GetLimit() <= 0) { unitsfree--; }
        }

        if(unitsfree <= 0) { free = false; }

        return free;
    }

    private void SetBusy() { busy = true; }
    private void SetFree() { busy = false; }
}
