using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// 맵 크기 열거형
/// </summary>
public enum MapSize
{
    Sectors_8,    // 3x3
    Sectors_12,   // 4x3
}


/// <summary>
/// 맵 생성, 섹터 객체 관리, 입력 감지
/// </summary>
public class MapSystem
{
    private readonly MapConfiguration _config;
    private readonly Transform _rootTransform;

    private bool _isSelectionEnabled = false;
    private int _totalSectors;

    private Dictionary<int, GameObject> _sectors = new Dictionary<int, GameObject>();
    private Dictionary<int, Color> _sectorBaseColors = new Dictionary<int, Color>();

    public event Action<int> OnSectorSelected;

    public int TotalSectors => _totalSectors;

    public MapSystem(MapConfiguration config, Transform rootTransform)
    {
        _config = config;
        _rootTransform = rootTransform;
    }

    #region Map Generation Methods
    /// <summary>
    /// 맵 생성
    /// </summary>
    public void GenerateMap(MapSize mapSize)
    {
        ClearMap();

        List<Vector2Int> gridPositions = new List<Vector2Int>();
        Vector2 gridDimensions = Vector2.zero;
        float currentSectorSize = _config.baseSectorSize;

        switch (mapSize)
        {
            case MapSize.Sectors_8:
            default:
                _totalSectors = 8;
                gridDimensions = new Vector2(3, 3);
                gridPositions.Add(new Vector2Int(0, 2)); gridPositions.Add(new Vector2Int(1, 2));
                gridPositions.Add(new Vector2Int(2, 2)); gridPositions.Add(new Vector2Int(2, 1));
                gridPositions.Add(new Vector2Int(2, 0)); gridPositions.Add(new Vector2Int(1, 0));
                gridPositions.Add(new Vector2Int(0, 0)); gridPositions.Add(new Vector2Int(0, 1));
                break;
            case MapSize.Sectors_12:
                _totalSectors = 12;
                gridDimensions = new Vector2(4, 3);
                gridPositions.Add(new Vector2Int(0, 2)); gridPositions.Add(new Vector2Int(1, 2));
                gridPositions.Add(new Vector2Int(2, 2)); gridPositions.Add(new Vector2Int(3, 2));
                gridPositions.Add(new Vector2Int(0, 1)); gridPositions.Add(new Vector2Int(1, 1));
                gridPositions.Add(new Vector2Int(2, 1)); gridPositions.Add(new Vector2Int(3, 1));
                gridPositions.Add(new Vector2Int(0, 0)); gridPositions.Add(new Vector2Int(1, 0));
                gridPositions.Add(new Vector2Int(2, 0)); gridPositions.Add(new Vector2Int(3, 0));
                break;
        }
        CreateSectorObjects(gridPositions, gridDimensions, currentSectorSize);
        Debug.Log($"[MapSystem] 맵 생성 완료: {mapSize} ({_totalSectors} 섹터)");
    }
    /// <summary>
    /// 맵 생성 시 혹시 모를 오류를 방지하기 위해 맵 클리어함수
    /// </summary>
    private void ClearMap()
    {
        foreach (GameObject sector in _sectors.Values)
            if (sector != null) UnityEngine.Object.Destroy(sector);
        _sectors.Clear();
        _sectorBaseColors.Clear();
    }
    /// <summary>
    /// 섹터 프리팹 생성 함수 (GenerateMap 함수에서 사용)
    /// </summary>
    private void CreateSectorObjects(List<Vector2Int> positions, Vector2 dimensions, float size)
    {
        float totalSpacing = size + _config.sectorSpacing;
        Vector3 centerPos = _rootTransform.position;
        float xOffset = (dimensions.x - 1) / 2.0f;
        float yOffset = (dimensions.y - 1) / 2.0f;

        for (int i = 0; i < _totalSectors; i++)
        {
            int sectorNum = i + 1;
            Vector2Int gridPos = positions[i];

            float x = centerPos.x + (gridPos.x - xOffset) * totalSpacing;
            float y = centerPos.y + (gridPos.y - yOffset) * totalSpacing;
            Vector3 worldPos = new Vector3(x, y, 0);

            GameObject sectorObj;
            if (_config.sectorPrefab != null)
            {
                sectorObj = UnityEngine.Object.Instantiate(_config.sectorPrefab, worldPos, Quaternion.identity, _rootTransform);
                sectorObj.name = $"Sector_{sectorNum}";
                sectorObj.transform.localScale = new Vector3(size * 0.9f, size * 0.9f, 1);

                var handler = sectorObj.AddComponent<MapSectorHandler>();
                handler.Initialize(sectorNum, this);

                _sectors.Add(sectorNum, sectorObj);
                SetSectorBaseColor(sectorNum, _config.normalColor);
            }
            else
            {
                Debug.LogError("섹터 프리팹이 연결되지 않았습니다");
            }
        }
    }
    #endregion

    #region 외부 참조용 함수
    public Vector3 GetSectorPosition(int sectorIndex)
    {
        if (_sectors.TryGetValue(sectorIndex, out GameObject obj))
        {
            return obj.transform.position;
        }
        Debug.LogWarning($"[MapSystem] 존재하지 않는 섹터 {sectorIndex}의 위치를 요청했습니다");
        return Vector3.zero;
    }
    #endregion

    #region Visual State API
    /// <summary>
    /// 섹터의 기본 색상을 변경
    /// 이 함수 호출 시 즉시 색상 바뀜
    /// </summary>
    public void SetSectorBaseColor(int sectorNum, Color color)
    {
        if (_sectorBaseColors.ContainsKey(sectorNum))
            _sectorBaseColors[sectorNum] = color;
        else
            _sectorBaseColors.Add(sectorNum, color);
        // 즉시 반영
        UpdateSectorColor(sectorNum, color);
    }

    // 일시적인 색상으로 설정하는 함수(공격 이펙트, 마우스 오버 등)
    public void SetSectorTempColor(int sectorNum, Color color)
    {
        UpdateSectorColor(sectorNum, color);
    }
    // 일시적인 색상 지우는 함수
    public void ResetSectorColor(int sectorNum)
    {
        if (_sectorBaseColors.TryGetValue(sectorNum, out Color baseColor))
            UpdateSectorColor(sectorNum, baseColor);
        else
            UpdateSectorColor(sectorNum, _config.normalColor);
    }


    private void UpdateSectorColor(int sectorNum, Color color)
    {
        if (_sectors.TryGetValue(sectorNum, out GameObject obj))
        {
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = color;
        }
    }
    #endregion

    #region Input Logic
    public void EnableSelectionMode()
    {
        _isSelectionEnabled = true;
    }

    public void DisableSelectionMode()
    {
        _isSelectionEnabled = false;
        foreach(int key in _sectors.Keys) ResetSectorColor(key);
    }

    public void HandleSectorClick(int sectorNum)
    {
        if (_isSelectionEnabled) 
            OnSectorSelected?.Invoke(sectorNum);
    }

    public void HandleSectorHover(int sectorNum, bool isEnter)
    {
        if (!_isSelectionEnabled) return;

        if (isEnter)
        {
            SetSectorTempColor(sectorNum, _config.hoverColor);
        }
        else
        {
            ResetSectorColor(sectorNum);
        }
    }
    #endregion

}
