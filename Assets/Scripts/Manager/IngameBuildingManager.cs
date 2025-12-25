using UnityEditor.Rendering;
using UnityEngine;

public class IngameBuildingManager : MonoBehaviour
{
    public static IngameBuildingManager Instance;

    [Header("블럭 정보창")]
    [SerializeField] 
    private Unlocked_detailUI detailUI;
    private Unlocked_BlockUI current;
    private BlockData _seleckedBlock;

    private SelectionManager<Unlocked_BlockUI> _selection = new();

    public int? SelectedBlockID = null;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// 블록 페이지 슬롯 클릭 시 호출
    /// </summary>
    public void OnSlotClicked(Unlocked_BlockUI slot)
    {
        bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        if (ctrl)
        {
            _selection.Toggle(slot);
        }
        else
        {
            _selection.SelectSingle(slot);
        }

        UpdateVisuals();
        UpdateDetail();
    }

    private void UpdateVisuals()
    {
        foreach (Unlocked_BlockUI slot in FindObjectsByType<Unlocked_BlockUI>(FindObjectsSortMode.None))
        {
            bool selected = _selection.Selected.Contains(slot);
            slot.SetSelected(selected);
        }
    }

    private void UpdateDetail()
    {
        if (_selection.Focused == null)
        {
            Debug.Log("여기한번함");
            detailUI.Hide();
            return;
        }

        detailUI.Show(_selection.Focused.R_Block.BaseData);
    }
}
