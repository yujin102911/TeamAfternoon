using UnityEngine;
using TMPro;

public class CardTooltip : MonoBehaviour
{
    public static CardTooltip Instance { get; private set; }

    [SerializeField] private GameObject root;    // 전체 툴팁 패널
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;

    [Header("위치 오프셋")]
    [SerializeField] private Vector2 offset = new Vector2(0f, -10f); // ← 오른쪽으로 안 밀리게 X=0

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        canvasGroup = root.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = root.AddComponent<CanvasGroup>();

        canvasGroup.blocksRaycasts = false;

        Hide();
    }

    // ✅ 카메라를 같이 받도록 변경
    public void Show(string title, string body, Vector2 screenPos, Camera cam)
    {
        if (titleText != null)
            titleText.text = title;

        if (bodyText != null)
            bodyText.text = body;

        root.SetActive(true);

        RectTransform rt = root.GetComponent<RectTransform>();
        RectTransform parentRt = rt.parent as RectTransform;
        if (parentRt == null) return;

        Vector2 targetScreenPos = screenPos + offset;

        // ⚠️ 여기에서 null 대신 cam 사용
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRt,
            targetScreenPos,
            cam,
            out Vector2 localPos
        );

        rt.anchoredPosition = localPos;

        // 부모 Rect 안으로 클램프
        Vector2 halfSize = rt.rect.size * 0.5f;

        float minX = parentRt.rect.xMin + halfSize.x;
        float maxX = parentRt.rect.xMax - halfSize.x;
        float minY = parentRt.rect.yMin + halfSize.y;
        float maxY = parentRt.rect.yMax - halfSize.y;

        Vector2 clamped = rt.anchoredPosition;
        clamped.x = Mathf.Clamp(clamped.x, minX, maxX);
        clamped.y = Mathf.Clamp(clamped.y, minY, maxY);

        rt.anchoredPosition = clamped;
    }

    public void Hide()
    {
        if (root != null)
            root.SetActive(false);
    }
}
