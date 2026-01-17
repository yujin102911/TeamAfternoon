using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class KeywordSpawn_UI : MonoBehaviour, IBeginDragHandler,
     IDragHandler, IEndDragHandler
{
    public KeywordData Keyword_Data;

    [SerializeField]
    private Keyword_UI _keywordUI;
    [SerializeField]
    private TextMeshProUGUI _numTxt;
    [SerializeField]
    private TextMeshProUGUI _descriptTxt;

    [Header("드래그 복제본")]
    public GameObject dragGhostPrefab; // UI 프리팹 복제본
    private GameObject ghost;
    private Canvas canvas;

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
    }

    private void OnDisable()
    {
        if (ghost != null)
            Destroy(ghost);
        ghost = null;
    }

    public void Init(Owned_Keyword_Data keyword_Data)
    {
        Keyword_Data = ServiceLocator.Instance.CurrentRepository.GetKeyword(keyword_Data.Owned_KeywordID);

        // 이름 설정
        _keywordUI.SetAndShow(Keyword_Data.KeywordName);

        // 설명 설정
        _descriptTxt.text = Keyword_Data.KeywordDescription;
        // 보유 개수 설정
        _numTxt.text = "x" + keyword_Data.Keyword_Num.ToString();

    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //TODO: 키워드 사용량이 늘어나면 1을 변수로 바꿔야함
        if (DeckBuildingManager.Instance.TryPickUp_Keyword(Keyword_Data.KeywordID, 1))
        {
            // 드래그용 UI 생성
            ghost = Instantiate(dragGhostPrefab, canvas.transform);
            ghost.transform.position = transform.position;
            ghost.GetComponent<Keyword_UI>().SetAndShow(Keyword_Data.KeywordName);

            // 보유 개수 재설정
            _numTxt.text = "x" + DeckBuildingManager.Instance.Number_buffer.ToString();
        }
        else
        {
            Debug.Log("보유 키워드 부족");
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ghost != null)
            ghost.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 드래그 종료
        if (ghost != null)
            Destroy(ghost);
        ghost = null;

        if(DeckBuildingManager.Instance != null)
        {
            // 키워드 반납
            DeckBuildingManager.Instance.Return_Keyword(Keyword_Data.KeywordID, 1);
        }
    }
}
