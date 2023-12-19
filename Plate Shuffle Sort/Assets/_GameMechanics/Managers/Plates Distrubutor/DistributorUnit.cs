using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;

public class DistributorUnit : MonoBehaviour
{
    [Header("Character Animator")]
    [SerializeField] WaiterAnimator waiterAnimator; 

    [Space]
    [Header("Stack Data --")]
    [SerializeField] Transform stackPoint;
    [SerializeField]private Transform tempStackPoint;
    [SerializeField] List<Plate> stackedPlates;
    
    public Transform StackPoint => stackPoint;
    
    private float plateOffset = 0.03f;
    int count;

    private void Awake()
    {
        tempStackPoint = stackPoint;
    }

    public void AddPlate(Plate plate)
    {
        plate.transform.DOMove(GetPosRot().Pos, 0.1f);
        stackedPlates.Add(plate);
        count = stackedPlates.Count;
    }

    public void AddPlateQuickly(Plate plate)
    {
        //plate.transform.DOMove(GetPosRot().Pos, 0.1f);
        //plate.transform.position = GetPosRot().Pos;
        plate.transform.position = stackPoint.position;
        stackedPlates.Add(plate);
        count = stackedPlates.Count;
    }

    public Plate GetPlate()
    {
        count--;
        if(count < 0) { return null; }
        Plate temp = stackedPlates[count];
        return temp;
    }

    public void ClearStack(bool destroyExtraPlates = false)
    {
        stackedPlates.Clear();
        count = 0;
        tempStackPoint = stackPoint;
    }

    public void MoveUnit(Transform target,float moveSPeed,Action onReachAction)
    {
        waiterAnimator.Move();
        transform.DOMove(target.position, moveSPeed).OnComplete(() => { waiterAnimator.SetIdle(); onReachAction?.Invoke(); });
        transform.DORotateQuaternion(target.rotation, 0.2f);
    }
    public virtual TransformData GetPosRot()
    {
        if (stackedPlates.Count != 0)
        {
            tempStackPoint = stackedPlates[stackedPlates.Count - 1].transform;
        }
        TransformData data = new TransformData();
        data.Pos = new Vector3(tempStackPoint.position.x, tempStackPoint.position.y + plateOffset, tempStackPoint.position.z);
        data.Rot = tempStackPoint.eulerAngles;
        return data;
    }

    public void SetIK(bool active)
    {
        waiterAnimator.SetIK(active);
    }
}
