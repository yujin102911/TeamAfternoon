using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StageLinkHandler : MonoBehaviour
    , IPointerClickHandler, IPointerMoveHandler, IPointerExitHandler
{
    [SerializeField] private string battleSceneName = "battleScene";
    [SerializeField] private AppLauncher _launcher;

    [Header("커서 설정")]
    [SerializeField] private Texture2D _linkCursor;
    [SerializeField] private Vector2 _hotSpot = Vector2.zero;
    [SerializeField] private Color32 _hoverColor = Color.white;
    private Color32 _normalColor = new Color32(88, 101, 242, 255);

    [Header("입장 불가 패널")]
    [SerializeField] private GameObject _cantPanel;
    [SerializeField] private Button _closeCantPanelButton;
    [SerializeField] private Vector2 _panelPosition;
    [SerializeField] private GameObject _cantPopup;

    private TextMeshProUGUI _tmpText;
    private Canvas _canvas;
    private int _currentHoverLinkIndex = -1;

    private void Awake()
    {
        _tmpText = GetComponent<TextMeshProUGUI>();
        _canvas = GetComponent<Canvas>();
        if (_closeCantPanelButton != null)
        {
            _closeCantPanelButton.onClick.AddListener(CloseCantPanel);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        int linkIndex = GetLinkIndex(eventData);
        if (linkIndex != -1)
        {
            string linkID = _tmpText.textInfo.linkInfo[linkIndex].GetLinkID();

            if (linkID.StartsWith("stage_enter:"))
            {
                string stageNumStr = linkID.Split(':')[1];
                if (int.TryParse(stageNumStr, out int stageNum) )
                {
                    HandleStageLinkClick(stageNum);
                }
            }
        }

    }

    private void HandleStageLinkClick(int stageNum)
    {
        UserGameData currentUser = ServiceLocator.Instance.CurrentUser;
        if (currentUser != null && currentUser.IsStageCleared(stageNum))
        {
            OpenCantPanel();
        }
        else
        {
            EnterStage(stageNum);
        }
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        int linkIndex = GetLinkIndex(eventData);

        if (linkIndex != _currentHoverLinkIndex)
        {
            if (_currentHoverLinkIndex != -1)
                SetLinkColor(_currentHoverLinkIndex, _normalColor);

            _currentHoverLinkIndex = linkIndex;
            if (_currentHoverLinkIndex != -1)
            {
                SetLinkColor(_currentHoverLinkIndex, _hoverColor);
                Cursor.SetCursor(_linkCursor, _hotSpot, CursorMode.Auto);
            }
            else
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_currentHoverLinkIndex != -1)
        {
            SetLinkColor(_currentHoverLinkIndex, _normalColor);
            _currentHoverLinkIndex = -1;
        }
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    private void EnterStage(int id)
    {
        Debug.Log($"[LinkHandler] 스테이지 {id}로 이동합니다.");
        GameManager.SelectedStageID = id;
        if (_launcher != null)
        {
            _launcher.LaunchBattle(battleSceneName);
        }
    }

    private int GetLinkIndex(PointerEventData eventData)
    {
        if (_canvas == null)
        {
            _canvas = GetComponentInParent<Canvas>();
        }
        if (_canvas == null)
        {
            return -1;
        }
        Camera cam = (_canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : _canvas.worldCamera;
        return TMP_TextUtilities.FindIntersectingLink(_tmpText, eventData.position, cam);
    }
    private void SetLinkColor(int linkIndex, Color32 color)
    {
        TMP_LinkInfo linkInfo = _tmpText.textInfo.linkInfo[linkIndex];

        for (int i = 0; i < linkInfo.linkTextLength; i++)
        {
            int charIndex = linkInfo.linkTextfirstCharacterIndex + i;
            int meshIndex = _tmpText.textInfo.characterInfo[charIndex].materialReferenceIndex;
            int vertexIndex = _tmpText.textInfo.characterInfo[charIndex].vertexIndex;

            Color32[] vertexColors = _tmpText.textInfo.meshInfo[meshIndex].colors32;

            if (_tmpText.textInfo.characterInfo[charIndex].isVisible)
            {
                vertexColors[vertexIndex + 0] = color;
                vertexColors[vertexIndex + 1] = color;
                vertexColors[vertexIndex + 2] = color;
                vertexColors[vertexIndex + 3] = color;
            }
        }
        _tmpText.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
    }

    #region 기간 만료 패널용 함수 *^^*
    private void CloseCantPanel()
    {
        if (_cantPanel != null)
        {
            _cantPanel.SetActive(false);
        }
    }

    private void OpenCantPanel()
    {
        if (_cantPanel != null)
        {
            _cantPanel.SetActive(true);
            _cantPanel.transform.SetAsLastSibling();
        }
        if (_cantPopup != null)
        {
            RectTransform rt = _cantPopup.GetComponent<RectTransform>();

            rt.anchorMin = _panelPosition;
            rt.anchorMax = _panelPosition;

            rt.anchoredPosition = Vector2.zero;
        }
    }
    #endregion

}
