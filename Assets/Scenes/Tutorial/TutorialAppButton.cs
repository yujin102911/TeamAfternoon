using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TutorialAppButton : MonoBehaviour, IPointerClickHandler, ISelectHandler, IDeselectHandler
{
    [Header("UI 시각 효과")]
    [SerializeField] private Image _selectedImage;    // 클릭 시 강조용 이미지

    [Header("연결된 패널 설정")]
    [SerializeField] private GameObject _applicationPanel; // 열릴 MSN 패널
    [SerializeField] private Vector2 _panelAnchoredPos;    // 패널이 나타날 중앙 좌표 (예: 0, 0)

    private void Awake()
    {
        // 시작 시 선택 강조 이미지는 꺼둠
        if (_selectedImage != null) _selectedImage.gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount == 1)
        {
            OnSingleClick();
        }
        else if (eventData.clickCount == 2)
        {
            OnDoubleClick();
        }
    }

    private void OnSingleClick()
    {
        // 유니티 이벤트 시스템에 이 오브젝트가 선택됨을 알림
        EventSystem.current.SetSelectedGameObject(gameObject);
        Debug.Log($"{gameObject.name} 단일 클릭됨");
    }

    private void OnDoubleClick()
    {
        if (_applicationPanel != null)
        {
            // 1. 패널 활성화
            _applicationPanel.SetActive(true);

            // 2. 패널 위치 설정 (RectTransform 기준 anchoredPosition 사용 권장)
            RectTransform panelRect = _applicationPanel.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                panelRect.anchoredPosition = _panelAnchoredPos;
            }

            // 3. 패널을 블로커 위로 올리기 (Canvas 설정)
            Canvas panelCanvas = _applicationPanel.GetComponent<Canvas>();
            if (panelCanvas == null) panelCanvas = _applicationPanel.AddComponent<Canvas>();

            panelCanvas.overrideSorting = true;
            panelCanvas.sortingOrder = 110; // 블로커(100)보다 높게

            if (_applicationPanel.GetComponent<GraphicRaycaster>() == null)
                _applicationPanel.AddComponent<GraphicRaycaster>();

            // 4. 대화 매니저에게 다음 단계 요청
            DialogueManager dm = FindObjectOfType<DialogueManager>();
            if (dm != null && dm.bubbleObject.activeSelf)
            {
                dm.DisplayNextStep();
            }

            Debug.Log($"{gameObject.name} 더블 클릭: 패널 오픈 및 대사 넘김");
        }
    }

    // --- 선택 효과 (IPointerClickHandler와 연동) ---
    public void OnSelect(BaseEventData eventData)
    {
        if (_selectedImage != null) _selectedImage.gameObject.SetActive(true);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (_selectedImage != null) _selectedImage.gameObject.SetActive(false);
    }
}