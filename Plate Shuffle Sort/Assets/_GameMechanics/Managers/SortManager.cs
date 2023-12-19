using System.Collections;
using System.Collections.Generic;
using _GameMechanics.AxisGames.Core.WorldPositionInput;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _GameMechanics.Managers
{
    public class SortManager : MonoBehaviour
    {
        [SerializeField] private UnitBase selectedUnit;
        [SerializeField] private UnitBase targetUnit;
        [Space]
        [SerializeField]
        private float timePerPlate;
        [SerializeField] private List<Plate> tempPlates;
        private MouseWorldInput _mouseWorldInput;
        private bool _busy;
        
        private void Awake()
        {
            tempPlates = new List<Plate>();
        }
        
        private void Start()
        {
            _mouseWorldInput = FindObjectOfType<MouseWorldInput>();
        }

        private void Update()
        {
            if (_busy) return;

            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                var hitObject = _mouseWorldInput.GetHitObject();
                if (hitObject && hitObject.TryGetComponent<DealButton>(out DealButton button)) { button.BuyDeal(); return; }

                if (hitObject && hitObject.TryGetComponent<UnitBase>(out UnitBase unit))
                {
                    if (!unit.Opned) { return; }
                    if (unit.LockHandler.Locked)
                    {
                        if (unit.LockHandler.CanUnlock())
                        {
                            RefrenceManager.Instance.LockManager.UnlockUnit(unit);
                            SoundManager.Instance.PlayOneShot(SoundManager.Instance.stackUnlock, 0.5f);
                            Vibration.VibrateNope();
                            return;
                        }
                        return;
                    }

                    if (selectedUnit == null)
                    {
                        if (unit.TopPlate == PlateColor.Empty) { return; }
                        selectedUnit = unit;
                        selectedUnit.SelectUnit();
                        SoundManager.Instance.PlayOneShot(SoundManager.Instance.stackSelect, 0.5f);
                        Vibration.VibratePop();
                        return;
                    }

                    if (selectedUnit && selectedUnit == unit)
                    {
                        ResetData();
                        return;
                    }

                    if (selectedUnit && unit.CanInteract(selectedUnit.TopPlate) || unit.TopPlate == PlateColor.Empty)
                    {
                        tempPlates = selectedUnit.GetPlates(selectedUnit.TopPlate, unit.GetLimit());
                        StartCoroutine(MovePlates(selectedUnit,unit));
                        SoundManager.Instance.PlayOneShot(SoundManager.Instance.stackSelect, 0.5f);
                        Vibration.VibratePop();
                        return;
                    }
                    else
                    {
                        Vibration.VibrateNope();
                        ResetData();
                        return;
                    }

                }
            }
        }
        IEnumerator MovePlates(UnitBase selected,UnitBase target)
        {
            SetBusy();

            for (int i = 0; i < tempPlates.Count; i++)
            {
                //if(target.GetLimit() <= 0) 
                //{
                //    foreach (var item in tempPlates)
                //    {
                //        selected.AddPlate(item);
                //    }
                //    break; 
                //}

                tempPlates[i].MoveAndRotateTowardsDestination(target.GetPosRot(), target.AddPlate);
                //tempPlates.RemoveAt(i);
                //i--;
                yield return new WaitForSeconds(timePerPlate);
            }
            yield return new WaitForSeconds(timePerPlate);
            ResetData();

        }
        private void ResetData()
        {
            if (selectedUnit)
            {
                selectedUnit.ClearUnit();
                selectedUnit = null;
            }
            tempPlates.Clear();
            SetFree();
        }
        private void SetBusy() { _busy = true; }
        private void SetFree() { _busy = false; }
    }
}
