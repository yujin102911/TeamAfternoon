using UnityEngine;

/// <summary>
/// MapSystem 대신 실제 게임 오브젝트에 붙어 마우스 입력을 MapSystem으로 전달하는 전달자
/// </summary>
public class MapSectorHandler : MonoBehaviour
{
    private int _sectorNum;
    private MapSystem _system;

    public void Initialize(int num, MapSystem system)
    {
        _sectorNum = num;
        _system = system;
    }
    void OnMouseDown() => _system?.HandleSectorClick(_sectorNum);
    void OnMouseEnter() => _system?.HandleSectorHover(_sectorNum, true);
    void OnMouseExit() => _system?.HandleSectorHover(_sectorNum, false);

}
