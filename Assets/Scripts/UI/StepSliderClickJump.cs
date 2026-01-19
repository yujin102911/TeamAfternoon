using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StepSliderClickJump : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private Slider slider;
    [SerializeField] private RectTransform trackRect;
    [SerializeField] private int steps = 16;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!slider || !slider.interactable || !trackRect) return; 

        var canvas = trackRect.GetComponentInParent<Canvas>();
        Camera cam = null;
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            cam = canvas.worldCamera;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            trackRect, eventData.position, cam, out Vector2 localPos);

        // Track의 좌우 기준 0~1로 정규화
        float width = trackRect.rect.width;
        float normalized = Mathf.Clamp01((localPos.x + width * 0.5f) / width);

        // step 계산
        int step = Mathf.RoundToInt(normalized * (steps - 1));

        Debug.Log($"<color=red>[StepSliderClickJump] 슬라이더 옮기기 / step: {step}</color>");

        // 값 설정
        //slider.SetValueWithoutNotify(step);
        //slider.onValueChanged.Invoke(step);

        slider.value = step;
    }
}
