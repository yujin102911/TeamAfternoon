using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Building_Hand : MonoBehaviour
{
    private RuntimeBlock runtimeBlock;

    [SerializeField] private RectTransform targetChild; // 기준이 될 자식
    [SerializeField] private RectTransform parent;

    [SerializeField]
    private Film_UI _film;
    [SerializeField]
    private TextMeshProUGUI _nameTxt;
    [SerializeField]
    private Button _button;

    private float _lastWidth = -1f;

    public void Init(RuntimeBlock rBlock)
    {
        _lastWidth = -1f;

        runtimeBlock = rBlock;

        _film.Init(rBlock);
        _nameTxt.text = rBlock.BaseData.BlockName;

        _button.onClick.RemoveAllListeners();

        if(IngameBuildingManager.Instance != null)
            _button.onClick.AddListener(() => IngameBuildingManager.Instance.Hand_to_Deck(runtimeBlock));
    }

    private void SyncWidthToChild()
    {
        if (targetChild == null || parent == null)
            return;

        float w = targetChild.rect.width;
        if (Mathf.Abs(_lastWidth - w) < 0.01f)
            return;

        _lastWidth = w;

        Vector2 size = parent.sizeDelta;
        size.x = w;
        parent.sizeDelta = size;
    }

    private void LateUpdate()
    {
        SyncWidthToChild();
    }
}
