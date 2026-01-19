using UnityEngine;

public class FloatObject : MonoBehaviour
{
    [Header("설정")]
    [Tooltip("얼마나 높이 움직일건지")]
    public float floatStrength = 20.0f;
    [Tooltip("얼마나 빨리 움직일건지")]
    public float floatSpeed = 1.0f;

    private RectTransform rectTransform;
    private Vector2 startAnchoredPos;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        startAnchoredPos = rectTransform.anchoredPosition;
    }

    private void Update()
    {
        float newY = startAnchoredPos.y + (Mathf.Sin(Time.time * floatSpeed) * floatStrength);
        rectTransform.anchoredPosition = new Vector2(startAnchoredPos.x, newY);
    }

}
