using UnityEngine;

/// <summary>
/// 맵 생성 시 필요한 정보를 담고있는 ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "MapConfiguration", menuName = "Game/Map Configuration")]
public class MapConfiguration : ScriptableObject
{
    [Header("Prefabs & Layout")]
    public GameObject sectorPrefab;
    public float sectorSpacing = 0.15f;
    public float baseSectorSize = 1.5f;

    [Header("Colors")]
    public Color normalColor = new Color(0.0f, 0.0f, 0.0f);
    public Color attackColor = new Color(1f, 0.3f, 0.3f);
    public Color selectableColor = new Color(0.5f, 1f, 0.5f);
    public Color hoverColor = new Color(0.7f, 1f, 0.7f);
    public Color cureColor = new Color(0.7f, 1f, 0.7f);
}
