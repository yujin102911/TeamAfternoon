using System;
using TMPro;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Additional_effect_UI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler,
     IDragHandler, IEndDragHandler
{
    public Additional_Effect Additional_Effect;

    [SerializeField]
    private Additional_EffectCell cell;

    [Header("드래그 복제본")]
    public GameObject dragGhostPrefab; // UI 프리팹 복제본
    private GameObject ghost;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    [Header("마우스 호버 설정")]
    [SerializeField]
    private Image _image;
    private Color _originColor;
    private bool _isPointerOver = false;

    [Header("텍스트 설정")]
    [SerializeField]
    private TextMeshProUGUI costText;
    [SerializeField]
    private TextMeshProUGUI nameText;

    private RectTransform rectTransform;

    private void Awake()
    {
        if (_image == null)
            _image = GetComponent<Image>();

        _originColor = _image.color;

        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        //if(TimelineManager.Instance.Combo < Additional_Effect.cost)
        //{
        //    canvasGroup.alpha = 0.5f;
        //    canvasGroup.blocksRaycasts = false;
        //}
        //else
        //{
        //    canvasGroup.alpha = 1f;
        //    canvasGroup.blocksRaycasts = true;
        //}
    }

    private void OnDisable()
    {
        Destroy(ghost);
        ghost = null;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
    }

    public void Init(Additional_Effect _Effect)
    {
        Additional_Effect = _Effect;
        cell.Update_CellVisual(Additional_Effect);

        nameText.text = Additional_Effect.effectName;
        costText.text = $"메모리 {Additional_Effect.cost}소모";
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Additional_Effect == null) return;
        if (eventData.pointerDrag != null) return;

        _isPointerOver = true;

        // 색상 변경
        _image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Set_Descript_Text();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isPointerOver = false;
        _image.color = _originColor;

        if (CardTooltip.Instance != null)
        {
            CardTooltip.Instance.Hide();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (GameManager.Instance.IsExecutingRound) return;

        // 드래그용 복제 생성
        ghost = Instantiate(dragGhostPrefab, canvas.transform);
        ghost.transform.position = transform.position;

        // 드래그 복제본 초기화 세팅
        ghost.GetComponent<Additional_EffectCell>().Update_CellVisual(Additional_Effect);


        // 원본은 숨기기 or 투명화
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ghost != null)
            ghost.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 드래그 종료
        Destroy(ghost);
        ghost = null;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        if (BattleUIManager.Instance != null)
            BattleUIManager.Instance.HandleBar_raycastOn();
    }

    private void Set_Descript_Text()
    {
        if(CardTooltip.Instance != null)
        {
            string title = Additional_Effect.effectName;
            string body = Additional_Effect.effectDescription;

            // 캔버스에 연결된 카메라 사용 (Screen Space - Camera 대응)
            Camera cam = canvas != null ? canvas.worldCamera : Camera.main;

            // 카드 Rect의 오른쪽 중앙 월드 좌표
            Vector3 worldBottomCenter = rectTransform.TransformPoint(
                new Vector3(rectTransform.rect.width * 0.6f, rectTransform.rect.height * 0.5f, 0f)
            );

            // 월드 → 스크린 좌표
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, worldBottomCenter);

            CardTooltip.Instance.Show(title, body, screenPos, Camera.main);
        }
    }
}
