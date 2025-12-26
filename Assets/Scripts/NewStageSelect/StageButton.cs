using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 지금은 스테이지 데이터에서 가져오는게 아닌 버튼이 자기가 읽혔는지 안읽혔는지에 대한 정보를
/// 가지고 있음 
/// 추후에는 스테이지 데이터에 이 내용이 읽혔는지 
/// 안읽혔는지에 대한 정보를 추가하고
/// 그 내용을 갖고올 수 있도록 수정해야함
/// </summary>
public class StageButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _titleText;

    private StageData _data;
    private System.Action<StageData> _onSelect;

    public void Setup(StageData data, System.Action<StageData> onSelect)
    {
        _data = data;
        _onSelect = onSelect;

        _titleText.text = $"[의뢰] {data.StageName}";
        GetComponent<Button>().onClick.AddListener(() => _onSelect?.Invoke(_data));
    }

}
