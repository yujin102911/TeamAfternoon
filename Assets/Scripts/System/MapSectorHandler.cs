using TMPro;
using UnityEngine;

/// <summary>
/// MapSystem 대신 실제 게임 오브젝트에 붙어 마우스 입력을 MapSystem으로 전달하는 전달자
/// </summary>
public class MapSectorHandler : MonoBehaviour
{
    private int _sectorNum;
    private MapSystem _system;

    [SerializeField] private TextMeshPro _countText;

    public void Initialize(int num, MapSystem system)
    {
        _sectorNum = num;
        _system = system;
        if (_countText == null)
        {
            _countText = GetComponentInChildren<TextMeshPro>();
        }
        if (_countText != null)
        {
            UpdateText("");
        }
        if (_countText == null)
        {
            Debug.LogError($"[MapSectorHandler] Sector {_sectorNum}: TextMeshPro 컴포넌트를 찾을 수 없습니다! (UI용 TextMeshProUGUI 인지 확인해보세요)");
            return;
        }
        UpdateText("");
    }
    public void UpdateText(string text)
    {
        if (_countText != null)
        {
            _countText.text = text;
        }
    }
    void OnMouseDown() => _system?.HandleSectorClick(_sectorNum);
    void OnMouseEnter() => _system?.HandleSectorHover(_sectorNum, true);
    void OnMouseExit() => _system?.HandleSectorHover(_sectorNum, false);

}
