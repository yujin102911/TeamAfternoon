using UnityEngine;

public class UIFollowWorldTarget : MonoBehaviour
{
    [SerializeField] private Transform target;   // Enemy Anchor
    [SerializeField] private Vector2 offset;
    [SerializeField] private Camera cam;
    [SerializeField] private Canvas canvas;

    private RectTransform rectTransform;
    private RectTransform canvasRect;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasRect = canvas.GetComponent<RectTransform>();

        if (cam == null)
            cam = Camera.main;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 screenPos = cam.WorldToScreenPoint(target.position);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : cam,
            out Vector2 localPos
        );

        rectTransform.anchoredPosition = localPos + offset;
    }

    public void SetTarget(Transform newTarget, Vector2 hp_offset)
    {
        target = newTarget;
        offset = hp_offset;
    }
}
