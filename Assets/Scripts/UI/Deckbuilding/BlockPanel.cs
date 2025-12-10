using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DeckPanel : MonoBehaviour
{
    [SerializeField] private GameObject _blockPrefab;  // 풀링할 대상
    [SerializeField] private Transform _spawnPoint;   // 블록이 생성될 위치
    [SerializeField] private int _initialSize = 10;

    private Queue<GameObject> _blockPool = new Queue<GameObject>();

    private void Awake()
    {
        InitializePool();
    }

    void Start()
    {
        // 일단 한번 생성
        DeckBuildingManager.Instance.OnDeckChanged += UpdateDeckBlockUI;
        UpdateDeckBlockUI(DeckBuildingManager.Instance.UserGameData.Deck_Block_IDs);
    }

    private void OnDestroy()
    {
        DeckBuildingManager.Instance.OnDeckChanged -= UpdateDeckBlockUI;
    }

    private void InitializePool()
    {
        for (int i = 0; i < _initialSize; i++)
        {
            GameObject obj = Instantiate(_blockPrefab, _spawnPoint);
            obj.SetActive(false);
            _blockPool.Enqueue(obj);
        }
    }

    public GameObject Get()
    {
        if (_blockPool.Count > 0)
        {
            GameObject obj = _blockPool.Dequeue();
            obj.SetActive(true);
            return obj;
        }

        // 풀링 오브젝트 부족 시 자동 확장
        GameObject newObj = Instantiate(_blockPrefab, transform);
        return newObj;
    }

    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        _blockPool.Enqueue(obj);
    }

    // 덱 UI 업데이트
    public void UpdateDeckBlockUI(List<int> block_IDs)
    {
        if (_spawnPoint == null) return;

        // 기존 손패 UI 반환
        for (int i = _spawnPoint.childCount - 1; i >= 0; i--)
        {
            Return(_spawnPoint.GetChild(i).gameObject);
        }

        foreach (int id in block_IDs)
        {
            GameObject go = Get();
            Deck_UI uiBlock = go.GetComponent<Deck_UI>();
            RuntimeBlock block = new RuntimeBlock(DataRepository.Instance.GetBlock(id));
            
            if (uiBlock != null)
            {

                uiBlock.R_Block = block;
                uiBlock.Init(block);
            }
        }
    }

    //TODO: 칸수에 맞는 블럭만 보여주기

    public void UpdateHandUI(IReadOnlyList<RuntimeBlock> hand)
    {
        //UpdateHandUI(new List<RuntimeBlock>(hand));
    }
}
