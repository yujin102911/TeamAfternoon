using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.Mathematics;

/// <summary>
/// 맵 크기 열거형
/// </summary>
public enum MapSize
{
    Sectors_4,
    Sectors_8,    // 3x3
    Sectors_12,   // 4x3
    Grid_3x3,    // 3x3
    Grid_4x3,   // 4x3
    Custom,       // 맵에 설치한 포인트들 기준
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

    private int _columns;
    private int _rows;

    private Dictionary<int, GameObject> _sectors = new Dictionary<int, GameObject>();
    private Dictionary<int, Color> _sectorBaseColors = new Dictionary<int, Color>();

    public event Action<int> OnSectorSelected;

    public int TotalSectors => _totalSectors;
    public int Rows => _rows;
    public int Columns => _columns;

    public MapSystem(MapConfiguration config, Transform rootTransform)
    {
        _config = config;
        _rootTransform = rootTransform;
    }

    #region Map Generation Methods
    /// <summary>
    /// 맵 생성
    /// </summary>
    public void GenerateMap(MapSize mapSize, List<Vector3> customPositions = null)
    {
        ClearMap();

        if (mapSize == MapSize.Custom)
        {
            if (customPositions != null && customPositions.Count > 0)
            {
                GenerateCustomMap(customPositions);
                return;
            }
            else
            {
                // 설정된게 없으면 sector8 을 기본으로 걍 설정
                Debug.LogError("[MapSystem] Custom모드지만 배치 포인트 리스트가 비어있음");
                mapSize = MapSize.Grid_3x3;
            }
        }

        List<Vector2Int> gridPositions = new List<Vector2Int>();
        float currentSectorSize = _config.baseSectorSize;

        switch (mapSize)
        {
            case MapSize.Grid_3x3:
                _columns = 3;
                _rows = 3;
                break;
            case MapSize.Grid_4x3:
                _columns = 4;
                _rows = 3;
                break;
            case MapSize.Sectors_4:
                _totalSectors = 4;
                gridPositions.Add(new Vector2Int(0, 1)); gridPositions.Add(new Vector2Int(1, 1));
                gridPositions.Add(new Vector2Int(1, 0)); gridPositions.Add(new Vector2Int(0, 0));
                break;

            case MapSize.Sectors_8:
            default:
                _columns = 3;
                _rows = 3;
                break;
        }
        _totalSectors = _columns * _rows;
        for (int y = _rows - 1; y >= 0; y--) 
        {
            for (int x = 0; x < _columns; x++) 
            {
                gridPositions.Add(new Vector2Int(x, y));
            }
        }
        Vector2 dimensions = new Vector2(_columns, _rows);
        New_CreateSectorObjects(gridPositions, dimensions, currentSectorSize);
        Debug.Log($"[MapSystem] 맵 생성 완료: {mapSize} ({_totalSectors} 섹터)");
    }

    private void GenerateCustomMap(List<Vector3> localPositions)
    {
        _totalSectors = localPositions.Count;
        float size = _config.baseSectorSize;

        for (int i = 0; i < _totalSectors; i++)
        {
            int sectorNum = i + 1;
            Vector3 localPos = localPositions[i];

            GameObject sectorObj;
            if (_config.sectorPrefab != null)
            {
                sectorObj = UnityEngine.Object.Instantiate(_config.sectorPrefab, _rootTransform);
                
                sectorObj.transform.localPosition = localPos;

                sectorObj.name = $"Sector_{sectorNum}";
                sectorObj.transform.localScale = new Vector3(size * 0.9f, size * 0.9f, 1);

                FloatObject floater = sectorObj.AddComponent<FloatObject>();
                floater.floatStrength = 0.07f;
                floater.floatSpeed = UnityEngine.Random.Range(0.8f, 1.2f);

                //MapSectorHandler handler = sectorObj.AddComponent<MapSectorHandler>();
                //handler.Initialize(sectorNum, this);

                _sectors.Add(sectorNum, sectorObj);
                SetSectorBaseColor(sectorNum, _config.normalColor);
            }
        }
        Debug.Log($"[MapSystem] Custom 맵 생성 완료 ({_totalSectors} 섹터)");
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

                //FloatObject floater = sectorObj.AddComponent<FloatObject>();
                //floater.floatStrength = 0.15f;
                //floater.floatSpeed = UnityEngine.Random.Range(0.8f, 1.2f);

                //var handler = sectorObj.AddComponent<MapSectorHandler>();
                //handler.Initialize(sectorNum, this);

                _sectors.Add(sectorNum, sectorObj);
                SetSectorBaseColor(sectorNum, _config.normalColor);
            }
            else
            {
                Debug.LogError("섹터 프리팹이 연결되지 않았습니다");
            }
        }
    }

    private void New_CreateSectorObjects(List<Vector2Int> positions, Vector2 dimensions, float size)
    {
        if (_config.sectorPrefab == null)
        {
            Debug.LogError("섹터 프리팹이 연결되지 않았습니다");
            return;
        }

        // 프리팹의 실제 Sprite 크기 가져오기
        SpriteRenderer prefabSR = _config.sectorPrefab.GetComponent<SpriteRenderer>();
        if (prefabSR == null)
        {
            Debug.LogError("섹터 프리팹에 SpriteRenderer가 없습니다");
            return;
        }

        Vector2 tileSize = prefabSR.bounds.size; // ⭐ 실제 월드 단위 크기
        Vector3 centerPos = _rootTransform.position;

        // 중심 보정값
        float xOffset = (dimensions.x - 1) * 0.5f;
        float yOffset = (dimensions.y - 1) * 0.5f;

        for (int i = 0; i < _totalSectors; i++)
        {
            int sectorNum = i + 1;
            Vector2Int gridPos = positions[i];

            float x = centerPos.x + (gridPos.x - xOffset) * tileSize.x;
            float y = centerPos.y + (gridPos.y - yOffset) * tileSize.y;

            Vector3 worldPos = new Vector3(x, y, 0f);

            GameObject sectorObj = UnityEngine.Object.Instantiate(
                _config.sectorPrefab,
                worldPos,
                Quaternion.identity,
                _rootTransform
            );

            sectorObj.name = $"Sector_{sectorNum}";

            _sectors.Add(sectorNum, sectorObj);
            SetSectorBaseColor(sectorNum, _config.normalColor);
        }
    }

    #endregion

    #region Grid Navigation Logic (이동 계산용)
    public Vector2Int GetGridCoords(int sectorNum)
    {
        int index = sectorNum - 1;
        int rowFromTop = index / _columns; 
        int col = index % _columns; 

        return new Vector2Int(col, (_rows - 1) - rowFromTop);
    }

    public int GetSectorNumFromCoords(int x, int y)
    {
        if (x < 0 || x >= _columns || y < 0 || y >= _rows) return -1;

        int rowFromTop = (_rows - 1) - y;
        return (rowFromTop * _columns) + x + 1;
    }
    /// <summary>
    /// 특정 섹션에서 상하좌우 인접한 섹터 번호 반환
    /// </summary>
    public int GetNeighborSector(int currentSector, Vector2Int direction)
    {
        Vector2Int coords = GetGridCoords(currentSector);
        return GetSectorNumFromCoords(coords.x + direction.x, coords.y + direction.y);
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

    public Transform GetSectorTransform(int sectorNum)
    {
        if (_sectors.TryGetValue(sectorNum, out GameObject obj))
            return obj.transform;
        return null;
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

    public void PlaySectorParticle(int sectorNum)
    {
        if (_sectors.TryGetValue(sectorNum, out GameObject obj))
        {
            ParticleSystem ps = obj.GetComponentInChildren<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
            }
        }
    }

    // 지정된 섹터의 화면상의 좌표 반환
    public Vector2 GetSector_Pos_ToScreen(int sectorNum)
    {
        _sectors.TryGetValue(sectorNum, out GameObject obj);

        if (obj == null)
            return Vector2.zero;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(obj.transform.position);

        // 화면 중심 기준으로 변환
        Vector2 centeredPos = new Vector2(
            screenPos.x - Screen.width * 0.5f,
            screenPos.y - Screen.height * 0.5f
        );

        return centeredPos;
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

    public void Handle_SetSector(int sectorNum)
    {
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
    public void SetSectorText(int sectorNum, string text)
    {
        if (_sectors.TryGetValue(sectorNum, out GameObject obj))
        {
            var handler = obj.GetComponent<MapSectorHandler>();
            if (handler != null)
            {
                handler.UpdateText(text);
            }
        }
    }
}
