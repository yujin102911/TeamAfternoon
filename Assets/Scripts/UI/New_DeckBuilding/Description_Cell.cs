using Sirenix.OdinInspector;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Description_Cell : MonoBehaviour
{
    [Header("수정할 UI연결")]
    [SerializeField]
    private Image _colorBack;
    [SerializeField]
    private Image _icon;
    [SerializeField]
    private TextMeshProUGUI _nameText;
    [SerializeField]
    private TextMeshProUGUI _descriptionText;

    [TabGroup("Attack")]
    public Sprite Attack_back;
    [TabGroup("Attack")]
    public Sprite Sword_icon;

    [TabGroup("Move")]
    public Sprite Move_back;
    [TabGroup("Move")]
    public Sprite Shoes_icon;

    [TabGroup("Bow")]
    public Sprite Bow_back;
    [TabGroup("Bow")]
    public Sprite Bow_startIcon;

    public void Clear()
    {
        _nameText.text = "";
        _descriptionText.text = "";
    }

    // TODO: 설명들은 나중에 SO로 따로 빼기
    public void Update_descriptionCell(ActionType action)
    {
        Clear();

        this.gameObject.SetActive(true);

        switch (action)
        {
            case ActionType.Attack:
                _colorBack.sprite = Attack_back;
                _icon.sprite = Sword_icon;
                _nameText.text = "근거리 공격";
                _descriptionText.text = "적과 가장 가까운 열에서만 데미지가 들어갑니다.";
                break;

            case ActionType.Move:
                _colorBack.sprite = Move_back;
                _icon.sprite = Shoes_icon;
                _nameText.text = "이동";
                _descriptionText.text = "지정한 방향으로 1칸 이동합니다.";
                break;

            case ActionType.Bow_single:
            case ActionType.Bow_start:
                _colorBack.sprite = Bow_back;
                _icon.sprite = Bow_startIcon;
                _nameText.text = "원거리 공격";
                _descriptionText.text = "거리에 관계없이 적에게 피해를 줍니다.\n앞의 차징이 길수록 데미지가 강해집니다.";
                break;

            default:
                this.gameObject.SetActive(false);
                break;
        }
    }
}
