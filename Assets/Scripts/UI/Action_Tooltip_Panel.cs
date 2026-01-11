using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

public class Action_Tooltip_Panel : MonoBehaviour
{
    [Header("호버 설정")]
    [SerializeField] private GameObject root;
    [SerializeField] private RectTransform panelRt;   // root의 RectTransform
    [SerializeField] private Vector2 offset = new Vector2(12f, 12f); // 마우스 기준 오른쪽 위

    [SerializeField]
    private TextMeshProUGUI _nameTxt; // 이름 텍스트 UI
    [SerializeField]
    private LocalizedString _nameText;
    [SerializeField]
    private Action_Desc_Cell[] _descriptionCells;

    private void Clear_Cells()
    {
        for (int i = 0; i < _descriptionCells.Length; i++)
        {
            _descriptionCells[i].gameObject.SetActive(false);
        }
    }

    public void Show(RuntimeBlock r_block, Vector2 screenPos, Camera cam)
    {
        root.SetActive(true);

        // ✅ pivot을 좌하단으로 강제(인스펙터에서 해도 됨)
        panelRt.pivot = Vector2.zero; // (0,0) = 좌하단

        RectTransform parentRt = panelRt.parent as RectTransform;
        if (parentRt == null) return;

        Vector2 targetScreenPos = screenPos + offset;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRt,
            targetScreenPos,
            cam,
            out Vector2 localPos
        );

        // 1) 일단 “좌하단을 마우스+offset에”
        panelRt.anchoredPosition = localPos;

        // 2) 화면(부모 Rect) 밖으로 나가지 않게 클램프
        ClampToParent(panelRt, parentRt);


        Clear_Cells();

        this.gameObject.SetActive(true);

        // 이름 텍스트 설정(추후 
        _nameTxt.text = r_block.BaseData.blockName;

        for (int i = 0; i < r_block.BaseData.blockLength; i++)
        {
            ActionType action = r_block.BaseData.GetEffectAt(i);
            int dam = r_block.BaseData.attackDamage;
            _descriptionCells[i].Update_descriptionCell(action, dam);
        }
    }

    private void ClampToParent(RectTransform rt, RectTransform parentRt)
    {
        // rt는 pivot=(0,0) 기준이므로
        // anchoredPosition은 “좌하단”
        Vector2 pos = rt.anchoredPosition;

        float minX = parentRt.rect.xMin;
        float maxX = parentRt.rect.xMax - rt.rect.width;
        float minY = parentRt.rect.yMin;
        float maxY = parentRt.rect.yMax - rt.rect.height;

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        rt.anchoredPosition = pos;
    }


    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

}
