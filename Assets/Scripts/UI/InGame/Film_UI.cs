using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class Film_UI : MonoBehaviour
{
    public RuntimeBlock runtimeBlock;

    [Header("참조")]
    public GameObject[] _filmCells;        // 틱 셀 프리팹

    public virtual void Init(RuntimeBlock rBlock)
    {
        runtimeBlock = rBlock;

        UpdateCellVisuals(rBlock.BaseData);
    }

    public void Show(RuntimeBlock rBlock)
    {
        UpdateCellVisuals(rBlock.BaseData);
    }

    private void UpdateCellVisuals(BlockData data)
    {
        if (_filmCells == null) return;

        // 기존 틱 셀 비활성화
        foreach (GameObject tickcell in _filmCells) tickcell.SetActive(false);

        for (int i = 0; i < data.blockLength; i++)
        {
            GameObject cell = _filmCells[i];

            if (!cell.activeSelf) cell.SetActive(true);

            ActionType action = data.GetEffectAt(i);
            MoveDirection dir = data.MoveDirections[i];
            int damage = data.attackDamage;

            cell.GetComponent<Action_cell>().Update_CellVisual(action, dir, damage);


        }
    }


}
