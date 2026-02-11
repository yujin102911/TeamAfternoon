using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Settings")]
    public TextMeshProUGUI buttonText;
    public float normalScale = 1.0f;
    public float hoverScale = 1.1f;
    public float smoothSpeed = 10f; // 값이 클수록 더 빠르게 변합니다.

    private float _targetScale;
    private float _currentScale;

    void Start()
    {
        if (buttonText == null) buttonText = GetComponentInChildren<TextMeshProUGUI>();

        _targetScale = normalScale;
        _currentScale = normalScale;
    }

    void Update()
    {
        // 현재 크기에서 목표 크기로 부드럽게 보간
        _currentScale = Mathf.Lerp(_currentScale, _targetScale, Time.deltaTime * smoothSpeed);

        // 텍스트의 Scale에 적용
        if (buttonText != null)
        {
            buttonText.transform.localScale = Vector3.one * _currentScale;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _targetScale = hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _targetScale = normalScale;
    }

}
