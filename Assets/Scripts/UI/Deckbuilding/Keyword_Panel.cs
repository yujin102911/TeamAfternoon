using System.Collections.Generic;
using UnityEngine;

public class Keyword_Panel : MonoBehaviour
{
    [Header("풀링 세팅")]
    [SerializeField] private GameObject _keywordPrefab;  // 풀링할 대상
    [SerializeField] private Transform _spawnPoint;   // 블록이 생성될 위치
    [SerializeField] private int _initialSize = 10;

    private Queue<GameObject> _keywordPool = new Queue<GameObject>();

    private void Awake()
    {
        InitializePool();
    }

    void Start()
    {
        // 일단 한번 생성
        DeckBuildingManager.Instance.OnKeywordChanged += UpdateKeywordUI;
        UpdateKeywordUI(DeckBuildingManager.Instance.UserGameData.Owned_Keywords);

    }

    private void OnDestroy()
    {
        DeckBuildingManager.Instance.OnKeywordChanged -= UpdateKeywordUI;
    }

    private void InitializePool()
    {
        for (int i = 0; i < _initialSize; i++)
        {
            GameObject obj = Instantiate(_keywordPrefab, _spawnPoint);
            obj.SetActive(false);
            _keywordPool.Enqueue(obj);
        }
    }

    public GameObject Get()
    {
        if (_keywordPool.Count > 0)
        {
            GameObject obj = _keywordPool.Dequeue();
            obj.SetActive(true);
            return obj;
        }

        // 풀링 오브젝트 부족 시 자동 확장
        GameObject newObj = Instantiate(_keywordPrefab, transform);
        return newObj;
    }

    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        _keywordPool.Enqueue(obj);
    }

    // 키워드 UI 업데이트
    public void UpdateKeywordUI(List<Owned_Keyword_Data> keyword_Datas)
    {
        if (_spawnPoint == null) return;

        // 기존 UI 반환
        for (int i = _spawnPoint.childCount - 1; i >= 0; i--)
        {
            Return(_spawnPoint.GetChild(i).gameObject);
        }

        foreach (Owned_Keyword_Data data in keyword_Datas)
        {
            //보유중인 키워드만 출력
            if (data.Keyword_Num == 0)
            {
                continue;
            }

            GameObject go = Get();
            KeywordSpawn_UI uiBlock = go.GetComponent<KeywordSpawn_UI>();

            uiBlock.Init(data);
        }
    }
}
