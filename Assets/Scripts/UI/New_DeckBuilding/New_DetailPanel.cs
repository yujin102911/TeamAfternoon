using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class New_DetailPanel : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _nameTxt; // 이름 텍스트 UI
    [SerializeField]
    private Film_UI Film_UI;
    [SerializeField]
    private Description_Cell[] _descriptionCells;

    private Dictionary<ActionType, int> _actionCount = new Dictionary<ActionType, int>();

    private void Clear_Cells()
    {
        for (int i = 0; i < _descriptionCells.Length; i++)
        {
            _descriptionCells[i].gameObject.SetActive(false);
        }
    }

    public void Show(RuntimeBlock r_block)
    {
        _actionCount.Clear();
        Clear_Cells();

        this.gameObject.SetActive(true);

        // 틱 정보 출력
        Film_UI.Show(r_block);

        // 이름 텍스트 설정
        _nameTxt.text = r_block.BaseData.blockName;

        for(int i = 0; i < r_block.BaseData.blockLength; i++)
        {
            ActionType action = r_block.BaseData.GetEffectAt(i);
            AddAction(action);
        }


        int index = 0;
        foreach (var pair in _actionCount)
        {
            if (pair.Value != 0)
            {
                ActionType key = pair.Key;

                _descriptionCells[index].Update_descriptionCell(key);
            }
            index++;
        }
    }
    public void Hide()
    {
        this.gameObject.SetActive(false);
    }


    void AddAction(ActionType type)
    {
        if (_actionCount.ContainsKey(type))
            _actionCount[type]++;
        else
            _actionCount[type] = 1;
    }

}
