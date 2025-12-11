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
/// 맵을 생성하는 시스템이다.
/// </summary>
public class MapSystem
{
    private readonly MapConfiguration _config;
    private readonly Transform _rootTransform;

    private Dictionary<int, GameObject> _sectors = new Dictionary<int, GameObject>();
    private bool _isSelectionEnabled = false;
    private int _totalSectors;

    private Dictionary<int, Color> _sectorOwnerColors = new Dictionary<int, Color>();

    public event Action<int> OnSectorSelected;

    public int TotalSectors => _totalSectors;

    public MapSystem(MapConfiguration config, Transform rootTransform)
    {
        _config = config;
        _rootTransform = rootTransform;
    }

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

    public Vector3 GetSectorPosition(int sectorIndex)
    {
        if (_sectors.TryGetValue(sectorIndex, out GameObject obj))
        {
            return obj.transform.position;
        }
        Debug.LogWarning($"[MapSystem] 존재하지 않는 섹터 {sectorIndex}의 위치를 요청했습니다");
        return Vector3.zero;
    }

    public void EnableSelectionMode()
    {
        _isSelectionEnabled = true;
        foreach (var key in _sectors.Keys)
        {
            if (_sectorOwnerColors.TryGetValue(key, out Color ownerColor))
            {
                UpdateSectorColor(key, ownerColor);
            }
            else
            {
                UpdateSectorColor(key, _config.selectableColor);
            }
        }
    }

    public void DisableSelectionMode()
    {
        _isSelectionEnabled = false;
        ResetAllSectorColors();
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
            // 들어올 땐 하이라이트 색
            UpdateSectorColor(sectorNum, _config.hoverColor);
        }
        else
        {
            // ★ [수정] 나갈 땐 "기억해둔 주인 색"으로 복구!
            if (_sectorOwnerColors.TryGetValue(sectorNum, out Color ownerColor))
            {
                UpdateSectorColor(sectorNum, ownerColor);
            }
            else
            {
                // 기억된 게 없으면 기본 색(설정값)으로
                UpdateSectorColor(sectorNum, _config.selectableColor);
            }
        }
    }

    public void UpdateSectorColor(int sectorNum, Color color)
    {
        if (_sectors.TryGetValue(sectorNum, out GameObject obj))
        {
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = color;
        }
    }

    private void ResetAllSectorColors()
    {
        foreach (var key in _sectors.Keys) UpdateSectorColor(key, _config.normalColor);
    }

    private void ClearMap()
    {
        foreach (GameObject sector in _sectors.Values)
            if (sector != null) UnityEngine.Object.Destroy(sector);
        _sectors.Clear();
    }

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
                UpdateSectorColor(sectorNum, _config.normalColor);
            }
            else
            {
                Debug.LogError("섹터 프리팹이 연결되지 않았습니다");
            }
        }
    }

    /// <summary>
    /// 공격 섹터 표시용 함수
    /// </summary>
    public void HighlightAttackSectors(List<int> sectorIndices)
    {
        foreach (int index in sectorIndices)
        {
            if (_sectors.ContainsKey(index))
            {
                UpdateSectorColor(index, _config.attackColor);
            }
        }
    }
    public void ResetHighlight()
    {
        foreach (var key in _sectors.Keys)
        {
            // 주인 색이 있으면 그걸로, 없으면 기본 색으로 리셋
            if (_sectorOwnerColors.TryGetValue(key, out Color ownerColor))
                UpdateSectorColor(key, ownerColor);
            else
                UpdateSectorColor(key, _config.normalColor);
        }
    }
    public void SetSectorOwnerColor(int sectorNum, Color color)
    {
        if (_sectorOwnerColors.ContainsKey(sectorNum))
            _sectorOwnerColors[sectorNum] = color;
        else
            _sectorOwnerColors.Add(sectorNum, color);

        UpdateSectorColor(sectorNum, color);
    }


}
