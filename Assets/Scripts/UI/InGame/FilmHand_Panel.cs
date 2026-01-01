using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum HandSortType { Length, Action }

public class FilmHand_Panel : MonoBehaviour
{
    [SerializeField] private GameObject _blockPrefab;  // 풀링할 대상
    [SerializeField] private Transform _spawnPoint;   // 블록이 생성될 위치
    [SerializeField] private int _initialSize = 10;

    private Queue<GameObject> _handPool = new Queue<GameObject>();
    private HandSortType _currentSortType = HandSortType.Length;

    private void Awake()
    {
        InitializePool();
    }

    void Start()
    {

        if (TimelineManager.Instance != null)
        {
            TimelineManager.Instance.OnHandChanged += UpdateHandUI;
            UpdateHandUI(TimelineManager.Instance.CurrentHand);
        }
    }

    private void OnDestroy()
    {
        if (TimelineManager.Instance != null)
        {
            TimelineManager.Instance.OnHandChanged -= UpdateHandUI;
        }
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
        if (_handPool.Contains(obj)) return; // ⭐ 중복 방지

        obj.SetActive(false);
        obj.transform.SetParent(_spawnPoint); // 부모 정리 (선택)
        _handPool.Enqueue(obj);
    }

    public void UpdateHandUI(List<RuntimeBlock> hand)
    {
        if (_spawnPoint == null) return;

        // 기존 손패 UI 반환
        for (int i = _spawnPoint.childCount - 1; i >= 0; i--)
        {
            Return(_spawnPoint.GetChild(i).gameObject);
        }

        List<RuntimeBlock> sortedHand = SortBlocks(hand, _currentSortType);

        int ix = 0;
        foreach (RuntimeBlock block in sortedHand)
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
}
