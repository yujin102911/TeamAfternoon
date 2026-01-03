using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public enum HandSortType { Length, Action }

public enum HandFilterType
{
    Jump,
    Move,
    Melee,
    Ranged,
    Guard,
}

public class FilmHand_Panel : MonoBehaviour
{
    [SerializeField] private GameObject _blockPrefab;  // 풀링할 대상
    [SerializeField] private Transform _spawnPoint;   // 블록이 생성될 위치
    [SerializeField] private int _initialSize = 10;

    private Queue<GameObject> _handPool = new Queue<GameObject>();
    private HandSortType _currentSortType = HandSortType.Length;
    private HashSet<HandFilterType> _activeFilters = new HashSet<HandFilterType>();
    private bool _isShowAll = true;
    private bool _syncing = false;
    [Header("필터 UI 참조")]
    [SerializeField] private Toggle _showAllToggle;
    [SerializeField] private List<Toggle> _filterToggles;

    private void Awake()
    {
        InitializePool();
    }

    void Start()
    {
        if (_showAllToggle != null)
        {
            _showAllToggle.onValueChanged.RemoveAllListeners();
            _showAllToggle.onValueChanged.AddListener(OnAllToggleChanged);
        }

        if (_filterToggles != null)
        {
            for (int i = 0; i < _filterToggles.Count; i++)
            {
                int idx = i;
                var t = _filterToggles[idx];
                if (t == null) continue;

                t.onValueChanged.RemoveAllListeners();
                t.onValueChanged.AddListener((on) => OnFilterToggleChanged(idx, on));
            }
        }

        SetAll(false);

        if (TimelineManager.Instance != null)
        {
            TimelineManager.Instance.OnHandChanged += UpdateHandUI;
            UpdateHandUI(TimelineManager.Instance.CurrentHand);
        }
    }
    private void OnAllToggleChanged(bool isOn)
    {
        if (_syncing) return;
        SetAll(isOn);
    }

    private void OnFilterToggleChanged(int filterIndex, bool isOn)
    {
        if (_syncing) return;

        HandFilterType filter = (HandFilterType)filterIndex;
        if (isOn) _activeFilters.Add(filter);
        else _activeFilters.Remove(filter);

        int total = System.Enum.GetValues(typeof(HandFilterType)).Length;
        bool nowAll = (_activeFilters.Count == total);

        _syncing = true;
        _isShowAll = nowAll;
        if (_showAllToggle != null) _showAllToggle.SetIsOnWithoutNotify(nowAll);
        _syncing = false;

        UpdateHandUI(TimelineManager.Instance.CurrentHand);
    }

    private void OnDestroy()
    {
        if (TimelineManager.Instance != null)
        {
            TimelineManager.Instance.OnHandChanged -= UpdateHandUI;
        }
    }
    private void SetAll(bool isOn)
    {
        _syncing = true;

        _isShowAll = isOn;

        if (isOn)
        {
            _activeFilters.Clear();
            foreach (HandFilterType type in System.Enum.GetValues(typeof(HandFilterType)))
                _activeFilters.Add(type);
        }
        else
        {
            _activeFilters.Clear();
        }

        if (_showAllToggle != null) _showAllToggle.SetIsOnWithoutNotify(isOn);

        if (_filterToggles != null)
            foreach (var t in _filterToggles)
                if (t != null) t.SetIsOnWithoutNotify(isOn);

        _syncing = false;

        UpdateHandUI(TimelineManager.Instance.CurrentHand);
    }

    private void InitializePool()
    {
        for (int i = 0; i < _initialSize; i++)
        {
            GameObject obj = Instantiate(_blockPrefab, _spawnPoint);
            obj.SetActive(false);
            _handPool.Enqueue(obj);
        }
    }

    public GameObject Get()
    {
        Debug.Log($"{_handPool.Count}. 블럭 풀 크기");
        if (_handPool.Count > 0)
        {
            GameObject obj = _handPool.Dequeue();
            obj.SetActive(true);
            return obj;
        }

        // 풀링 오브젝트 부족 시 자동 확장
        GameObject newObj = Instantiate(_blockPrefab, transform);
        return newObj;
    }

    public void Return(GameObject obj)
    {
        if (_handPool.Contains(obj)) return;

        obj.SetActive(false);
        obj.transform.SetParent(_spawnPoint);
        _handPool.Enqueue(obj);
    }

    public void UpdateHandUI(List<RuntimeBlock> hand)
    {
        if (_spawnPoint == null) return;
        for (int i = _spawnPoint.childCount - 1; i >= 0; i--) Return(_spawnPoint.GetChild(i).gameObject);

        var filteredListed = hand.Where(block =>
        {
            //if (_isShowAll) return true;
            if (_activeFilters.Count == 0) return true;
            return _activeFilters.All(f => HasActionType(block.BaseData, f));
        });

        var sortedQuery = filteredListed.OrderByDescending(b => b.IsFavorite);
        if (_currentSortType == HandSortType.Action)
        {
            sortedQuery = sortedQuery.ThenByDescending(b => IsFirstActionInFilter(b.BaseData))
                .ThenBy(b => b.BaseData.GetEffectAt(0))
                .ThenBy(b => b.BaseData.BlockLength);
        }
        else
        {
            sortedQuery = sortedQuery.ThenBy(b => b.BaseData.BlockLength)
                .ThenBy(b => b.BaseData.GetEffectAt(0));
        }

        int ix = 0;
        foreach (RuntimeBlock block in sortedQuery.ToList())
        {
            GameObject go = Get();
            go.transform.SetParent(_spawnPoint, false);
            go.transform.SetSiblingIndex(ix);
            FilmHand_UI uiBlock = go.GetComponent<FilmHand_UI>();
            if (uiBlock != null) uiBlock.Init(block);
            ix++;
        }

    }

    public void UpdateHandUI(IReadOnlyList<RuntimeBlock> hand)
    {
        UpdateHandUI(new List<RuntimeBlock>(hand));
    }

    private List<RuntimeBlock> SortBlocks(List<RuntimeBlock> list, HandSortType type)
    {
        var query = list.OrderByDescending(b => b.IsFavorite);

        if (type == HandSortType.Length)
        {
            return query.ThenBy(b => b.BaseData.BlockLength)
                .ThenBy(b => b.BaseData.GetEffectAt(0))
                .ToList();
        }
        else
        {
            return query.ThenBy(b => b.BaseData.GetEffectAt(0))
                .ThenBy(b => b.BaseData.BlockLength)
                .ToList();
        }
    }

    public void SetSortType(int typeIndex)
    {
        _currentSortType = (HandSortType)typeIndex;
        Debug.Log($"{_currentSortType}이 뭔지");
        UpdateHandUI(new List<RuntimeBlock>(TimelineManager.Instance.CurrentHand));
    }

    private bool HasActionType(BlockData data, HandFilterType filter)
    {
        return filter switch
        {
            HandFilterType.Jump => data.actionTypes.Contains(ActionType.Jump),
            HandFilterType.Move => data.actionTypes.Contains(ActionType.Move),
            HandFilterType.Melee => data.actionTypes.Any(a => a == ActionType.Attack || a == ActionType.Sword_end),
            HandFilterType.Ranged => data.actionTypes.Any(a => a == ActionType.Bow_single || a == ActionType.Bow_end),
            HandFilterType.Guard => data.actionTypes.Contains(ActionType.Guard),
            _ => false
        };
    }

    private bool IsFirstActionInFilter(BlockData data)
    {
        ActionType firstAction = data.GetEffectAt(0);
        foreach (var filter in  _activeFilters)
        {
            if (MatchActionToFilter(firstAction, filter)) return true;
        }
        return false;
    }

    private bool MatchActionToFilter(ActionType action, HandFilterType filter)
    {
        return filter switch
        {
            HandFilterType.Jump => action == ActionType.Jump,
            HandFilterType.Move => action == ActionType.Move,
            HandFilterType.Melee => action == ActionType.Attack || action == ActionType.Sword_end,
            HandFilterType.Ranged => action == ActionType.Bow_end || action == ActionType.Bow_single,
            HandFilterType.Guard => action == ActionType.Guard,
            _ => false
        };
    }

}
